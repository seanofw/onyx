# Computed Styles

## Overview

A **computed style** is the final set of CSS property values that applies to a single element, after all stylesheets have been consulted, selectors have been matched, specificity conflicts have been resolved, shorthand properties have been expanded, and inheritance from the parent element has been applied. In Onyx, this process is implemented across several subsystems documented elsewhere; this document ties them together into a single end-to-end description.

The key files involved are:

| File | Role |
|------|------|
| `Html/Dom/Element.cs` | `GetComputedStyle()` — the entry point; caches the result and manages invalidation |
| `Css/StyleManager.cs` | `ComputeStyle()` — orchestrates the full pipeline; `GetStyleRules()` and `FindCandidateRules()` — the style rule engine |
| `Css/StyleQueue.cs` | Tracks elements needing recomputation; provides batched processing |
| `Css/Computed/ComputedStyle.cs` | The immutable, copy-on-write output object |
| `Css/Parsing/CssParser.cs` | Top-level stylesheet parser; orchestrates selector, property, and media query sub-parsers; threads `@media` queries onto `StyleRule` objects |
| `Css/Stylesheet.cs`, `Css/StyleRule.cs` | The parsed stylesheet data; each `StyleRule` carries an optional `MediaQuery` from its enclosing `@media` block |
| `Css/Properties/StyleProperty.cs` | The abstract base for all parsed property values |

---

## The computation pipeline

When `element.GetComputedStyle()` is called, the following steps occur:

### Step 1: Check the cache

Each element caches its computed style in a private `_computedStyle` field. If it's non-null, `GetComputedStyle()` returns it immediately — no work is done.

```
if (_computedStyle != null)
    return _computedStyle;
```

### Step 2: Verify the element is in a styled tree

If the element's tree root is not an `IStyleRoot` (i.e., it's not attached to a `Document`), there are no stylesheets to consult. The element receives `ComputedStyle.Default` — the global default style with all CSS initial values.

### Step 3: Compute the parent style

CSS inheritance requires knowing the parent's computed style before computing the child's. `GetComputedStyle()` handles this recursively:

```
ComputedStyle? parentStyle = Parent is Element parentElement
    ? parentElement.GetComputedStyle().MakeChildStyle()
    : null;
```

If the parent's style is also invalid, this recursive call will recompute it first — and that may in turn recompute *its* parent, and so on up the ancestor chain. The recursion bottoms out when either a valid cached style is found or the root of the tree is reached.

`MakeChildStyle()` creates a new `ComputedStyle` that carries forward only the **inherited** properties (text, font, list style, table, and other inherited fields) while resetting all non-inherited properties (sizes, borders, backgrounds, enums, etc.) to their defaults. This implements the CSS rule that inheritable properties propagate downward unless overridden, while non-inheritable properties start fresh at each element.

### Step 4: Find matching style rules

This step is delegated to `StyleManager.ComputeStyle()`, which calls `GetStyleRules()`. The style rule engine (documented in detail in [Selector Matching.md](Selector%20Matching.md)) performs a two-phase lookup:

**Phase 1 — Candidate generation (`FindCandidateRules`):**

The `StyleManager` maintains indexes over the **last simple selector** of every rule in every stylesheet. When a stylesheet is added, each rule is indexed by the element name, classnames, and IDs that appear in its last (rightmost) simple selector:

| Index | Keyed by | Contents |
|-------|----------|----------|
| `_elementNameIndex` | Tag name (`"div"`, `"p"`, ...) | Rules whose last simple selector specifies that element name |
| `_classNameIndex` | Classname (`"widget"`, `"active"`, ...) | Rules whose last simple selector includes that class |
| `_idIndex` | ID (`"main"`, `"header"`, ...) | Rules whose last simple selector includes that ID |
| `_genericIndex` | *(none — flat list)* | Rules whose last simple selector has no element name, class, or ID (e.g., `*`, `[attr]`, `:hover`) |

`FindCandidateRules()` assembles a superset of possibly-matching rules by querying every applicable index for the element — its tag name, its ID, and each of its classnames — and unioning the results with the generic rules. The result is a `HashSet<StyleRule>` that includes every rule that *could* match, with most non-matching rules excluded.

**Phase 2 — Verification (`GetStyleRules`):**

