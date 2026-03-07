# Onyx.Windows

## What Is a Host?

An Onyx *host* is a platform-specific package that provides a native window or surface into which Onyx content is rendered. The host owns the OS-level window lifecycle (creation, message dispatch, destruction), bridges native input events to Onyx's input model, and drives the rendering pipeline that turns Onyx's layout output into pixels on screen.

The host is a deliberate boundary. Everything on one side of it—the HTML parser, the DOM, CSS selectors, computed styles, layout—is pure managed C# with no OS dependencies. Everything on the other side is platform-specific. A host joins the two.

`Onyx.Windows` is the first host implementation. It targets the Microsoft Windows desktop platform using the Win32 API directly via P/Invoke. Future hosts include `Onyx.Wpf`, `Onyx.Avalonia`, `Onyx.X11`, and `Onyx.MacOS`.

## Host Design Philosophy

No attempt is made to make hosts uniform with each other, and none ever will be. This is intentional.

A host by necessity must interact with its platform in whatever ways are natural for that platform. The right way to host Onyx on Windows is not the same as the right way to host it on macOS or inside WPF, and pretending otherwise—papering over real differences with a false abstraction—produces something that is harder to use than the native approach on every platform it supports. Avalonia, for example, goes to considerable lengths to hide the underlying `HWND` from the developer. The result is that doing anything Windows-specific becomes a struggle against the framework instead of a natural extension of it.

`Onyx.Windows` takes the opposite position. `Handle` is the very first property on `Window`, and that is not an accident. To do anything meaningful on Windows, you need a window handle. Hiding it does not make it go away; it just makes it harder to reach when you need it. The message queue is exposed directly as `WindowsMessageQueue`. The methods, behaviors, and nomenclature on `Window` align closely to the messages, behaviors, and nomenclature of Win32 itself—`WM_CLOSE` becomes `OnClose`, `WM_PAINT` becomes `Render`, size constraints map onto `WM_WINDOWPOSCHANGING`. If you know Windows, `Onyx.Windows` will feel familiar. If you don't yet know Windows, `Onyx.Windows` gives you a direct line to the documentation that does.

If your application already has a message loop—WinForms, for instance, runs its own—you do not need `WindowsMessageQueue` at all. Nothing in `Window` requires it. The loop is provided as a convenience for applications that have no other message pump, not as a mandatory component.

The goal of a host is not portability. It is *escape velocity*. The host speaks the platform's native language well enough that you can satisfy whatever platform requirements you have—embedding, parenting, interop, native controls alongside Onyx content—and then, having satisfied them, stop thinking about the platform entirely and build the rest of your UI with HTML and CSS. The host is a launchpad. Its job is to let you leave.

## Package Dependencies

`Onyx.Windows` references two packages:

- **`Onyx`** — the core engine: parsers, DOM, CSS, selectors, computed styles.
- **`Onyx.Skia`** — the Skia-backed rendering implementation.

