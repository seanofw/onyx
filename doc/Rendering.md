# Rendering

## Overview

The rendering layer is Onyx's interface boundary between the layout engine and the backend that actually puts pixels on screen. It is intentionally narrow: a handful of interfaces that express what the layout engine needs to say without prescribing how a backend implements it. The interfaces live in `Onyx.Rendering` (in the core `Onyx` package); the implementations live in `Onyx.Skia` or any other renderer package.

The entire surface area is:

| Type | Kind | Role |
|---|---|---|
| `IRenderer` | interface | Draw primitives onto a surface |
| `IRenderables` | interface | Factory for backend-owned rendering objects |
| `DrawStyle` | class | Immutable bundle of style parameters passed to every draw call |
| `IBrush` | interface | Opaque handle to a backend-managed paint source |
| `IClipper` | interface | Composable clip region |
| `IFont` | interface | Live font object capable of measuring text |
| `IImage` | interface | Handle to a decoded image |
| `FontInfo` | struct | A font request (name, size, style, weight) |
| `FontMetrics` | struct | Per-font metric data (ascent, descent, em/en/ex, etc.) |
| `TextMetrics` | struct | Per-string measurement result (size, advance, bounds) |
| `LineStyle` | class | Stroke geometric style: dash pattern, cap, join, and miter limit |
| `LineCap` | enum | How open stroke endpoints are rendered |
| `LineJoin` | enum | How corners are rendered where two stroke segments meet |

## The Two Interfaces: IRenderer and IRenderables

Drawing has two distinct concerns, and they are split into two interfaces deliberately.

**`IRenderer`** — the drawing surface itself. It knows how to paint primitives: rects, polygons, text, images, round rects, lines. Everything it draws is described by a `DrawStyle`.

```csharp
public interface IRenderer
{
    void Begin();
    void End();
    void Clear(Color32 color);

    void FillRect(Rect2d rect, DrawStyle style);
    void FillRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style);
    void DrawRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style);
    void DrawLines(ReadOnlySpan<Vector2d> points, bool closePolygon, DrawStyle style);
    void FillPolygon(ReadOnlySpan<Vector2d> points, DrawStyle style);
    void DrawText(Vector2d topLeftCorner, ReadOnlySpan<char> text, DrawStyle style);
    void DrawImage(IImage image, Rect2d sourceRect, Vector2d dest, DrawStyle style);
}
```

**`IRenderables`** — a factory for the objects that draw calls consume. It creates brushes, clippers, fonts, and images—things that are allocated once, used across many draw calls, and eventually disposed.

```csharp
public interface IRenderables
{
    IBrush CreateLinearGradientBrush(LinearGradient linearGradient);
    IBrush CreateRadialGradientBrush(RadialGradient radialGradient);
    IImage? CreateImage(string url);
    IClipper CreateClipper(ReadOnlySpan<Vector2d> convexPolygon);
    IFont? CreateFont(FontInfo fontInfo, bool exactMatchOnly = false);
}
```

The split is also a performance contract, and understanding it makes implementing a renderer straightforward:

- **`IRenderables` methods are slow by assumption.** They are called upfront, rarely, and outside of render loops. A call to `CreateFont` may load and decode a font file. A call to `CreateLinearGradientBrush` may compile a GPU shader. Taking time to do the work correctly is acceptable here; the caller has already committed to paying the cost.

- **`IRenderer` methods must be fast.** They are called inside tight rendering loops, potentially many thousands of times per frame. A draw call must do as little work as possible: read the `DrawStyle`, paint the primitive, return. Any non-trivial setup that could be amortized belongs in `IRenderables`, not here.

If a method belongs on `IRenderer`, it is a draw call and must run fast. If a method belongs on `IRenderables`, it is resource allocation and may take time. This distinction is the primary guide for how to partition work when implementing a new backend.

A concrete renderer (e.g., `SkiaRenderer`) implements both interfaces on the same object, but that is a convenience of the Skia backend, not a requirement. The layout engine will receive `IRenderer` and `IRenderables` as **separate parameters**. A host that finds it cleaner to implement them on different objects—a dedicated resource manager paired with a drawing surface, for example—is free to do so. The interfaces are separated precisely to keep that option open. The constraint is only that the objects come from the same backend: you cannot mix an `IClipper` created by one `IRenderables` implementation with an `IRenderer` from a different one, since the handle types are backend-specific.

## DrawStyle: The Style Bundle

Every draw call takes a `DrawStyle`. Rather than threading individual style parameters through every call, all rendering context is bundled into a single immutable value that travels with the draw call.

