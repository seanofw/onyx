# Onyx Architecture

## What Onyx is

Onyx is a standards-compliant HTML5 and CSS engine for .NET, implemented entirely in
managed C#.  It is not a browser; it is the part of a browser that developers have
always wished they could use independently.  Onyx parses HTML into a DOM, parses CSS
into stylesheets, matches selectors to elements, computes styles, and (once layout is
complete) renders the result — all without requiring a browser, a JavaScript runtime,
an HTTP client, or any external dependencies beyond .NET itself.

The compiled Onyx core is a single DLL of approximately 500 KB.  It has no third-party
dependencies.

Onyx is licensed under the MIT license.

## Design philosophy

Several principles govern every design decision in Onyx:

### Standards compliance is non-negotiable

Onyx targets HTML5 and CSS 2.1 with selected CSS 3 extensions.  You should be able to
drop in HTML and CSS from a web page and it should look the same.  Deviations from the
standards are bugs, not features.  The only non-standard additions are the `<row>` and
`<column>` elements, which are simply block elements with default flexbox styles — they
can be reproduced on any web page by including eight lines of CSS, so they are not truly
a deviation.

The goal is that major CSS libraries like Bootstrap or Tailwind should eventually work
in Onyx verbatim.  Standards conformance is the path to that goal.

### Loose coupling everywhere

Decoupling is not just a rendering concern — it is a core design consideration
throughout Onyx.  At every level, Onyx avoids direct dependencies between components
that most other software would couple together, and many components are designed to be
usable outside of the "normal" scenarios.

**Each parser is independent.**  `HtmlParser`, `CssSelectorParser`, `CssPropertyParser`,
and `CssMediaQueryParser` can each be instantiated and used on their own, without
creating a `Document` or loading a stylesheet.  You can parse a single CSS selector into
a `Selector` object, test it against elements, and never touch the style system.  You
can use `HtmlParser` as a standalone HTML5 parser and ignore CSS entirely.  You can
parse a single property declaration and inspect its structure.  The top-level
`CssParser` orchestrates all four, but none of them requires it.

**Elements have no direct dependency on `Document`.**  An `Element` does not know or
care whether its tree root is a `Document`, a `DocumentFragment`, or some other
`ContainerNode`.  It only needs *something* to act as a tree root.  The style system
is accessed through the internal `IStyleRoot` interface — if the tree root implements
it, styling works; if it doesn't, elements simply receive `ComputedStyle.Default`.
This means you can build and manipulate element trees without ever creating a
`Document`, and attach them to a `Document` later (or never).

**The style system has no overhead if you don't use it.**  If you never add a
stylesheet to a `Document`, the `StyleManager` is never populated, no indexes are
built, and `GetComputedStyle()` returns the default style immediately.  There is no
startup cost, no background processing, and no memory overhead for features you aren't
using.  The style queue sits empty.  Media queries are never evaluated.  The entire
style computation pipeline only activates when you actually load CSS.

**Rendering is fully abstracted.**  The core library contains all parsing, DOM
management, selector matching, style computation, and layout logic, but rendering is
abstracted behind an `IRenderer` interface.  Concrete renderers (such as `Onyx.Skia`
for SkiaSharp, or `Onyx.Windows` for native Windows rendering) are separate assemblies
that implement this interface.  This means Onyx can render onto a standalone window,
onto a region of an existing UI, or onto a bitmap.  An existing WPF or Avalonia
application can host an Onyx-rendered region for part of its UI without replacing
anything.  It also opens the door to renderers for other platforms — OpenGL, Vulkan,
Unity, or any other system that can draw rectangles and text.

**Visibility is intentionally permissive.**  Even many classes that are only intended
for internal use are marked `public`.  This is deliberate: it signals that these classes
are safe to use for purposes the library author didn't anticipate.  If you want to build
a CSS linter using Onyx's property parser, or a selector visualizer using the parsed
`Selector` tree, or a media query analyzer using the `MediaQuery` expression nodes, you
can — those types are public precisely so that unexpected use cases are not blocked by
access restrictions.  The few things that *are* internal (like `IStyleRoot`) are internal
because they represent implementation contracts that could change, not because they are
dangerous to use.

### Computer science cost over software engineering cost

