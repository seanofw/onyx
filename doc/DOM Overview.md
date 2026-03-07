# DOM Overview

## What is the DOM?

The Onyx DOM is a tree of nodes that represents a parsed HTML document. It is the central data structure of the library: the HTML parser produces it, CSS selectors query it, computed styles attach to it, and (eventually) the layout and rendering engines will consume it.

The DOM is inspired by the JavaScript DOM but is not a faithful recreation of it. It follows .NET naming conventions (`TitleCase`), uses standard .NET collection interfaces (`IList<Node>`, `IDictionary<string, string>`), and adds Linq-compatible extension methods for traversal and querying. Legacy DOM APIs that don't fit well in C# have been removed or replaced.

## Class Hierarchy

```
Node (abstract)
├── SimpleNode (abstract)
│   ├── TextNode
│   └── CommentNode
└── ContainerNode (abstract) : IList<Node>
    ├── Element : IAttributeNode
    │   └── LeafElement (abstract)
    │       ├── ImageElement
    │       ├── InputElement
    │       ├── ButtonElement
    │       └── ...
    ├── Document : IElementLookupContainer, IStyleRoot
    └── DocumentFragment
```

Every object in the tree is a `Node`. Nodes split into two families:

- **`SimpleNode`** — a leaf that cannot have children. `TextNode` and `CommentNode` inherit from this. Any attempt to add children to a `SimpleNode` throws `NotSupportedException`.
- **`ContainerNode`** — a node that can have children. `Element`, `Document`, and `DocumentFragment` inherit from this. `ContainerNode` implements `IList<Node>`, so children can be manipulated with standard collection operations.

`Element` is the workhorse: it represents an HTML element with a tag name, attributes, class names, inline styles, and a computed style. `LeafElement` is a subclass of `Element` that prohibits children (for elements like `<img>`, `<input>`, and `<br>`).

`Document` is the full-featured root of a DOM tree. It maintains fast lookup tables for elements by ID, class name, tag name, and other attributes, and it owns the style system (stylesheets, style computation, and a style invalidation queue).

`DocumentFragment` is a lightweight alternative root that omits the lookup tables and style system. Use it for temporary or throwaway DOM fragments where you don't need styling or fast queries.

## Node Identity and Position

Every node knows its place in the tree:

- **`Parent`** — the containing `ContainerNode` (null if detached or if this is the root).
- **`Root`** — the tree root (`Document` or `DocumentFragment`). Propagated recursively when nodes are attached or detached.
- **`Index`** — the node's zero-based position in its parent's child list. This enables O(1) access to `NextSibling` and `PreviousSibling` (computed as `Parent.Children[Index ± 1]`), rather than storing explicit sibling pointers.

Nodes also carry a `SourceLocation` if they were produced by the HTML parser, recording the filename, line, column, and character offset of the original source text.

## Tree Manipulation

`ContainerNode` provides the standard set of tree-manipulation methods:

| Method | Description |
|---|---|
| `AppendChild(node)` | Add a child at the end. |
| `InsertBefore(newNode, refNode)` | Insert before a reference child. |
| `RemoveChild(node)` | Remove a child. |
| `ReplaceChild(newNode, refNode)` | Replace a child with a new node. |
| `Clear()` | Remove all children. |
| `Insert(index, node)` | Insert at a specific index. |
| `RemoveAt(index)` | Remove by index. |

These methods enforce structural invariants:

- **Cycle prevention** — you cannot make a node its own ancestor.
- **Type validation** — only `Element`, `TextNode`, and `CommentNode` can be children.
- **Automatic reparenting** — appending a node that already has a parent detaches it from the old parent first.
- **Read-only enforcement** — containers can be marked read-only via `RenderFlags.ReadOnlyContainer`, which causes mutation methods to throw.