```csharp
public class DrawStyle
{
    public IClipper?   Clip           { get; }
    public double      Opacity        { get; }
    public IFont?      Font           { get; }
    public Color32?    Color          { get; }
    public IBrush?     Brush          { get; }
    public Rect2d      BrushRect      { get; }
    public double      LineThickness  { get; }
    public LineStyle   LineStyle      { get; }
    public Matrix3x2d  Transform      { get; }
}
```

`DrawStyle` is immutable. The `With*` methods produce new instances:

```csharp
DrawStyle style = DrawStyle.Default
    .WithColor(Color32.Red)
    .WithLineThickness(2.0)
    .WithOpacity(0.8);
```

`DrawStyle.Default` is a canonical starting point: black, 1px solid, fully opaque, identity transform, no clip, no font, infinite brush rect.

### Color vs. Brush: Mutual Exclusion

`Color` and `Brush` are mutually exclusive. When you set a color (via `WithColor`), the brush is cleared; when you set a brush (via `WithBrush`), the color is cleared. The constructor enforces this: `Brush = color.HasValue ? null : brush`. A draw call is always painted with exactly one paint source: a flat `Color32` or a `Brush`.

This simplifies renderer implementations. At paint time, the renderer checks `Color.HasValue`—if so, paint with that color; if not, look at `Brush`.

### BrushRect: Connecting Layout Geometry to Brush Geometry

A brush like a linear gradient is defined in abstract terms (e.g., "go from blue to red, top to bottom"). To actually paint it, the renderer needs to know what rectangle the gradient spans. That's `BrushRect`.

The layout engine knows the geometry it's painting. When it issues a draw call with a brush, it also provides the bounding rectangle of the element being painted. The brush implementation uses that rectangle to anchor the gradient (or other paint effect) to the right area of screen space.

`DrawStyle.WithBrush(brush, rect)` is the common call: it sets the brush and its rect together, since they describe the same thing.

## IBrush: An Opaque Handle

```csharp
public interface IBrush : IDisposable { }
```

`IBrush` is deliberately almost empty. The core knows nothing about how a brush works—it only holds a reference to one and passes it through `DrawStyle`. The actual painting logic lives in the backend.

In `Onyx.Skia`, `ISkiaBrush` extends `IBrush` with `Apply(SKPaint paint, Rect2d rect)`, which the `SkiaRenderer` calls when it needs to paint with the brush. This is an internal detail of the Skia backend; the core never sees it.

The consequence is that brushes are opaque across the assembly boundary. The layout engine creates a brush via `IRenderables.CreateLinearGradientBrush(...)`, puts it in a `DrawStyle`, and passes it to `IRenderer`. What happens to it there is none of the layout engine's concern. The only universal contracts are: brushes are `IDisposable` (you must release them when done), and passing a brush from one backend to another is not supported and will throw.

## IClipper: Composable Clip Regions

```csharp
public interface IClipper : IDisposable
{
    IClipper Union(IEnumerable<IClipper> others);
    IClipper Intersect(IEnumerable<IClipper> others);
    IClipper Transform(Matrix3x2d transform);
}
```

A clipper is a geometric mask. Pixels outside the clip region are not drawn. Clippers are composable: `Union` and `Intersect` combine multiple clippers into one. `Transform` moves or scales a clipper with an affine matrix.

All three operations return new clippers. The originals are unchanged.

Clippers are created via `IRenderables.CreateClipper(convexPolygon)`. The polygon is expected to be convex; implementations may assume this for efficiency.

CSS has several clipping models: `overflow: hidden` clips children to the padding box, `clip-path` clips to an arbitrary shape, and stacking contexts can have their own clip. The layout engine composes these into a final clip region before issuing draw calls.

Like brushes, the actual clip implementation is backend-specific and the core does not look inside.

## IFont, FontInfo, FontMetrics, TextMetrics

Text rendering has two phases: measurement and drawing. Both require a live font object.

### FontInfo: The Request

`FontInfo` is a value type that describes the font you want:

```csharp
public readonly struct FontInfo
{
    public string    Name    { get; }
    public double    Size    { get; }
    public double    Stretch { get; }  // 1.0 = 100% (normal); use FontStretch constants
    public FontStyle Style   { get; }  // Normal, Italic, Oblique
    public int       Weight  { get; }  // 100–900; 400 = regular, 700 = bold
}
```