Onyx prioritizes algorithmic efficiency over micro-optimization.  The biggest
performance differences between a good browser engine and a bad one are found in the
computer science: caching, laziness, copy-on-write, hash tables, balanced trees,
database-theory-style indexing, and keeping algorithmic orders as low as possible.  Onyx
makes nearly all the right choices at the algorithmic level, so that even though it runs
in managed C# rather than C++, it should still perform well enough for interactive use.

This means that throughout the codebase, you will find:

- **Heavy caching** — computed styles, compiled selectors, compiled media queries, and
  parsed inline styles are all cached and reused.
- **Lazy evaluation** — nothing is computed until it is needed, and invalidation marks
  things as stale without immediately recomputing them.
- **Copy-on-write** — immutable data structures are shared aggressively and only copied
  when a value changes, minimizing both allocation and the blast radius of mutations.
- **Index-driven lookups** — the style rule engine maintains hash-table indexes over
  element names, class names, and IDs, so that finding the rules that apply to an
  element is sublinear in the number of rules.
- **Selective invalidation** — when an element's attributes change, Onyx checks whether
  any stylesheet actually references those attributes before invalidating the element's
  computed style, avoiding unnecessary recomputation.

### JIT compilation and on-the-fly specialization

Several subsystems compile data structures into native code at runtime:

- **Selector `IsMatch()`**: The adaptive JIT compiler in the selector matching engine
  observes how many times each simple selector is tested.  After a threshold is reached,
  it compiles the selector's filter chain into a specialized delegate using
  `System.Linq.Expressions`, eliminating virtual dispatch overhead for hot selectors.

- **Selector `Find()`**: The query planner analyzes a compound selector's structure and
  selects an optimized execution strategy, choosing between index-driven lookups (by ID,
  class, or element name) and tree traversals, similar to how a database query optimizer
  chooses between index scans and table scans.

- **Media query evaluation**: Each `MediaQuery` expression tree can be compiled into a
  native delegate via `GetEval()`, so that media queries evaluated against every
  candidate rule on every element execute as JIT-compiled code rather than interpreted
  tree walks.

### Programmer ergonomics

Despite the complex internals, the external API is designed to be simple.  You don't
need to construct a parser or configure anything — the simplest possible Onyx program
is:

```csharp
Document doc = new Document("<h1>Hello, World</h1>");
```

That single line produces a complete, fully-parsed HTML document.  Many common
operations in Onyx can fit comfortably in a tweet:

```csharp
Document doc = new Document("<div class='foo'>Hello</div>");
IEnumerable<Element> foos = doc.Find(".foo");
bool isMatch = someElement.IsMatch("div.active > span");
ComputedStyle style = someElement.GetComputedStyle();
```

Onyx's extension methods on `IEnumerable<Element>` are designed to chain naturally with
standard Linq, so CSS selectors and C# lambdas can be mixed freely in a single
expression:

```csharp
document.Find("#foo .bar").Descendants(".baz").Where(e => e.Children.Any()).AddClass("qux");
```

This finds all `.bar` elements inside `#foo`, drills down to their `.baz` descendants,
keeps only those that have children, and adds the class `qux` to each — in one line,
using a mix of Onyx selector queries, Onyx tree traversal, a Linq predicate, and an
Onyx bulk mutation, all composed as a single readable chain.

The DOM provides Linq-compatible extension methods on `IEnumerable<Element>` —
`Find()`, `Where()`, `Closest()`, `Descendants()`, `Ancestors()`, `HasClass()`,
`AddClass()`, `RemoveClass()`, and more — so that complex DOM traversals can often be
expressed as simple Linq chains.

### Immutability as a correctness tool

Onyx uses immutable objects extensively: `Stylesheet`, `StyleRule`, `StyleProperty`,
`Selector`, `CompoundSelector`, `SimpleSelector`, `SelectorFilter`, `ComputedStyle`,
and all parsed tokens are immutable.  Immutability eliminates entire categories of bugs
(aliasing, unintended mutation, inconsistent state) and enables safe sharing across
threads or across the copy-on-write tree without defensive copying.

Where mutation is needed (applying a CSS property to a computed style, for example),
Onyx uses C# record types with `with` expressions, producing a new object with one
field changed while sharing all other data with the original.

### Documentation as a first-class deliverable

Onyx is intended to be not just usable but *understandable*.  The code is heavily
commented, and the documentation (the `doc/` directory) explains not just what the
code does but *why* — the algorithms, the data structures, the tradeoffs, and the
design decisions.  This serves two purposes: it makes adoption easier for users of
Onyx, and it provides a complete enough reference that someone could use the
documentation as a roadmap to port Onyx to another language.