All tree mutations trigger attach/detach notifications (see [Lifecycle Hooks](#lifecycle-hooks) below) and maintain incremental subtree counts.

There are also `internal` "fast and unsafe" variants (`AppendChildFastAndUnsafe`, etc.) used by the HTML parser that skip some validation for speed. These are not part of the public API.

## Child Storage (`NodeList<T>`)

*(Internal — documented for completeness.)*

Children are stored in a `NodeList<T>`, a hybrid data structure that adapts based on child count:

- **8 or fewer children** — stored in a flat array. Most real-world elements have few children, so this keeps the common case cache-friendly and allocation-light.
- **More than 8 children** — switches to an `ImmutableList<T>` (a balanced AVL tree), which provides O(log n) insertion and removal instead of O(n) array shifts.

The list switches back to array storage when it shrinks to 4 or fewer items (half the threshold), providing hysteresis to avoid thrashing at the boundary.

When a container has no children at all, the internal storage is null — no array or list is allocated.

## Attributes (`AttributeDictionary`)

Element attributes are stored in an `AttributeDictionary`, which implements `IDictionary<string, string>`. Key behaviors:

- **Case-insensitive keys** — all attribute names are lowercased on insertion, so `class`, `Class`, and `CLASS` are the same key.
- **Change notifications** — every modification calls `Element.OnAttrChange()`, which updates internal caches (ID, class names), element lookup tables, and triggers style invalidation when needed.
- **Lazy initialization** — the backing dictionary is not allocated until the first attribute is added.
- **Attribute values are always strings** — there is no null; valueless attributes (like `disabled`) are stored with `string.Empty`.

## Element Identity Caching

For selector matching performance, `Element` caches two pieces of identity information as direct fields rather than deriving them from the attribute dictionary on every access:

- **`_id`** — the element's `id` attribute value, cached as a `string`.
- **`_classNames`** — the element's `class` attribute parsed into a `HashSet<string>`, cached and reused. Elements with no classes share a single static empty set to avoid allocations.

These caches are updated automatically when the `id` or `class` attributes change via `OnAttrChange()`.

## Element Lookup Tables

*(Internal — documented for completeness.)*

`Document` maintains an `ElementLookupTables` instance that indexes every element in the tree by:

- **ID** — for `getElementById`-style lookups.
- **Class name** — for `.className`-style lookups.
- **Tag name** — for `getElementsByTagName`-style lookups.
- **Name attribute** — for `getElementsByName`-style lookups.
- **Type attribute** — for elements with a `type` attribute (e.g., `<input type="text">`).

All lookups are O(1) dictionary lookups that return a `HashSet<Element>`. The tables are maintained automatically via the attach/detach hooks: when an element enters the tree, it is added to all relevant tables; when it leaves, it is removed.

The lookup tables also include:

- A **parsed selector cache** (1,024 entries) to avoid re-parsing the same CSS selector string repeatedly.
- **Query plan caches** for optimized selector execution.

Empty `HashSet<Element>` instances are pooled (up to 64) to reduce GC pressure from elements being added and removed frequently.

`DocumentFragment` does not have lookup tables. Selector queries on a `DocumentFragment` work but fall back to tree traversal instead of indexed lookup.

## Lifecycle Hooks

When nodes are added to or removed from the tree, four notifications fire in sequence:

1. **`OnAttaching`** — called before the node is linked to its new parent.
2. **`OnAttached`** — called after the node is linked. Updates element lookup tables (if the root is a `Document`).
3. **`OnDetaching`** — called before the node is unlinked. Removes from element lookup tables.
4. **`OnDetached`** — called after the node is unlinked.

Subclasses can override `OnAttach(AttachmentAction, ContainerNode)` to respond to any of these phases. The `AttachmentAction` enum distinguishes `Attaching`, `Attached`, `Detaching`, and `Detached`.

When a subtree is attached to or detached from a `Document`, `Root` is propagated recursively through the entire subtree via `SetRoot()`. This also manages style queue enrollment: elements entering a `Document` are enqueued for style computation; elements leaving are dequeued.

## Subtree Counts

`ContainerNode` maintains three incrementally-updated counts:

- **`SubtreeNodeCount`** — total number of nodes in the subtree (including self).
- **`SubtreeElementCount`** — total number of elements in the subtree (including self if this is an `Element`).
- **`ChildElementCount`** — number of immediate child elements.

These are updated in O(depth) time whenever nodes are added or removed, by walking up the ancestor chain and adjusting counts. This makes queries like "how many elements are in this subtree?" an O(1) property read instead of a tree traversal.

## Selectors and Querying

Selector operations are available at multiple levels:

**On individual nodes:**

| Method | Description |
|---|---|
| `IsMatch(selector)` | Test whether this element matches a CSS selector. |
| `Find(selector)` | Find all descendant elements matching a selector. |
| `Get(selector)` | Get the first matching descendant in document order. |

**On `IEnumerable<Node>` (via extension methods):**

| Method | Description |
|---|---|
| `Find(selector)` | Find matching descendants across all nodes in the sequence. |
| `Where(selector)` | Filter to elements matching the selector. |
| `Except(selector)` | Filter to elements *not* matching the selector. |
| `Closest(selector)` | Find the closest matching ancestor of each node. |
| `Any(selector)` / `All(selector)` | Test whether any/all nodes match. |
| `Children()` / `Descendants()` | Collect child or descendant elements. |
| `Parents()` / `Ancestors()` | Collect parent or ancestor elements. |
| `HasClass(name)` | Filter to elements with a given class. |
| `AddClass(name)` / `RemoveClass(name)` / `ToggleClass(name)` | Modify classes across a collection. |
| `OrderByPosition()` | Sort elements into document order. |

All of these accept selectors as strings, `Selector` objects, or `CompoundSelector` objects. String selectors are parsed once and cached.

**Results are sets.** Many query methods — `Find()`, `Closest()`, `Children()`, `Descendants()`, `Parents()`, `Ancestors()` — return `IReadOnlySet<Element>` or `IReadOnlyCollection<Element>` backed by a `HashSet<Element>`. This is intentional: set return types enable efficient set operations on query results (membership testing, intersection, union, subtraction) without requiring the caller to convert the result first.

**No ordering guarantees.** These sets do not guarantee document order. They are unordered for performance — imposing document order requires ancestry comparisons that are unnecessary when the caller just needs "all matches." This is a deliberate departure from the JS DOM, where `querySelectorAll()` always returns results in document order. If you need document order, chain `.OrderByPosition()`:

```csharp
// Unordered (fast):
IReadOnlySet<Element> items = doc.Find("li.active");

// Document-ordered (when you need it):
IEnumerable<Element> ordered = doc.Find("li.active").OrderByPosition();
```

When the root is a `Document`, `Find()` has a fast path for simple selectors: a pure `#id` selector becomes a direct lookup in the ID table, and a pure `.className` selector becomes a direct lookup in the class table, both O(1). Complex selectors fall back to tree traversal.

## Traversal

Beyond selector-based querying, the DOM provides multiple traversal APIs:

- **`Ancestors()` / `GetAncestors()`** — walk up the parent chain. The lazy `Ancestors()` returns an `IEnumerable`; the eager `GetAncestors()` returns a `List`. Both accept an optional `stopAt` node.
- **`Descendants()` / `GetDescendants()`** — walk the subtree depth-first. Same lazy/eager split.
- **`NextSibling` / `PreviousSibling`** — O(1) access to adjacent siblings.
- **`FirstChild` / `LastChild`** — O(1) access to the first and last children.
- **`Children`** — the full child list as `IReadOnlyList<Node>`.

All traversal methods have generic variants (`Ancestors<T>()`, `Descendants<T>()`) that filter by node type.

## Position Comparison

`CompareDocumentPosition(other)` returns a `DocumentPosition` flags enum indicating the relationship between two nodes: `Preceding`, `Following`, `Contains`, `ContainedBy`, or `Disconnected` (in different trees).

`ComparePosition(a, b)` is a simpler static method that returns -1, 0, or +1 for ordering. It has an optimized internal variant (`ComparePositionInternal`) that accepts reusable `AncestorList` buffers to avoid allocations during sorting.

`OrderByPosition()` on `IEnumerable<Node>` uses these comparisons to sort elements into document order.

## Computed Styles

Each `Element` has a `ComputedStyle` — the fully-resolved set of CSS properties that apply to it. Computed styles are:

- **Lazy** — not computed until `GetComputedStyle()` is called.
- **Cached** — once computed, the result is stored until invalidated.
- **Invalidated selectively** — `OnAttrChange()` checks the `StyleManager`'s tracking sets to determine whether a change to a particular attribute could affect any CSS rule. Only attributes that are actually referenced by a loaded selector trigger style invalidation.
- **Cascade-invalidated** — when stylesheets are added or removed from a `Document`, all elements in the tree have their computed styles invalidated.

When a computed style is invalidated, the element is added to the `Document`'s `StyleQueue`. Calling `Document.ValidateComputedStyles()` processes the queue and recomputes all invalid styles.

Style computation itself is recursive: computing an element's style may require its parent's style to be computed first (for inheritance), which may require the grandparent's, and so on.

## Serialization

Every node can serialize itself back to HTML via `ToString()`:

- **`Element`** — emits `<tagname attrs>children</tagname>`, or just `<tagname attrs>` for auto-closing tags.
- **`TextNode`** — emits the text content, HTML-encoded.
- **`CommentNode`** — emits `<!--text-->`.
- **`Document` / `DocumentFragment`** — emits the concatenation of all children (no wrapper element).

`InnerHtml` (get) serializes the children of a container without the container itself. `OuterHtml` (get) serializes the container and its children. Both have setters that parse HTML and replace the content.

## Enums

| Enum | Purpose |
|---|---|
| `NodeType` | JS DOM compatible node type identifier (`Element`, `Text`, `Comment`, `Document`, `DocumentFragment`, etc.). |
| `DocumentPosition` | Flags enum for `CompareDocumentPosition()` results. |
| `AttachmentAction` | Identifies the phase of a tree attach/detach operation (`Attaching`, `Attached`, `Detaching`, `Detached`). |
| `StyleFlags` | Bit flags for CSS pseudo-class states (`Hover`, `Active`, `Focus`, `Disabled`, `Visited`, `Checked`, `Indeterminate`). Stored on each `Node`. |
| `RenderFlags` | Bit flags for rendering state (`Visible`, `NeedsRepaint`, `NeedsReflow`, `VertScroll`, `HorzScroll`, `ReadOnlyContainer`). Stored on each `Node`. |

## Exceptions

| Exception | When |
|---|---|
| `HierarchyException` *(internal)* | Tree manipulation would create a cycle, or the child type is invalid for the parent. |
| `NotSupportedException` | Attempting to add children to a `SimpleNode` (or any `LeafElement`). |
| `InvalidOperationException` | Attempting to modify a read-only container. |
| `ArgumentOutOfRangeException` | Index out of bounds on child access. |

## Thread Safety

DOM objects are **not thread safe**. Do not mutate a DOM tree from multiple threads simultaneously. However, there is no thread affinity — any thread can own and manipulate a DOM tree, as long as it is the only thread doing so at that time.

Immutable objects produced from the DOM (such as `ComputedStyle`, `Stylesheet`, `HtmlToken`, `SourceLocation`) are safe to share across threads.

## Key Interfaces

| Interface | Implemented By | Purpose |
|---|---|---|
| `IAttributeNode` | `Element` | Marks nodes that have attributes (`Id`, `ClassName`, `ClassNames`, `Attributes`). |
| `IElementLookupContainer` *(internal)* | `Document` | Provides indexed element lookup by ID, class, tag name, etc. |
| `IStyleRoot` *(internal)* | `Document` | Provides access to the `StyleManager` and `StyleQueue` for style computation. |

## Differences from the JavaScript DOM

The Onyx DOM is inspired by the JavaScript DOM but is intentionally not compatible with it. Full JS DOM compatibility is not in scope. The differences fall into three categories: naming conventions, simplified APIs, and new capabilities.

### Naming and conventions

- **`TitleCase` throughout.** All properties, methods, and types use .NET naming conventions. `childNodes` becomes `ChildNodes`, `getElementById` becomes `GetElementsById`, `nodeName` becomes `NodeName`, and so on.
- **Standard .NET types.** Collections are `IReadOnlyList<Node>`, `IDictionary<string, string>`, `IReadOnlySet<string>`, and `HashSet<Element>` — not custom DOM-specific collection types. This means the full surface area of Linq and the .NET collection APIs is immediately available.

### Simplified APIs

- **No live collections.** In the JS DOM, methods like `getElementsByClassName()` return live `HTMLCollection` objects that update automatically as the DOM changes. Onyx returns fixed snapshots instead. Live collections are a source of subtle bugs and performance surprises, and they complicate the implementation considerably. If you need a fresh result, call the method again.
- **No `getAttribute()`/`setAttribute()`.** Elements have an `Attributes` property that implements `IDictionary<string, string>`. You read and write attributes using normal dictionary syntax (`element.Attributes["href"]`), not through getter/setter methods. This integrates naturally with Linq, pattern matching, and all the other tools C# provides for working with dictionaries.
- **`Find()` replaces `querySelectorAll()`.** The verbose `querySelectorAll()` is replaced with the shorter `Find()`. There is no `querySelector()` equivalent; use `Get()` instead, which returns the first match in document order, or use `Find()` and take the first result. `Find()` also has fast paths for simple `#id` and `.class` selectors that `querySelectorAll()` typically lacks. Unlike `querySelectorAll()`, `Find()` does **not** guarantee document order — it returns an unordered set for performance. Use `.OrderByPosition()` when ordering matters.
- **`Document` is a simple container.** In the JS DOM, `document` is a complex singleton with many responsibilities (creating elements, managing events, holding global state). In Onyx, `Document` is just a `ContainerNode` with lookup tables and a style system attached. It can host multiple children (not just a `<body>`), it is not a singleton, and element construction is just `new Element("div")`.
- **`DocumentFragment` is `Document` minus lookups.** In the JS DOM, `DocumentFragment` is a special lightweight container with unique behavior around insertion. In Onyx, it is simply a `ContainerNode` that serves as a tree root without the lookup tables and style system of `Document`. There is no special insertion behavior.
- **No `nodeValue` complexity.** In the JS DOM, `nodeValue` has different meanings for different node types and is null for elements. In Onyx, `Value` exists on all nodes but returns null by default; only `TextNode` overrides it meaningfully. The separate `TextContent` property handles the common case of reading concatenated text from a subtree.

### New capabilities

- **First-class class manipulation.** `AddClass()`, `RemoveClass()`, `ToggleClass()`, `UpdateClass()`, and `HasClass()` are methods directly on `Element` — and on any `IEnumerable<Node>` via extension methods. There is no `classList` wrapper object. The `ClassNames` property is an `IReadOnlySet<string>` for O(1) membership testing, in addition to the traditional whitespace-separated `ClassName` string.
- **Rich Linq extensions.** Any `IEnumerable<Node>` gains selector-aware methods: `Find(selector)`, `Where(selector)`, `Except(selector)`, `Closest(selector)`, `Any(selector)`, `All(selector)`, `Children()`, `Descendants()`, `Parents()`, `Ancestors()`, and `OrderByPosition()`. These allow complex DOM queries to be expressed as Linq pipelines.
- **Lazy and eager traversal.** `Ancestors()` and `Descendants()` return lazy `IEnumerable<T>` sequences. `GetAncestors()` and `GetDescendants()` return eagerly-materialized `List<T>`. Both have generic variants for type filtering. This lets you choose between allocation-light iteration and convenient list-based access.
- **`OrderByPosition()`.** Since Onyx methods return unordered sets for performance, `OrderByPosition()` is provided as an explicit opt-in when document order is needed. The JS DOM generally guarantees document order implicitly, which is convenient but forces the implementation to do ordering work even when the caller doesn't need it.
- **`InnerHtml` and `OuterHtml` as properties.** Both have getters (serialize to HTML) and setters (parse HTML and replace content). The JS DOM has `innerHTML` and `outerHTML` with similar behavior, but Onyx's versions use the full `HtmlParser` for the setter, providing the same error recovery and structural enforcement as initial parsing.
- **Incremental subtree counts.** `SubtreeNodeCount` and `SubtreeElementCount` give O(1) answers to "how many nodes/elements are under this node?" — something the JS DOM has no equivalent for without tree traversal.

### Intentional omissions

- **No events.** The Onyx DOM does not (yet) have an event system. There are no `addEventListener`, `removeEventListener`, or event bubbling/capture phases. This is planned for a future version.
- **No `document.createElement()`.** Elements are constructed directly: `new Element("div")`. There is no factory method on `Document`.
- **No `window` object.** There is no global context. The `Document` is the top of the hierarchy.
- **No `Range` or `Selection`.** Text selection and range APIs are not implemented.
- **No mutation observers.** The attach/detach hooks (`OnAttach`) serve a similar purpose internally but are not exposed as a public observer API.

## Similarities to jQuery

Several of Onyx's APIs are directly inspired by jQuery's approach to DOM manipulation. jQuery's insight was that operating on *collections* of elements with a concise, chainable API is far more productive than the JS DOM's one-element-at-a-time approach. Onyx brings this same philosophy to C# through extension methods on `IEnumerable<Node>`.

| jQuery | Onyx equivalent | Notes |
|---|---|---|
| `$(selector)` | `document.Find(selector)` | Returns a set of matching elements. |
| `$(selector, context)` | `context.Find(selector)` | Scoped search within a subtree. |
| `.find(selector)` | `.Find(selector)` | Search descendants of a collection. |
| `.filter(selector)` | `.Where(selector)` | Filter collection to matching elements. |
| `.not(selector)` | `.Except(selector)` | Filter collection to non-matching elements. |
| `.closest(selector)` | `.Closest(selector)` | Find nearest matching ancestor of each element. |
| `.children()` | `.Children()` | Get immediate child elements. |
| `.children(selector)` | `.Children(selector)` | Get filtered child elements. |
| `.parents()` | `.Parents()` / `.Ancestors()` | Get parent or all ancestor elements. |
| `.parents(selector)` | `.Parents(selector)` / `.Ancestors(selector)` | Filtered ancestors. |
| `.hasClass(name)` | `.HasClass(name)` | Filter to elements with a class (or test on single element). |
| `.addClass(name)` | `.AddClass(name)` | Add class(es) to all elements in collection. |
| `.removeClass(name)` | `.RemoveClass(name)` | Remove class(es) from all elements in collection. |
| `.toggleClass(name)` | `.ToggleClass(name)` | Toggle class(es) on all elements in collection. |

The key difference is that jQuery wraps results in a custom `$` object for chaining, while Onyx uses standard `IEnumerable<Node>` and `IReadOnlySet<Element>` return types. This means Onyx's results compose naturally with Linq — you can freely mix selector-based filtering with Linq's `Select`, `Where`, `GroupBy`, `ToDictionary`, and everything else, without leaving the type system.

Like jQuery and unlike the JS DOM, most Onyx collection methods accept space-separated class names (e.g., `AddClass("active highlight")`) and operate on every element in the collection, not just a single element.

## Design Principles

**Lazy everything.** Attributes, class name sets, inline styles, computed styles, and child storage are all lazily initialized. An element with no attributes allocates no dictionary. An element whose style is never queried never computes one. A container with no children allocates no list.

**Cache at the boundary.** Identity data (ID, class names) is cached on the element as soon as it arrives, so selector matching never has to re-parse the `class` attribute or look up the `id` attribute in the dictionary.

**Incremental maintenance.** Subtree counts, element lookup tables, and style invalidation are maintained incrementally as the tree changes, rather than recomputed from scratch. This keeps common operations (adding a child, removing an element, checking subtree size) fast.

**Allocation awareness.** Empty collections are shared (static empty class name sets, `Array.Empty<Node>()`). HashSets in the lookup tables are pooled and reused. Struct enumerators avoid boxing. The hybrid `NodeList` avoids both the overhead of a tree for small lists and the O(n) cost of array shifts for large ones.