It's just a description, not a loaded font. Pass it to `IRenderables.CreateFont(fontInfo, exactMatchOnly)`.

### IFont: The Live Object

```csharp
public interface IFont : IDisposable
{
    FontInfo     FontInfo    { get; }
    FontMetrics  FontMetrics { get; }
    TextMetrics  MeasureText(ReadOnlySpan<char> text);
}
```

`IFont` wraps a backend-loaded typeface. It carries `FontMetrics` (properties of the font as a whole) and can measure individual strings.

`exactMatchOnly` on `CreateFont` controls fallback behavior. If `true`, a missing font returns `null` rather than falling back to the system default. The layout engine uses this to distinguish "font was intentionally unavailable" from "font was not found."

### FontMetrics: Per-Font Geometry

```csharp
public readonly struct FontMetrics
{
    public double LineHeight          { get; }   // Total line height (ascent + descent + leading)
    public double Ascent              { get; }   // Distance from baseline to top of cap height
    public double Descent             { get; }   // Distance from baseline to bottom of descenders
    public double UnderlineThickness  { get; }
    public double UnderlinePosition   { get; }
    public double StrikethroughPosition { get; }
    public double OverlinePosition    { get; }
    public Size2d Em                  { get; }   // Measured size of 'M'
    public Size2d En                  { get; }   // Measured size of 'N'
    public Size2d Ex                  { get; }   // Measured size of 'x' (x-height)
}
```

These are the CSS layout metrics. `LineHeight` drives line-box sizing. `Ascent` and `Descent` are needed for baseline alignment. `Em`, `En`, and `Ex` feed CSS length units `em`, `en`, and `ex`; they carry both dimensions since the full glyph measurement is available at no extra cost.

### TextMetrics: Per-String Measurement

```csharp
public readonly struct TextMetrics
{
    public Rect2d   Bounds  { get; }     // Ink bounds of the glyphs, including origin offset
    public Vector2d Advance { get; }     // How far the text cursor moves after this text
    public Size2d   Size    { get; }     // Bounds.Size — the width and height of the ink bounds
    public Vector2d Offset  { get; }     // Bounds.TopLeft — offset of the glyphs from the draw origin
}
```

`MeasureText` is the layout engine's primary text measurement call. `Advance` is how far the text cursor moves—this is what the layout engine uses for inline layout. `Bounds` is the full ink bounding box, including both the offset of the glyph cluster from the draw origin and its dimensions. `Size` and `Offset` are computed from `Bounds` as convenience accessors for the common cases where only the dimensions or only the offset is needed.

## IImage: Format-Agnostic Image Handle

```csharp
public interface IImage : IDisposable
{
    Size2d?  Size        { get; }   // Intrinsic size, if the format has one
    Size2d   MinSize     { get; }   // Minimum display size
    Size2d   MaxSize     { get; }   // Maximum display size
    double?  AspectRatio { get; }   // Intrinsic aspect ratio, if available
}
```

`Size?` is nullable because some image formats (SVG, for example) have no intrinsic pixel dimensions—they are infinitely scalable. The layout engine uses `Size` to determine the element's natural size, falling back to `AspectRatio` alone if `Size` is null.

`MinSize` and `MaxSize` constrain the renderable range. For raster images these are typically `(1,1)` and `(Width,Height)`.

`IImage` is not yet implemented in `Onyx.Skia`; `CreateImage` throws `NotImplementedException`. The interface is in place so the layout engine can be written against it now.

## LineStyle: Stroke Geometric Style

```csharp
public class LineStyle : IEquatable<LineStyle>
{
    public ReadOnlyMemory<float> Segments { get; }  // Alternating on/off lengths in CSS pixels; empty = solid
    public bool     IsSolid     { get; }            // True when Segments is empty
    public LineCap  Cap         { get; }            // How open endpoints are rendered
    public LineJoin Join        { get; }            // How corners are rendered
    public float    MiterLimit  { get; }            // Miter fallback ratio; CSS default is 4.0

    public static LineStyle Solid       { get; }    // Unbroken line
    public static LineStyle Dotted      { get; }    // 1px on, 2px off
    public static LineStyle Dashed      { get; }    // 4px on, 4px off
    public static LineStyle DashDot     { get; }    // 4px on, 2px off, 1px on, 2px off
    public static LineStyle DashDotDot  { get; }    // 4px on, 2px off, 1px on, 2px off, 1px on, 2px off
    // ... and more predefined patterns
}
```

`LineStyle` is an immutable class that describes the full geometric style of a stroked line.