---

## Major subsystems

The following diagram shows how Onyx's major pieces connect:

```
                        ┌─────────────────────────────────┐
                        │         User Application        │
                        └────────┬────────────────┬───────┘
                                 │                │
                    ┌────────────▼──┐     ┌───────▼────────┐
                    │   HtmlParser  │     │   CssParser    │
                    │  (HTML text   │     │  (CSS text     │
                    │   → Document) │     │   → Stylesheet)│
                    └────────┬──────┘     └───────┬────────┘
                             │                    │
                    ┌────────▼──────────────┐     │
                    │       Document        │◄────┘
                    │  (DOM tree + style    │  .AddStylesheet()
                    │   manager + style    │
                    │   queue + media ctx) │
                    └────────┬─────────────┘
                             │
                    ┌────────▼──────────────┐
                    │   StyleManager        │
                    │  (index-driven rule   │
                    │   lookup, selector    │
                    │   matching, media     │
                    │   query filtering,    │
                    │   specificity         │
                    │   resolution)         │
                    └────────┬─────────────┘
                             │
                    ┌────────▼──────────────┐
                    │   ComputedStyle       │
                    │  (copy-on-write tree  │
                    │   of final CSS        │
                    │   property values)    │
                    └────────┬─────────────┘
                             │
                    ┌────────▼──────────────┐
                    │   Layout Engine       │
                    │  (not yet impl.)      │
                    └────────┬─────────────┘
                             │
                    ┌────────▼──────────────┐
                    │   IRenderer           │
                    │  (Onyx.Skia,          │
                    │   Onyx.Windows, ...)  │
                    └───────────────────────┘
```

### HTML parsing

**Files:** `Onyx/Html/Parsing/`
**Documentation:** [HTML Parsing.md](HTML%20Parsing.md)

The HTML parser (`HtmlParser`) is a full, standards-compliant HTML5 parser.  It
consumes HTML text via `HtmlLexer`, handles automatic tag closing, complex recovery
rules for mismatched and missing tags, and produces a `Document` containing a tree of
`Node` objects (`Element`, `TextNode`, `CommentNode`, etc.).

The parser handles raw-text elements (`<style>`, `<script>`, `<xmp>`) and void elements
(`<img>`, `<br>`, `<input>`, etc.) per the HTML5 specification.  Errors are recorded as
warnings rather than thrown as exceptions, matching the HTML5 error-recovery philosophy.

### The DOM

**Files:** `Onyx/Html/Dom/`
**Documentation:** [DOM Overview.md](DOM%20Overview.md)

The DOM is a tree of `Node` objects rooted at a `Document`.  The class hierarchy is:

```
Node
├── ContainerNode
│   ├── Document ............ tree root; holds StyleManager, StyleQueue, MediaInfo
│   ├── DocumentFragment .... lightweight container without fast-lookup indexes
│   └── Element ............. the core building block; has attributes, classes, styles
│       └── LeafElement ..... elements that cannot have children (img, input, br, ...)
├── TextNode ................ leaf containing text content
└── CommentNode ............. leaf containing an HTML comment
```

`Document` implements the internal `IStyleRoot` interface, which connects it to the
style system by providing a `StyleManager`, a `StyleQueue`, and a `MediaQueryContext`.
`Document` also maintains fast-lookup tables for elements by ID, class, and tag name.

The DOM API is designed for modern C# rather than JavaScript compatibility.  It provides
Linq-compatible extension methods, set-based classname management, and dictionary-based
attribute access.

### CSS parsing

**Files:** `Onyx/Css/Parsing/`
**Documentation:** [Selector Parsing.md](Selector%20Parsing.md),
[CSS Property Parsing.md](CSS%20Property%20Parsing.md),
[CSS Media Queries.md](CSS%20Media%20Queries.md)

CSS parsing is handled by four cooperating parsers, all orchestrated by `CssParser`:

| Parser | Responsibility |
|--------|---------------|
| `CssParser` | Top-level stylesheet structure: rule blocks, `@media`, `@supports` |
| `CssSelectorParser` | Selector syntax → `Selector` / `CompoundSelector` objects |
| `CssPropertyParser` | Property declarations → `StyleProperty` objects |
| `CssMediaQueryParser` | Media query conditions → `MediaQuery` expression trees |