Each candidate rule is first checked for a **media query gate**: if the rule was declared inside an `@media` block, its `StyleRule.MediaQuery` is non-null, and the media query is evaluated against the document's current `MediaQueryContext` (see [CSS Media Queries.md](CSS%20Media%20Queries.md)). The compiled evaluator (`MediaQuery.GetEval()`) is used for performance, and only rules whose media query evaluates to `true` proceed — both `false` and `null` (indeterminate, per Kleene logic) cause the rule to be skipped. Rules with no media query (i.e., those declared outside any `@media` block) are unconditionally included.

Each surviving rule's selectors are then tested against the element using `Selector.IsMatch()` — the full IsMatch engine (documented in [Selector Matching.md](Selector%20Matching.md)), including right-to-left path walking and the adaptive JIT compiler for simple selectors. A compound selector (comma-separated) may contain multiple selectors; each is tested, and the **highest specificity** among matching selectors is recorded.

The result is a list of `StylePropertySetWithSpecificity` — each pairing a rule's property declarations with the specificity of the most-specific matching selector.

### Step 5: Add inline styles

If the element has a `style` attribute, its parsed inline styles are added to the rule list with `Specificity.MaxValue`. This ensures that inline styles override all stylesheet rules, per the CSS specification. The inline styles are parsed lazily on first access and cached.

### Step 6: Resolve specificity conflicts (`ExtractMostSpecificStyles`)

All property declarations from all matching rules (plus inline styles) are now merged into a single winner-take-all result. For each CSS property kind:

1. **Decompose shorthands.** Shorthand properties like `margin` or `border` are expanded into their individual sub-properties (`margin-top`, `margin-right`, etc.) via `StyleProperty.Decompose()`. After decomposition, each property kind is represented independently.

2. **Pick the winner.** For each property kind, the declaration with the highest specificity wins. If two declarations have the same specificity, `!important` breaks the tie. The result is a single `StyleProperty` per property kind — the one that will actually be applied.

### Step 7: Apply properties to the computed style

Starting from the parent's child style (from step 3) or `ComputedStyle.Default`, each winning property declaration is applied in turn:

- **Normal properties** call `styleProperty.Apply(computedStyle)`, which returns a new `ComputedStyle` with that property's value incorporated. (The `ComputedStyle` is a copy-on-write tree, so applying a single property only copies the sub-object that contains it.)

- **`inherit` properties** call `styleProperty.CopyProperty(computedStyle, parentStyle)`, which copies the property's value from the parent's computed style.

- **`initial` properties** call `styleProperty.CopyProperty(computedStyle, ComputedStyle.Default)`, which copies the property's value from the global default style.

- **`unset` properties** are treated as `inherit` for inheritable properties and `initial` for non-inheritable properties (this is handled by simply not applying the property, which leaves the inherited or default value in place).

### Step 8: Cache and dequeue

The resulting `ComputedStyle` is cached in `_computedStyle`. If the element was in the document's `StyleQueue`, it is removed, since it no longer needs recomputation.

---

## Style invalidation

The computed style cache is invalidated (set to `null`) whenever something changes that could affect the element's style. Invalidation does not immediately trigger recomputation — it only marks the element as needing it the next time `GetComputedStyle()` is called or the style queue is processed.

### What triggers invalidation

`Element.InvalidateComputedStyle()` is the central method. It nulls the cached style and adds the element to the document's `StyleQueue`. It is called from `Element.OnAttrChange()`, which is invoked whenever any attribute on the element changes:

| Change | Invalidation behavior |
|--------|----------------------|
| **ID changes** | Always invalidate. IDs are fundamental to selector matching. |
| **Class changes** | Compute the symmetric difference between old and new classname sets. Only invalidate if at least one changed classname appears in `StyleManager.ClassnamesUsedByStyles`. |
| **`style` attribute changes** | Always invalidate. Inline styles directly affect the computed style. |
| **Other attribute changes** | Only invalidate if the attribute name appears in `StyleManager.AttributesUsedByStyles`. |

The `ClassnamesUsedByStyles` and `AttributesUsedByStyles` dictionaries are reference-counted sets maintained by the `StyleManager`. When a stylesheet is added, every classname and attribute name referenced by any selector is registered; when a stylesheet is removed, they are unregistered. This allows `OnAttrChange()` to skip invalidation entirely for attributes that no selector cares about — a significant optimization in documents where elements have many data attributes but stylesheets only reference a few.

### Stylesheet changes

When a stylesheet is added or removed from the document, the `StyleManager` raises the `StylesheetsChanged` event, and the `Document` responds by calling `InvalidateChildComputedStyles()` on itself — which invalidates the computed styles of all direct child elements. This is effectively a whole-tree invalidation, because when those children are later recomputed, their own children will be invalidated in turn (see "The style queue" below).