`Segments` is a flat array of alternating on/off lengths in CSS pixels, identical in format to CSS's `stroke-dasharray`. Even-indexed elements are drawn; odd-indexed elements are gaps. An empty array means a solid line. Segments are validated at construction (even count, all positive, rounded to three decimal places) and shared without re-copying by the `With*()` methods.

`Cap`, `Join`, and `MiterLimit` map directly to CSS's `stroke-linecap`, `stroke-linejoin`, and `stroke-miterlimit`. The `LineCap` and `LineJoin` enums both use `Unknown = 0` as their zero value, so an accidentally default-initialized enum is detectable rather than silently falling back to a reasonable-looking value.

```csharp
public enum LineCap  { Unknown = 0, Flat, Round, Square }
public enum LineJoin { Unknown = 0, Miter, Round, Bevel }
```

`MiterLimit` applies only when `Join` is `Miter`. When the miter point at a sharp corner would extend beyond `MiterLimit` times the line thickness, the join falls back to a bevel. The CSS default of 4.0 is also the constructor default.

The predefined statics cover all the CSS keyword combinations and the common programmatic cases. Renderers that do not support all of these properties should silently ignore the parts they do not understand; the Skia backend handles all of them directly since Skia's paint API accepts the same float array format.

## The Rendering Flow

The layout engine's interaction with the renderer follows a simple protocol:

```
IRenderer.Begin()               ← save renderer state; start of a frame or layer
  IRenderer.Clear(background)   ← fill with background color
  IRenderer.FillRect(...)       ← draw primitives in z-order
  IRenderer.FillRoundRect(...)
  IRenderer.DrawText(...)
  ...
IRenderer.End()                 ← restore renderer state; end of frame or layer
```

`Begin` and `End` bracket a drawing session. They correspond to canvas save/restore in Skia. Rendering is always back-to-front: the layout engine issues draw calls in painter's-algorithm order (background before foreground, parent before child).

Resources created via `IRenderables` live outside this flow and are long-lived by design. The layout engine caches them aggressively: a font or brush computed for a given element is retained and reused across render passes for as long as the element's computed style remains unchanged. These objects are generally not allocated during rendering at all — allocation happens when a style is first computed or when a style change invalidates the cached resource. Disposal happens when the cached resource is evicted: when an element is removed from the tree, when its style changes such that the old resource no longer applies, or when the renderer is torn down.

The fundamental assumption underlying all of this is that `IRenderables.Create*()` calls are expensive. This is not a theoretical concern. Loading and decoding a font file from disk, constructing a GPU shader for a gradient, uploading a texture — these are real costs that take real time. Onyx works hard to avoid them: if a resource can be reused, it will be. At the same time, Onyx does not cache without limit; it must also balance memory pressure, and resources that are no longer likely to be needed should be released. The caching strategy is a continuous tradeoff between allocation cost and memory cost, and the assumption of expensive allocation is what makes that tradeoff worth the complexity it adds.

## Lifetime and Disposal

All five handle types — `IBrush`, `IClipper`, `IFont`, `IImage`, and `IRenderer` itself — implement `IDisposable`. Lifetime decisions are driven directly by the performance contract:

Because `IRenderables` allocations are expensive, the layout engine holds onto them for as long as they remain useful. The guiding principle is: **don't dispose a resource until you know you won't need it again**, and don't create it again unless you must. Specific policies:

- **`IFont`** — cached for the lifetime of the element's computed style, and often longer if the same font is shared across multiple elements. Font loading is one of the most expensive `IRenderables` operations and fonts are almost never unique to a single element, so they are pooled and retained aggressively.
- **`IBrush`** — cached per element as long as its computed style is unchanged. A gradient brush may involve shader compilation; creating a new one on every frame would be unacceptable.
- **`IClipper`** — similarly cached. Clip regions are computed from layout geometry; they only change when the element is resized or repositioned.
- **`IImage`** — retained for as long as the image URL is referenced by any element. Images are the most memory-intensive resource and are subject to the most active eviction pressure when memory is constrained.
- **`IRenderer`** — owned by the host (e.g., `Window`). It is created when the rendering surface is created and disposed when the surface is destroyed (typically on window close or resize).

Additional rules that hold regardless of type:

- The **owner** (layout engine for render resources; host for `IRenderer`) is responsible for disposal.
- Handles **must not** be used after disposal.
- Passing a handle created by one backend to a different backend is **not supported** and will produce a runtime error. An `ISkiaBrush` cannot be used with a hypothetical OpenGL renderer.