It targets **net8.0** and enables `AllowUnsafeBlocks`, required by the GDI DIB interop (raw pointer into an unmanaged pixel buffer).

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│                Onyx.Windows                     │
│                                                 │
│  Window                                         │
│  ├── Document (Onyx core)                       │
│  ├── SkiaRenderer (Onyx.Skia)                   │
│  │   └── SKSurface → GDI DIB → BitBlt           │
│  └── Win32 message loop                         │
│      ├── WM_PAINT → Render()                    │
│      ├── WM_SIZE / WM_MOVE → geometry events    │
│      ├── WM_*BUTTON* → OnMouseButton()          │
│      ├── WM_MOUSEMOVE → OnMouseMove()           │
│      ├── WM_KEY* / WM_CHAR → OnKey() / OnChar() │
│      ├── WM_SETFOCUS / WM_KILLFOCUS             │
│      └── WM_CLOSE → OnClose()                   │
│                                                 │
│  WindowsMessageQueue                            │
│  └── GetMessage / TranslateMessage / Dispatch   │
└─────────────────────────────────────────────────┘
```

The `Window` class is the host. It is thin: it exists only to connect the Win32 world to the Onyx world. It does not know about layout algorithms or CSS properties—those belong in the core.

## The Window Class

`Window` is a non-inheriting, `IDisposable` class that wraps a Win32 `HWND`. Creating a `Window` allocates a real OS window. Disposing it destroys that window.

### Construction

```csharp
var window = new Window(
    document:         myDocument,
    title:            "My App",
    x:                100,  y: 100,
    width:            800,  height: 600,
    hasTitlebar:      true,
    canResize:        true,
    canMaximize:      true,
    canMinimize:      true,
    hasBorder:        true,
    isToolWindow:     false,
    alwaysOnTop:      false,
    isChildWindow:    false,
    parent:           null
);
```

All parameters are optional with reasonable defaults. Position defaults to `CW_USEDEFAULT` (Windows places the window automatically). Size defaults to `CW_USEDEFAULT` as well.

The constructor:
1. Registers the `"OnyxWindow"` Win32 window class (first call only; see below).
2. Allocates a `GCHandle` to itself (see Message Routing).
3. Calls `CreateWindowEx` with the style flags derived from the boolean parameters.
4. `CreateWindowEx` triggers `WM_NCCREATE` synchronously, which stores the `GCHandle` into the window's extra bytes before any other messages can arrive.

### Win32 Window Class Registration

Win32 requires that every window belong to a named window class registered with `RegisterClassEx`. Onyx registers the class `"OnyxWindow"` once, the first time any `Window` is created on a given thread.

Registration is guarded by a `lock` on a static object with a double-checked flag, making it thread-safe. The class sets `cbWndExtra = IntPtr.Size` (8 bytes on 64-bit), which reserves space in the Win32 kernel structure for one pointer—used by the message routing mechanism.

### Message Routing

Win32 window procedures are global C-style callbacks. Routing a Win32 message to a specific managed `Window` object requires storing a reference to the object where Win32 can find it.

Onyx uses the standard "window extra bytes" pattern:

1. At construction, `GCHandle.Alloc(this)` pins the `Window` in the GC and produces a pointer-sized handle.
2. That handle is stored in the window's extra bytes via `SetWindowLongPtr(GWLP_USERDATA)`.
3. The static `WindowProcInternal` callback (the actual Win32 `WndProc`) calls `GetWindowLongPtr(GWLP_USERDATA)` on every message, reconstructs the `GCHandle`, and calls the instance method `WindowProc()` on the retrieved `Window`.

Special care is taken for the two lifetime messages:

- **`WM_NCCREATE`** — Before calling `DefWindowProc`, extracts the `GCHandle` pointer from `CREATESTRUCT.lpCreateParams` (passed through `CreateWindowEx`) and stores it in the extra bytes. This is the first message a window receives, so the mapping must be established here.
- **`WM_NCDESTROY`** — After calling `DefWindowProc`, frees the `GCHandle`. This is the last message a window receives.

### Window Count and Automatic Quit

`Window` maintains a `[ThreadStatic]` counter `_windowCount` that is incremented on `WM_NCCREATE` and decremented on `WM_NCDESTROY`. When it reaches zero, `WindowsMessageQueue.Quit()` is called automatically, posting `WM_QUIT` to the thread's message queue. This means the message loop exits naturally when the last window is closed, without requiring the application to track window lifetime manually.

### Properties

#### Window Style

These properties correspond directly to Win32 window style bits and can be set at construction time. Most can also be changed after creation via `SetWindowLong` + `SetWindowPos`.

| Property | Win32 style bit | Description |
|---|---|---|
| `HasTitlebar` | `WS_CAPTION` | Caption bar at top of window. |
| `CanMaximize` | `WS_MAXIMIZEBOX` | Maximize button in caption. |
| `CanMinimize` | `WS_MINIMIZEBOX` | Minimize button in caption. |
| `CanResize` | `WS_THICKFRAME` | Resizable border. |
| `HasBorder` | `WS_BORDER` | Non-resizable border. |
| `IsToolWindow` | `WS_EX_TOOLWINDOW` | Tool palette style (smaller caption, omitted from taskbar). |
| `AlwaysOnTop` | `WS_EX_TOPMOST` (via `HWND_TOPMOST`) | Window floats above all others. |
| `IsChildWindow` | `WS_CHILD` | Embedded in a parent window (no separate taskbar entry). |

#### Window State

| Property | Description |
|---|---|
| `IsMaximized` | Whether the window is currently maximized. |
| `IsMinimized` | Whether the window is currently minimized. |
| `IsVisible` | Whether the window is currently shown. |
| `IsFocused` | Whether the window currently has keyboard focus. |

#### Geometry

| Property | Type | Description |
|---|---|---|
| `Rect` | `Rect2i` | Window rectangle in screen coordinates (includes frame). |
| `ClientRect` | `Rect2i` | Client area rectangle (excludes frame, title bar). |
| `MinSize` | `Size2i?` | Minimum window size enforced during resize (null = unconstrained). |
| `MaxSize` | `Size2i?` | Maximum window size enforced during resize (null = unconstrained). |

#### Handle

`Handle` exposes the raw `HWND` as an `IntPtr`. This allows embedding `Onyx.Windows` alongside other Win32 code or inside a WPF/WinForms `HwndHost`—zero-cost adoption.

### Events

Events follow standard .NET `EventHandler<TEventArgs>` conventions.

| Event | Args | Raised when |
|---|---|---|
| `DocumentChanged` | `EventArgs` | The `Document` property is assigned. |
| `TitleChanged` | `EventArgs` | The `Title` property is assigned. |
| `ShowTitlebarChanged` | `EventArgs` | `HasTitlebar` changes. |
| `RectChanging` | `RectChangingEventArgs` | Window is about to be moved or resized. Cancelable. |
| `RectChanged` | `RectChangedEventArgs` | Window was moved or resized. |
| `Moved` | `RectChangedEventArgs` | Window origin changed (subset of `RectChanged`). |
| `Sized` | `RectChangedEventArgs` | Window size changed (subset of `RectChanged`). |
| `Disposing` | `DisposeEventArgs` | `Dispose()` has been called; window is being destroyed. |
| `Disposed` | `DisposeEventArgs` | `Dispose()` has completed. |
| `FocusChanged` | `EventArgs` | Window gained or lost keyboard focus. |
| `VisibleChanged` | `EventArgs` | Window was shown or hidden. |
| `CloseClicked` | `EventArgs` | User clicked the close button (or pressed Alt+F4). |

`RectChangingEventArgs` derives from `CancelEventArgs`. Setting `Cancel = true` in a `RectChanging` handler vetoes the move/resize before it is applied.

### Message Handling: Three-Tier Design

The `WindowProc` method handles Win32 messages. Rather than putting Win32-specific logic directly into its `switch` statement, `Window` uses a three-tier design:

**Tier 1 — `OnWin32_*` methods (Win32-specific)**
Handle the raw Win32 mechanics: unpack `wParam`/`lParam`, convert Win32 types, call `DefWindowProc` where required. These are not portable—they depend on Win32 constants and calling conventions. Examples: `OnWin32_WmPaint`, `OnWin32_WmWindowPosChanging`.

**Tier 2 — `On*` methods (mid-level, platform-neutral signature)**
Called by Tier 1. Work in Onyx types rather than Win32 types. Raise events and perform state updates. Examples: `OnMouseButton(button, action, position, modifiers)`, `OnKey(vkey, action, modifiers)`, `OnChar(ch)`, `OnClose()`, `OnRectChanged(oldRect, newRect)`. These could in principle be shared with other hosts.

**Tier 3 — `OnDocumentChanged`, `OnRectChanged`, etc. (high-level, fully portable)**
Respond to semantic changes: what does a new `Document` mean? What does a size change mean for rendering? These contain only Onyx-level logic.

This layering ensures that porting `Window` to another platform (`Onyx.Wpf`, etc.) involves rewriting Tier 1 and leaving Tier 2/3 intact—or, in the case of a host that already speaks in .NET events, possibly only Tier 1.

### Size Constraints

When `MinSize` or `MaxSize` is set, `Window` enforces the constraints during `WM_WINDOWPOSCHANGING`. The constraint logic is more subtle than a simple clamp because the user can drag any of the four edges or corners, and the constraint must be applied to the correct side.

`Constrain1D(newLo, newHi, oldLo, oldHi, min, max)` handles one axis. If the high edge is moving, the high edge is clamped. If the low edge is moving, the low edge is clamped. If the size is below minimum or above maximum but neither edge is clearly moving (e.g., a programmatic resize), the size is clamped from the high side.

`Constrain2D` applies `Constrain1D` to both axes independently and writes back into the `WINDOWPOS` structure that Win32 uses to actually position the window.

## Rendering Pipeline

When Windows sends `WM_PAINT`, `Window` calls `Render(hdc, paintRect)` with the paint DC and the dirty rectangle.

### Back Buffer: GDI DIB

Onyx does not render directly to the window DC. Instead it maintains a GDI *device-independent bitmap* (DIB) as an off-screen back buffer. A memory DC is created with `CreateCompatibleDC`, and a 32bpp BGRA DIB section is created with `CreateDIBSection`. `CreateDIBSection` returns both a GDI `HBITMAP` and a raw pointer to the pixel data.

This buffer is created (or recreated) on demand when `GetRenderer()` detects that the client area size has changed. On resize, the old bitmap is deselected from the DC and deleted before the new one is created.

### SKSurface Over the DIB

`SKSurface.Create` wraps the raw DIB pixel pointer as an `SKSurface` (SkiaSharp surface). The surface's `SKImageInfo` describes the buffer as 32bpp BGRA (matching the DIB format). The `SkiaRenderer` is given this surface and uses it for all drawing.

No pixel data is copied to set this up—the DIB pixel memory is used directly by Skia. Skia writes into the DIB; the DIB is then presented to Win32.

### BitBlt Presentation

After the `SkiaRenderer` finishes drawing, `BitBlt` copies the memory DC (containing the rendered DIB) to the paint DC obtained from `BeginPaint`/`EndPaint`. This is the standard Win32 double-buffering pattern.

### Current State

The `Render()` method currently contains test content (a gradient, rounded rectangles, and text drawn through `SkiaRenderer`). DOM rendering is not yet hooked up: there is a `TODO` comment in `Render()` where the layout engine will eventually ask the renderer to paint dirty intersections of `Document` content. The rendering infrastructure—the DIB, the `SKSurface`, the `SkiaRenderer`, the `BitBlt` loop—is complete.

## The Message Queue

`WindowsMessageQueue` is a thin wrapper over the Win32 message loop.

```csharp
WindowsMessageQueue.Run();   // blocks until WM_QUIT
WindowsMessageQueue.Quit();  // posts WM_QUIT
```

`Run()` executes the standard `GetMessage` / `TranslateMessage` / `DispatchMessage` loop. `GetMessage` blocks until a message is available; `TranslateMessage` synthesizes `WM_CHAR` from `WM_KEYDOWN`; `DispatchMessage` routes the message to the window's `WndProc`.

`WindowsMessageQueue` is `[ThreadStatic]`: each thread has its own instance and its own message queue. This is a Win32 requirement—a thread may only process messages for windows it created.

Applications call `WindowsMessageQueue.Run()` after creating their windows. When the last window on the thread is destroyed, `Window._windowCount` reaches zero and `Quit()` is called automatically, which causes `Run()` to return.

## Input Types

### ModifierKeys

```csharp
[Flags]
public enum ModifierKeys : ushort
{
    None, LeftButton, RightButton, Shift, Control, MiddleButton, XButton1, XButton2
}
```

Passed to `OnMouseButton` and `OnMouseMove` to indicate which modifier keys and mouse buttons were held at the time of the event. Mirrors the low word of `wParam` in Win32 mouse messages.

### MouseButton

```csharp
public enum MouseButton { None, Left, Right, Middle, X1, X2 }
```

Identifies which button caused a mouse button event.

### MouseButtonAction

```csharp
public enum MouseButtonAction { None, Press, Release, DoubleClick }
```

Distinguishes between button press, release, and double-click, rather than having separate event types for each.

### KeyAction

```csharp
public enum KeyAction { None, Press, Release, Repeat }
```

Passed to `OnKey`. `Repeat` corresponds to `WM_KEYDOWN` messages with the previous-key-state bit set (the key is being held and auto-repeating).

## Event Args Types

| Type | Base | Fields |
|---|---|---|
| `RectChangingEventArgs` | `CancelEventArgs` | `OldRect`, `NewRect` (both `Rect2i`) |
| `RectChangedEventArgs` | `EventArgs` | `OldRect`, `NewRect` (both `Rect2i`) |
| `DisposeEventArgs` | `EventArgs` | `IsDisposing` (`bool`) |

`RectChangingEventArgs` inherits `CancelEventArgs` from `System.ComponentModel`, which provides the `Cancel` property. Setting `Cancel = true` in a `RectChanging` handler causes `Window` to reject the pending move or resize by restoring the original `WINDOWPOS` values in `WM_WINDOWPOSCHANGING`.

## Win32 Interop Layer

The Win32 API is surfaced through a static partial class `Win32` split across three files:

**`Win32.Functions.cs`** — P/Invoke declarations for `user32.dll` and `gdi32.dll`. All declarations use `LibraryImport` or `DllImport` with explicit `CharSet` and `ExactSpelling` where applicable. Both 32-bit (`GetWindowLong`/`SetWindowLong`) and 64-bit (`GetWindowLongPtr`/`SetWindowLongPtr`) variants are declared; `Window` calls the appropriate one based on `IntPtr.Size`.

**`Win32.Structs.cs`** — Managed equivalents of Win32 structures, all decorated with `[StructLayout(LayoutKind.Sequential)]`:

| Struct | Win32 equivalent | Purpose |
|---|---|---|
| `MSG` | `MSG` | Message from the queue. |
| `WNDCLASSEX` | `WNDCLASSEX` | Window class registration data. |
| `CREATESTRUCT` | `CREATESTRUCT` | Data passed to `WM_NCCREATE`/`WM_CREATE`. |
| `WINDOWPOS` | `WINDOWPOS` | Position data in `WM_WINDOWPOSCHANGING`. |
| `RECT` | `RECT` | Rectangle as four `int` fields. |
| `PAINTSTRUCT` | `PAINTSTRUCT` | Data from `BeginPaint`. |
| `BITMAPINFOHEADER` | `BITMAPINFOHEADER` | DIB format descriptor. |
| `BITMAPINFO` | `BITMAPINFO` | Wrapper around `BITMAPINFOHEADER`. |

**`Win32.Enums.cs`** — Win32 constants as C# `const int` fields, organized into groups:
- `WM_*` — window message identifiers
- `GWL_*` / `GWLP_*` — indices for `GetWindowLong(Ptr)`
- `HWND_*` — special `HWND` values for `SetWindowPos`
- `SWP_*` — flags for `SetWindowPos`
- `SW_*` — `ShowWindow` commands
- `CS_*` — window class style bits
- `WS_*` / `WS_EX_*` — window style and extended style bits
- `BI_RGB`, `DIB_RGB_COLORS`, `SRCCOPY` — GDI bitmap constants

## Thread Model

`Onyx.Windows` inherits the Win32 threading model:

- **Each window must be created, used, and destroyed on the same thread.** Win32 associates a window with the thread that created it; messages are delivered only on that thread.
- **`[ThreadStatic]` where needed.** Both `WindowsMessageQueue.Instance` and `Window._windowCount` are `[ThreadStatic]`, so a multi-threaded application can independently host multiple Onyx windows on separate threads.
- **No cross-thread message pumping.** If you need to trigger a repaint from another thread, use `InvalidateRect` (which is thread-safe in Win32) rather than calling `Render()` directly.

The Onyx core has no thread affinity of its own. A `Document` can be manipulated from any thread as long as you serialize access yourself. The constraint is on the host side: the thread that created the window is the thread that drives it.

## Embedding in Existing Applications

`Onyx.Windows` is designed for zero-cost adoption. You do not need to replace your existing Win32 window infrastructure. The `Handle` property exposes the raw `HWND`, which can be:

- Passed to `SetParent` to embed the Onyx window inside a parent Win32 window.
- Wrapped in a WPF `HwndHost` to embed inside a WPF application.
- Wrapped in a WinForms `NativeWindow` to embed inside a WinForms application.

When `IsChildWindow` is true and a `parent` is supplied to the constructor, the Onyx window becomes a child window with `WS_CHILD` and no separate taskbar entry. The parent handles its overall window lifecycle while Onyx manages only its own rectangle within it.

## The Rendering Subfolder

`Onyx.Windows/Rendering/` contains the Skia-backed implementation of Onyx's `IRenderer` abstraction. It is covered in a separate document. The key classes are:

- **`SkiaRenderer`** — implements `IRenderer` using an `SKCanvas` from `SkiaSharp`.
- **`SkiaClipper`** — implements `IClipper` using `SKPath` clip regions.
- **`SkiaFont`** — implements `IFont` wrapping `SKFont` and `SKTypeface`.
- **`SkiaLinearGradientBrush`** — implements `ILinearGradientBrush` using `SKShader`.
- **`*Extensions.cs`** — static extension methods that convert Onyx types (`Color32`, `Rect2i`, `Vector2f`) to their Skia counterparts (`SKColor`, `SKRect`, `SKPoint`).

`Window.GetRenderer()` creates a `SkiaRenderer` backed by the GDI DIB surface. The renderer is discarded and recreated whenever the window is resized, since the DIB must be reallocated to match the new client area.