This whole-tree invalidation is deliberate. Stylesheets are immutable objects in Onyx — there is no API to modify a single rule within a stylesheet. The only operations are adding or removing entire `Stylesheet` objects, which encourages batching style declarations into complete stylesheets rather than making incremental rule-by-rule changes. Because stylesheets are expected to be created at application startup and then left alone, the cost of whole-tree invalidation is paid rarely (typically once), while the far more common operation — changing an element's class or attributes at runtime — benefits from the fine-grained selective invalidation described above.

---

## The style queue

The `StyleQueue` is a `HashSet<Element>` owned by the `Document` (via the `IStyleRoot` interface). It tracks elements whose computed styles are invalid and need recomputation. Elements are added to the queue by `InvalidateComputedStyle()` and removed either when `GetComputedStyle()` recomputes their style or when the queue is explicitly processed.

### Eager vs. batched recomputation

There are two ways to trigger recomputation:

**Eager:** Calling `element.GetComputedStyle()` immediately recomputes that element's style (and recursively its ancestors' styles, if they are also invalid). This is useful when you need a specific element's style right now.

**Batched:** Calling `document.ValidateComputedStyles()` drains the entire queue. For each element dequeued, it calls `GetComputedStyle()` to recompute the style, then calls `InvalidateChildComputedStyles()` to enqueue the element's direct children. This creates a breadth-first ripple through the tree: each level is computed, its children are enqueued, then those children are computed, and so on until the queue is empty and the entire tree is up to date.

The batched approach is more efficient when many changes are made in sequence. For example, adding multiple classes, modifying several attributes, or restructuring the DOM can each trigger individual invalidations. Rather than recomputing after each change, the application can make all its changes and then call `ValidateComputedStyles()` once at the end.

### Why breadth-first, not recursive

The style queue's `ProcessQueue()` method processes elements one at a time, calling `InvalidateChildComputedStyles()` after each to enqueue children. This is effectively a breadth-first traversal, not a depth-first recursive one. The distinction matters because CSS inheritance flows downward: a parent's computed style must be finalized before its children can be computed. By processing the queue level by level, the system ensures that when a child's `GetComputedStyle()` is called, the parent's style is already valid and cached.

---

## Putting it all together

Here is the complete flow from stylesheet to rendered style, showing how the documented subsystems connect:

```
1. SETUP (application startup)
   ┌──────────────────────────────────────────────────┐
   │ document.AddStylesheet(cssText, filename)         │
   │   → CssLexer + CssParser → Stylesheet            │
   │     CssParser orchestrates sub-parsers:           │
   │       CssSelectorParser (selectors)               │
   │       CssPropertyParser (property declarations)   │
   │       CssMediaQueryParser (@media conditions)     │
   │     Each @media block's query is attached to      │
   │       every StyleRule inside that block            │
   │   → StyleManager indexes each rule's last         │
   │     simple selector into _elementNameIndex,       │
   │     _classNameIndex, _idIndex, _genericIndex      │
   │   → Tracks classnames and attributes used by      │
   │     selectors in reference-counted dictionaries   │
   │   → Fires StylesheetsChanged → invalidates tree   │
   └──────────────────────────────────────────────────┘

2. MUTATION (runtime)
   ┌──────────────────────────────────────────────────┐
   │ element.AddClass("highlighted")                   │
   │   → OnAttrChange("class", newValue, oldValue)     │
   │   → Symmetric diff: {"highlighted"} is new        │
   │   → Is "highlighted" in ClassnamesUsedByStyles?   │
   │     YES → InvalidateComputedStyle()               │
   │          → _computedStyle = null                   │
   │          → StyleQueue.Enqueue(element)             │
   │     NO  → nothing happens (fast path)             │
   └──────────────────────────────────────────────────┘

3. RECOMPUTATION (on demand or batched)
   ┌──────────────────────────────────────────────────┐
   │ element.GetComputedStyle()                        │
   │   → Cache miss (_computedStyle == null)           │
   │   → Recursively get parent's computed style       │
   │   → parentStyle.MakeChildStyle()                  │
   │     (inherit inherited props, reset the rest)     │
   │                                                   │
   │ StyleManager.ComputeStyle(context, element, parent) │
   │   → FindCandidateRules(element)                   │
   │     (query indexes by element's tag, ID, classes) │
   │   → GetStyleRules(element, context)               │
   │     (filter by @media query if present;           │
   │      IsMatch each candidate; track specificity)   │
   │   → Add inline styles at Specificity.MaxValue     │
   │   → ExtractMostSpecificStyles()                   │
   │     (decompose shorthands, pick highest-spec      │
   │      winner per property kind)                    │
   │   → Apply each winner to the child style          │
   │                                                   │
   │   → Cache result in _computedStyle                │
   │   → Remove from StyleQueue                        │
   └──────────────────────────────────────────────────┘
```