All four parsers share a single `Messages` collection for unified error/warning
reporting.  They all use the same `CssLexer` for tokenization and the same position-
based backtracking mechanism.

**The property parser** uses a declarative grammar (effectively a PEG) built from
composable `Syntax<TProp>` nodes via a fluent `SyntaxBuilder` API.  Each CSS property
is defined as a grammar rule in `PropertySyntaxDefinitions`, making it straightforward
to compare the implementation against the CSS specification.  The parser supports 90+
CSS properties across approximately 140 `StyleProperty` subclasses.

**The selector parser** produces immutable `Selector` objects that carry pre-computed
specificity values and support both `IsMatch()` (test one element) and `Find()` (search
a tree).

**The media query parser** produces `MediaQuery` expression trees that implement
Kleene three-valued logic, as required by the CSS Media Queries Level 4 specification.

### Selector matching

**Files:** `Onyx/Css/Selectors/`, `Onyx/Css/StyleManager.cs`
**Documentation:** [Selector Matching.md](Selector%20Matching.md)

There are three selector engines, each optimized for a different use case:

1. **The `IsMatch()` engine** — tests whether a single element matches a single
   selector.  Uses right-to-left path walking and an adaptive JIT compiler that
   promotes hot selectors from interpreted to compiled.

2. **The `Find()` engine** — given a selector and a subtree, locates all matching
   elements.  Includes a query planner that chooses between index-driven and
   traversal-based strategies depending on the selector's structure and the
   available indexes.

3. **The style rule engine** — given an element, locates all stylesheet rules that
   match it.  Maintains hash-table indexes over the last simple selector of each rule,
   enabling sublinear candidate generation.  Filters candidates through media query
   evaluation before full selector matching.

### Style computation

**Files:** `Onyx/Css/StyleManager.cs`, `Onyx/Css/Computed/`, `Onyx/Html/Dom/Element.cs`
**Documentation:** [Computed Styles.md](Computed%20Styles.md)

The style computation pipeline transforms parsed stylesheets into per-element computed
styles:

1. **Find candidate rules** — index-driven lookup in `StyleManager`.
2. **Filter by media query** — evaluate `@media` conditions against the document's
   `MediaQueryContext`.
3. **Match selectors** — full `IsMatch()` verification of each candidate.
4. **Add inline styles** — the element's `style` attribute, at maximum specificity.
5. **Resolve specificity** — decompose shorthands, pick the highest-specificity winner
   for each property.
6. **Apply to computed style** — produce a new `ComputedStyle` by applying each
   winning property to the parent's inherited style.

The result, `ComputedStyle`, is an immutable copy-on-write tree of data objects.  It is
2-3 levels deep with approximately 20 leaf objects, designed so that changing a single
property copies only the sub-object that contains it.  Inherited properties
(font, color, text settings) are shared by reference between parent and child styles
via a separate `ComputedInheritedStyle` subtree.

**Style invalidation** is selective: when an element's attributes change, Onyx checks
whether any stylesheet actually references those attributes before invalidating the
element's style.  Class changes use symmetric difference against the set of classnames
used by stylesheets.  The `StyleQueue` on the `Document` batches invalidated elements
for breadth-first recomputation.

### Media queries

**Files:** `Onyx/Css/Types/Media/`
**Documentation:** [CSS Media Queries.md](CSS%20Media%20Queries.md)

Media queries allow stylesheet rules to be conditional on the properties of the output
device.  The media query system splits the device description into two parts:

- **`MediaDimensions`** — width, height, aspect ratio, orientation.  These are dynamic
  and may change when the display is resized.
- **`MediaInfo`** — media type, color depth, pointer capabilities, hover support,
  overflow mode, update speed.  These are static and set once at startup.

The `UsesDimensions` flag on each `MediaQuery` node tracks whether the query depends on
the dynamic half, enabling a future optimization where only dimension-dependent elements
are invalidated on resize.

### Layout (not yet implemented)

The layout engine will consume computed styles and produce a tree of layout boxes with
concrete positions and sizes.  It will support normal flow, floats, flexbox, and tables.

### Rendering

**Files:** (external assemblies: `Onyx.Skia`, `Onyx.Windows`)

Rendering is decoupled from the core via the `IRenderer` interface.  The core library
produces layout boxes; a renderer draws them.  This means the same document can be
rendered by different backends without any changes to the document, stylesheets, or
application code.

---

## Project structure

```
Onyx/
├── Html/
│   ├── Parsing/ .............. HtmlParser, HtmlLexer, HtmlToken
│   └── Dom/ .................. Node, Element, Document, ContainerNode,
│                               TextNode, CommentNode, LeafElement,
│                               IEnumerableOfNodeExtensions
├── Css/
│   ├── Parsing/ .............. CssParser, CssLexer, CssSelectorParser,
│   │                           CssPropertyParser, CssMediaQueryParser,
│   │                           CssSupportsQueryParser
│   ├── Selectors/ ............ Selector, CompoundSelector, SimpleSelector,
│   │                           SelectorFilter, Combinator, Specificity
│   ├── Properties/ ........... StyleProperty (abstract), StylePropertySet,
│   │                           SyntaxBuilder, PropertySyntaxDefinitions,
│   │                           KnownProperties/ (~140 property classes)
│   ├── Computed/ .............. ComputedStyle and its ~20 sub-objects
│   ├── Types/ ................ Measure, Units, EdgeSizes, SizeConstraints
│   │   └── Media/ ............ MediaQuery tree, MediaInfo, MediaDimensions,
│   │                           MediaQueryContext, feature/type enums
│   ├── StyleManager.cs ....... Rule indexing, candidate lookup, style computation
│   ├── StyleRule.cs .......... Selector + properties + optional media query
│   ├── Stylesheet.cs ......... Immutable list of StyleRules
│   └── StyleQueue.cs ......... Batched invalidation queue
├── Types/ .................... Color32, Vector, Size, Rect value types
├── Extensions/ ............... Hyphenize, StringExtensions, HtmlEntities
├── Message.cs, Messages.cs ... Shared diagnostic infrastructure
└── SourceLocation.cs ......... File/line/column tracking for error reporting
```

---

## Threading model

Onyx objects are **not thread-safe** but have **no thread affinity**.  There is no "UI
thread."  Any thread may own and manipulate a `Document` tree, and ownership can be
transferred between threads, but only one thread may access a tree at a time.  The same
rules apply as for `Dictionary<K, V>` or `List<T>`.

Many Onyx classes are immutable (`Stylesheet`, `StyleRule`, `StyleProperty`, `Selector`,
`ComputedStyle`, all tokens), and immutable objects are inherently thread-safe.  A
`Stylesheet` parsed on one thread can be safely added to `Document` objects on other
threads.  The `Messages` collection is explicitly thread-safe.

---

## Data flow summary

```
HTML text ──► HtmlParser ──► Document (DOM tree)
                                │
CSS text ──► CssParser ──► Stylesheet ──► Document.AddStylesheet()
                                │                │
                                │    ┌───────────▼───────────┐
                                │    │    StyleManager        │
                                │    │  indexes rules by      │
                                │    │  element/class/ID      │
                                │    └───────────┬───────────┘
                                │                │
                           attribute change      │
                           or class change       │
                                │                │
                                ▼                ▼
                          ┌─ StyleQueue ──► GetComputedStyle() ─┐
                          │                                      │
                          │  1. FindCandidateRules (index lookup) │
                          │  2. Filter by @media query            │
                          │  3. IsMatch() each candidate          │
                          │  4. Add inline styles                 │
                          │  5. Resolve specificity               │
                          │  6. Apply to ComputedStyle            │
                          │                                      │
                          └──────────────► ComputedStyle ────────┘
                                               │
                                          Layout Engine
                                               │
                                          IRenderer
```

---

## Relationship to other documentation

| Document | Covers |
|----------|--------|
| [HTML Parsing.md](HTML%20Parsing.md) | The HTML parser: lexer, token types, tag recovery, entity handling |
| [DOM Overview.md](DOM%20Overview.md) | The DOM tree: Node hierarchy, Element, Document, attribute handling |
| [Selector Parsing.md](Selector%20Parsing.md) | How CSS selectors are parsed into Selector objects |
| [Selector Matching.md](Selector%20Matching.md) | The three selector engines: IsMatch, Find, and the style rule engine |
| [CSS Property Parsing.md](CSS%20Property%20Parsing.md) | The declarative property parser grammar and all supported properties |
| [CSS Media Queries.md](CSS%20Media%20Queries.md) | Media query parsing, expression trees, and evaluation |
| [Computed Styles.md](Computed%20Styles.md) | The style computation pipeline and the ComputedStyle data structure |