The entire process is designed so that the expensive parts (parsing stylesheets, building indexes) happen once at startup, while the frequent parts (invalidation checks, style recomputation) are as cheap as possible. The style rule engine's indexes narrow the candidate set before `IsMatch()` is called; media queries are evaluated via JIT-compiled delegates so that `@media` filtering adds negligible cost per rule; the `IsMatch()` engine's adaptive JIT compiler ensures that selectors used more than twice run at near-native speed; and the selective invalidation logic avoids unnecessary recomputation when attributes change that no selector references. The result is a system where changing an element's class at runtime and reading back its computed style is fast enough for interactive use.

---

## The ComputedStyle data structure

### Design principles

A `ComputedStyle` is an immutable, copy-on-write tree of data objects. It holds every computed CSS property value for a single element — dimensions, colors, fonts, borders, backgrounds, and more. The design is governed by two competing goals:

1. **Replacing a single property should copy as little as possible.** When a `StyleProperty.Apply()` call changes one value (e.g., `margin-left`), only the sub-object containing that value is copied. The rest of the tree is shared with the original.

2. **Reaching any property should require at most a couple of indirections.** The tree is deliberately wide and shallow — most properties are reachable in one or two hops from the root — so that layout code reading computed values doesn't chase long pointer chains.

The result is a tree that is 2–3 levels deep with 6 branches at the root, further subdivided into ~15 leaf-level objects. Every object is immutable: fields are `readonly`, and mutation is expressed through `With*()` methods that return a new copy with one field changed. When a `With*()` method is called on a leaf, only that leaf is copied; the parent then creates a new copy of itself pointing to the new leaf while sharing all other children with the old parent. This ripple propagates up to the root, producing a new `ComputedStyle` that shares most of its structure with the original.

### Tree structure

```
ComputedStyle
├── Enums              (ComputedEnumsStyle)         — display, position, float, clear, overflow, etc.
├── Sizes              (ComputedSizes)              — dimensions, offsets, padding, margin
├── Background         (ComputedBackgroundStyle)    — background color, layers, box-shadows, opacity
├── Border             (ComputedBorderStyle)        — 4 edges + 4 corner radii
│   ├── Top/Right/Bottom/Left  (ComputedBorderEdgeStyle)  — color, width, style per edge
│   └── Radii          (ComputedBorderRadii)        — 4 corner radii
├── Inherited          (ComputedInheritedStyle)     — all CSS-inheritable properties
│   ├── Text           (ComputedTextStyle)          — text-align, direction, line-height, spacing, etc.
│   ├── Font           (ComputedFontStyle)          — font family, size, weight, style, color, text-shadow
│   ├── Table          (ComputedTableStyle)         — border-collapse, empty-cells, caption-side, spacing
│   ├── List           (ComputedListStyle)          — list-style-type, -position, -image
│   └── Misc           (ComputedMiscInheritedStyle) — cursor, widows, orphans, quotes
└── RareFields         (ComputedRareFieldsStyle)    — infrequently-used properties
    ├── Flex           (ComputedFlexStyle)           — all flexbox properties
    ├── PageBreak      (ComputedPageBreakStyle)      — page-break-before/after/inside
    ├── Outline        (ComputedOutlineStyle)        — outline color, style, width, offset
    └── SuperRare      (ComputedSuperRareFieldsStyle)— clip, content, counters, vertical-align-length
```

### How inheritance works in the tree

The tree's top-level branching is not arbitrary — it reflects the CSS inheritance model. The `Inherited` subtree contains exactly the properties that CSS defines as inheriting by default (text, font, list style, table, cursor, etc.). Everything else — sizes, borders, backgrounds, enums, rare fields — is non-inherited.

This separation makes `MakeChildStyle()` trivial:

```csharp
public ComputedStyle MakeChildStyle()
    => new ComputedStyle(
        ComputedEnumsStyle.Default, ComputedSizes.Default,
        ComputedBackgroundStyle.Default, ComputedBorderStyle.Default,
        Inherited,                    // ← shared, not copied
        ComputedRareFieldsStyle.Default);
```

The child starts with the parent's `Inherited` subtree (shared by reference, zero cost) and defaults for everything else. When a CSS rule then overrides a property on the child, only the affected sub-object is copied. If no rules override any inherited properties, the child's `Inherited` subtree remains the same object as the parent's — the entire inherited state is shared across the ancestor chain until something changes it.

### Memory optimization techniques

The computed style objects use several techniques to minimize memory:

**Bit-packed enums.** `ComputedEnumsStyle` is a `struct` containing a single `ulong` field. Twelve enum values (display, position, float, clear, overflow-x, overflow-y, resize, box-sizing, table-layout, vertical-align, unicode-bidi, text-decoration) are packed into bit fields within this 8-byte value. Individual `With*()` methods use bitmasking to replace a single field.

**Split measure storage.** CSS measures (like `10px` or `50%`) are conceptually a `(value, units)` pair. Rather than storing a `Measure` struct (which would include padding overhead), several objects store the units and value as separate fields — e.g., `ComputedTextStyle` stores `_lineHeightUnits` and `_lineHeightValue` as separate `Units` and `double` fields. This allows tighter packing.

**Value types for small objects.** `ComputedEnumsStyle`, `ComputedBorderEdgeStyle`, `ComputedPageBreakStyle`, `EdgeSizes`, `SizeConstraints`, and `ComputedBorderRadii` are `struct` types. They are copied inline when their parent is copied, avoiding heap allocations for these small, frequently-duplicated objects.

**Rarity-based grouping.** Properties that are rarely used (flexbox, page breaks, outlines, clip, counters) are pushed into `RareFields` and `SuperRare`, so that for the common case where these properties are at their defaults, the `RareFields` sub-object is the shared `Default` instance and consumes no additional memory per element.

**ImmutableArray for collections.** Properties that hold variable-length data (background layers, box shadows, text shadows, font families, content pieces, counters, custom cursors, quotes) use `ImmutableArray<T>`, which is a value type wrapping a single array reference — zero overhead beyond the array itself, and safely shareable across copies.

### Convenience properties

`ComputedStyle` exposes a large number of public properties (display, position, margin-top, font-size, flex-grow, etc.), but most of them are simple one-line accessors that delegate to a child object:

```csharp
public DisplayKind Display => Enums.Display;
public Measure MarginTop => Sizes.MarginTop;
public FlexDirection FlexDirection => Flex.Direction;
```

These exist so that layout code can write `style.MarginTop` instead of `style.Sizes.MarginTop` — a readability convenience that does not add storage. The actual data is stored only in the leaf objects; the root just provides a flattened view.

The same pattern repeats at intermediate levels. For example, `ComputedStyle` has a `Flex` property that is shorthand for `RareFields.Flex`, and `ComputedBorderStyle` has `TopWidth` which is shorthand for `Top.Width`. These delegating properties exist purely to reduce the depth of access chains for common operations.

### The With* pattern

Every object in the tree follows the same mutation pattern. Each has a set of `With*()` methods that return a new instance with one field changed:

```csharp
// On ComputedBorderEdgeStyle (leaf):
public ComputedBorderEdgeStyle WithColor(Color32 color)
    => new ComputedBorderEdgeStyle(color, _value, _units, Style);

// On ComputedBorderStyle (intermediate):
public ComputedBorderStyle WithTopColor(Color32 color)
    => WithTop(Top.WithColor(color));    // copies edge, then copies border

// On ComputedStyle (root):
public ComputedStyle WithBorderTopColor(Color32 color)
    => WithBorder(Border.WithTopColor(color));    // copies border, then copies root
```

Changing `border-top-color` thus copies three objects: the edge struct (8 bytes, inline), the `ComputedBorderStyle` (one allocation), and the `ComputedStyle` root (one allocation). Everything else — sizes, backgrounds, inherited styles, rare fields — is shared by reference with the original. This is the copy-on-write discipline in action: the cost is proportional to the depth of the changed property (2–3 levels), not to the total size of the style.

### Default instances

Every computed style class has a `static Default` property containing an instance with all CSS initial values. These defaults serve three purposes:

1. **Root style.** Elements without a parent (or not in a styled tree) receive `ComputedStyle.Default`.
2. **Non-inherited reset.** `MakeChildStyle()` uses defaults for all non-inherited branches.
3. **`initial` keyword.** When a property is declared as `initial`, its value is copied from the corresponding default instance.

Because defaults are shared singleton instances, the common case where most properties are at their initial values is very memory-efficient: multiple elements whose styles differ only in a few properties share most of the same sub-objects.
