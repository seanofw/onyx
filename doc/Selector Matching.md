# Selector Matching

## Overview

This document describes Onyx's CSS selector matching engines — the runtime systems that test whether a given element satisfies a selector, and that search a DOM tree for all elements matching a selector. These engines operate on the selector object tree produced by the parser (documented in [CSS Selector Parsing.md](CSS%20Selector%20Parsing.md)).

There are three matching engines:

1. **The IsMatch engine** — tests a single element against a selector. Given an element and a selector, it returns `true` or `false`. This is the fundamental matching primitive on which everything else is built.
2. **The Find engine** — searches a subtree for all elements matching a selector. It uses IsMatch internally, but pairs it with a query planner that narrows the candidate set before testing.
3. **The Style Rule engine** — given an element, locates all stylesheet rules whose selectors match it. It uses IsMatch internally, but pairs it with its own set of indexes to narrow the candidate rules before testing. This engine is specific to CSS style computation but is architecturally another selector engine.

All three engines are used pervasively: `IsMatch()` is the core primitive, `Find()` is called by application code performing jQuery-style queries on the DOM, and the style rule engine is called during style computation to determine which CSS rules apply to each element.

> **Visibility conventions in this document:** Items marked *(internal)* are accessible within the Onyx assembly but are not part of the public API. Items marked *(private)* are implementation details of their containing class.

---

## The IsMatch Engine

### Entry points

There are three levels at which `IsMatch()` can be called, corresponding to the three levels of the selector object hierarchy:

| Class | Signature | Semantics |
|-------|-----------|-----------|
| `CompoundSelector` | `IsMatch(Node? node)` | Returns `true` if the node is an `Element` that matches **any** of the comma-separated selectors (OR logic) |
| `Selector` | `IsMatch(Element? element)` | Returns `true` if the element matches this single selector chain |
| `SimpleSelector` | `IsMatch(Element element)` | Returns `true` if the element matches this leaf-level selector (tag name + filters) |

In practice, `Selector.IsMatch()` is the primary entry point. `CompoundSelector.IsMatch()` delegates to it, and `Selector.IsMatch()` delegates to `SimpleSelector.IsMatch()` for each component.

### How `Selector.IsMatch()` works

A `Selector` stores its components as a `Path` array in **left-to-right order** — the same order they appear in the CSS text. For the selector `div.container > span.active`, the path is:

```
Path[0]:  Self         div.container
Path[1]:  Child        span.active
```

But matching proceeds **right-to-left**, because the question is: "Does this element match?" The element being tested corresponds to the *rightmost* component, and matching walks backward (toward the left) through the path, climbing the DOM tree as it goes.

The algorithm:

```
IsMatch(element):
    1. Test the rightmost SimpleSelector against the element.
       If it doesn't match, return false immediately.
    2. Call RecursivelyTestPath() to walk the rest of the path
       backward, using each component's Combinator to determine
       which DOM node to test next.
    3. If the path is exhausted (index < 0), all components matched:
       return true.
```

In code (`Selector.cs:101–116`):

```csharp
public bool IsMatch(Element? element)
{
    if (element == null || Path.Count == 0)
        return false;

    // Test the rightmost simple selector against the element directly.
    SimpleSelector simpleSelector = Path[^1].SimpleSelector;
    if (!simpleSelector.IsMatch(element))
        return false;

    // Walk backward through the path, testing combinators.
    return RecursivelyTestPath(Path.Count - 2, Path[^1].Combinator, element);
}
```

### `RecursivelyTestPath` — combinator traversal *(private)*

This is the heart of the matching engine. It takes a path index, a combinator, and a DOM node, and tests the selector component at that index against the DOM node indicated by the combinator. If the component matches, it recurses to test the next component to the left, until the path is exhausted.

The method handles each combinator differently:

#### `Self` — chained selectors

The `Self` combinator appears on the first (leftmost) component of every selector. It means "test this component against the same element" — there is no DOM traversal.

```csharp
case Combinator.Self:
    if (!(node is Element element) || !simpleSelector.IsMatch(element))
        return false;
    if (index <= 0)
        return true;
    return RecursivelyTestPath(index - 1, Path[index - 1].Combinator, node);
```

#### `Child` (`>`) — immediate parent

Tests only the direct parent of the current node. If the parent doesn't match, the entire selector fails — there is no backtracking.

```csharp
case Combinator.Child:
    Node? parent = node.Parent;
    if (!(parent is Element element) || !simpleSelector.IsMatch(element))
        return false;
    if (index <= 0)
        return true;
    return RecursivelyTestPath(index - 1, Path[index - 1].Combinator, parent);
```

#### `Descendant` (space) — any ancestor

Walks up the ancestor chain, testing each ancestor element. Because multiple ancestors might match the simple selector, and different ancestors might lead to different outcomes for the *rest* of the path, this combinator introduces **backtracking**: if one ancestor matches but the remainder of the path fails, it continues walking upward to try the next ancestor.

```csharp
case Combinator.Descendant:
    for (Node? ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
    {
        if (!(ancestor is Element element) || !simpleSelector.IsMatch(element))
            continue;

        if (index <= 0)
            return true;
        if (RecursivelyTestPath(index - 1, Path[index - 1].Combinator, ancestor))
            return true;
    }
    return false;
```

This is the only combinator where the worst-case complexity can be exponential (in the depth of nesting multiplied by the length of the selector path). In practice, real-world selectors and DOM trees almost never trigger this — the recursion stays close to linear.

#### `AdjacentSibling` (`+`) — previous element sibling

Finds the immediately preceding *element* sibling (skipping text nodes, comment nodes, etc.) and tests it. Like `Child`, there is no backtracking — only one candidate exists.

```csharp
case Combinator.AdjacentSibling:
    Node? prev = node.PreviousSibling;
    while (prev != null && !(prev is Element))
        prev = prev.PreviousSibling;
    if (!(prev is Element element) || !simpleSelector.IsMatch(element))
        return false;
    if (index <= 0)
        return true;
    return RecursivelyTestPath(index - 1, Path[index - 1].Combinator, prev);
```

#### `GeneralSibling` (`~`) — any sibling

Tests all siblings (both previous and following) of the current node. Like `Descendant`, this introduces backtracking because multiple siblings might match.

```csharp
case Combinator.GeneralSibling:
    ContainerNode? parent = node.Parent;
    if (parent == null)
        return false;
    IReadOnlyList<Node> childNodes = parent.ChildNodes;

    // Search backward through preceding siblings.
    for (int i = node.Index - 1; i >= 0; i--)
    {
        Node prev = childNodes[i];
        if (!(prev is Element element) || !simpleSelector.IsMatch(element))
            continue;
        if (index <= 0)
            return true;
        if (RecursivelyTestPath(index - 1, Path[index - 1].Combinator, prev))
            return true;
    }

    // Search forward through following siblings.
    for (int i = node.Index + 1; i < childNodes.Count; i++)
    {
        Node next = childNodes[i];
        if (!(next is Element element) || !simpleSelector.IsMatch(element))
            continue;
        if (index <= 0)
            return true;
        if (RecursivelyTestPath(index - 1, Path[index - 1].Combinator, next))
            return true;
    }
    return false;
```

Note that the general sibling combinator searches *both directions* from the current node. This is broader than the CSS specification, which only matches *preceding* siblings for `~`. The Onyx implementation matches any sibling, which is a superset of the standard behavior.

### Worked example

Consider matching the selector `div > ul li.active` against an element. The path is:

```
Path[0]:  Self         div
Path[1]:  Child        ul
Path[2]:  Descendant   li.active
```

When `IsMatch(element)` is called:

1. Test `Path[2]` (`li.active`) against the element. If the element isn't an `<li>` with class `active`, return `false`.
2. The combinator on `Path[2]` is `Descendant`, so call `RecursivelyTestPath(1, Descendant, element)`.
3. Walk up the element's ancestors. For each ancestor:
   - Test `Path[1]` (`ul`) against the ancestor. If this ancestor is a `<ul>`:
     - The combinator on `Path[1]` is `Child`, so call `RecursivelyTestPath(0, Child, ancestor)`.
     - Test `Path[0]` (`div`) against the `<ul>`'s immediate parent. If the parent is a `<div>`:
       - Index is 0 and it matched — return `true` all the way up the call stack.
     - If the parent isn't a `<div>`, this branch fails. Backtrack and try the next ancestor.
4. If no ancestor chain satisfies the full path, return `false`.

---

## `SimpleSelector.IsMatch()` — the adaptive JIT compiler

`SimpleSelector` is where individual element matching actually happens: testing the tag name and each filter against the element. This is also where Onyx's most distinctive performance optimization lives — **adaptive compilation of selectors into LINQ expression trees**.

### The two execution modes

Every `SimpleSelector` starts life as an interpreter. On its first and second invocations, it tests the element by walking its tag name and filter list in a straightforward loop. On the **third invocation**, it compiles itself into a native delegate via `System.Linq.Expressions` and uses that delegate for all subsequent calls.

```csharp
public bool IsMatch(Element element)
{
    if (++_usageCount >= 3)
    {
        _compiledMatchFunc ??= CompileMatchFunc();
        return _compiledMatchFunc(element);
    }
    else
    {
        // Interpreted path: test element name, then each filter.
        if (!string.IsNullOrEmpty(ElementName) && ElementName != "*")
        {
            if (!string.Equals(element.NodeName, ElementName,
                StringComparison.InvariantCultureIgnoreCase))
                return false;
        }

        foreach (SelectorFilter filter in Filters)
        {
            if (!filter.IsMatch(element))
                return false;
        }

        return true;
    }
}
```

The threshold of 3 uses is a tradeoff: compiling an expression tree has a one-time cost (roughly 50–200 microseconds depending on complexity), so selectors that are only tested once or twice run faster without compilation. But selectors in CSS stylesheets are typically tested against every element in the document, so they quickly cross the threshold and benefit from the compiled path.

### Why compile?

The interpreted path has two sources of overhead that compilation eliminates:

1. **Virtual dispatch on every filter.** Each `SelectorFilter.IsMatch()` call is a virtual method call. With 3–4 filters per simple selector (common in real CSS), that's 3–4 indirect calls per element tested. The compiled path inlines all the filter logic into a single native function with no virtual dispatch.

2. **Loop overhead and branching.** The interpreted path iterates the `Filters` array with a `foreach` loop. The compiled path replaces this with a straight-line sequence of short-circuit `&&` expressions, which the JIT can optimize further.

For style computation, where `IsMatch()` may be called hundreds of thousands of times (every selector in every stylesheet against every element in the document), the difference is substantial.

Critically, the compiled result is **performance-equivalent to hand-written C#**. A selector like `div.foo[bar=baz]`, once compiled, executes essentially the same instructions as if the programmer had written:

```csharp
element.NodeName.Equals("div", StringComparison.OrdinalIgnoreCase)
    && element.ClassNames.Contains("foo")
    && element.Attributes.TryGetValue("bar", out var v) && v == "baz"
```

The expression tree compilation produces the same property accesses, the same method calls, and the same short-circuit logic that a C# compiler would. The only overhead relative to hand-written code is the delegate invocation itself (one indirect call), which is negligible compared to the work saved by eliminating virtual dispatch on every filter.

### How compilation works

Compilation proceeds in two phases:

#### Phase 1: Build an expression tree

`SimpleSelector.GetMatchExpression()` constructs a `System.Linq.Expressions.Expression` that represents the entire match test as a single boolean expression. It chains together sub-expressions with `Expression.AndAlso` (short-circuit AND):

```csharp
public Expression GetMatchExpression(ParameterExpression element)
{
    Expression? expression = null;

    // Element name test, if not "*" or empty.
    if (!string.IsNullOrEmpty(ElementName) && ElementName != "*")
    {
        expression = Expression.Call(
            Expression.MakeMemberAccess(element, _elementNameProperty),
            _equalsMethod,
            Expression.Constant(ElementName),
            Expression.Constant(StringComparison.OrdinalIgnoreCase));
    }

    // Chain each filter's expression with short-circuit AND.
    foreach (SelectorFilter filter in Filters)
    {
        Expression nextExpression = filter.GetMatchExpression(element);

        expression = expression != null
            ? Expression.AndAlso(expression, nextExpression)
            : nextExpression;
    }

    return expression ?? Expression.Constant(false);
}
```

Each `SelectorFilter` subclass provides its own `GetMatchExpression()` that generates the expression tree fragment for its specific match logic. The `SimpleSelector` combines them all into one tree.

#### Phase 2: Compile to a native delegate

`CompileMatchFunc()` wraps the expression tree in a lambda and compiles it:

```csharp
public Func<Element, bool> CompileMatchFunc()
{
    ParameterExpression element = Expression.Parameter(typeof(Element), "element");

    Expression<Func<Element, bool>> matcher = Expression.Lambda<Func<Element, bool>>(
        GetMatchExpression(element),
        element);

    return matcher.Compile();
}
```

The result is a `Func<Element, bool>` backed by JIT-compiled native code. This delegate is cached in `_compiledMatchFunc` and reused for all subsequent `IsMatch()` calls.

### What the compiled code looks like

To make the compilation concrete, here is what each filter type generates as an expression tree, and what the equivalent C# would look like after compilation.

#### `SelectorFilterId` — `#header`

```
Expression tree:    element.Id == "header"
Equivalent C#:      (Element element) => element.Id == "header"
```

This compiles to a direct property access and string equality test. The `Element.Id` property is backed by a plain field (`_id`), so this resolves to a field load and a pointer comparison (for interned strings) or a fast string compare.

#### `SelectorFilterClass` — `.active`

```
Expression tree:    element.ClassNames.Contains("active")
Equivalent C#:      (Element element) => element.ClassNames.Contains("active")
```

`Element.ClassNames` returns a `HashSet<string>`, so `Contains` is an O(1) hash lookup.

#### `SelectorFilterHasAttrib` — `[disabled]`

```
Expression tree:    element.Attributes.ContainsKey("disabled")
Equivalent C#:      (Element element) => element.Attributes.ContainsKey("disabled")
```

#### `SelectorFilterAttrib` — `[type=text]`, `[class~=foo]`, `[href$=".pdf" i]`, etc.

Attribute filters generate the most complex expression trees because they must handle the `TryGetValue` pattern (the attribute may not exist) and then dispatch to the correct comparison. The generated expression tree is equivalent to:

```csharp
(Element element) => {
    string value;
    if (!element.Attributes.TryGetValue("type", out value))
        return false;
    return value == "text";  // or .Contains(), .StartsWith(), etc.
}
```

This is built using `Expression.Block` with a local variable, `Expression.Condition` for the if/else, and the appropriate `Expression.Call` for the comparison. The `StringComparison` argument (Ordinal vs. OrdinalIgnoreCase) is baked in as a constant depending on whether the `i`/`s` flag was specified.

#### `SelectorPseudoFirstChild` — `:first-child`

```
Expression tree:    element.PreviousSibling == null
Equivalent C#:      (Element element) => element.PreviousSibling == null
```

#### `SelectorPseudoEmpty` — `:empty`

```
Expression tree:    element.Count == 0
Equivalent C#:      (Element element) => element.Count == 0
```

#### `SelectorPseudoStyleFlag` — `:hover`, `:disabled`, `:checked`, etc.

```
Expression tree:    (element.StyleFlags & mask) == match
Equivalent C#:      (Element element) => (element.StyleFlags & StyleFlags.Hover) == default
```

The expression tree accesses the `StyleFlags` field (which is on `Node`, not `Element`, so a cast is included) and applies a bitwise AND with the mask, comparing against the expected match value.

Note: `:hover` and `:link` test for the *absence* of a flag (match value is `default`/zero), while `:disabled` and `:checked` test for the *presence* of a flag. The mask/match pattern handles both cases uniformly.

#### `SelectorPseudoIsNot` — `:is(...)`, `:not(...)`

```
Expression tree:    CompoundSelector.IsMatch(element)       // for :is()
                    !CompoundSelector.IsMatch(element)      // for :not()
Equivalent C#:      (Element element) => childSelector.IsMatch(element)
```

The child `CompoundSelector` is embedded in the expression tree as a constant (a captured reference). This means the compiled expression for `:not(.active)` calls back into the selector matching engine — but the *inner* `.active` selector will itself be compiled after its own 3-use threshold is reached.

#### `SelectorUnknownPseudoClass` — `:custom`, `::custom(arg)`

```
Expression tree:    element.HasPseudoClass("custom", null)
                    element.HasPseudoElement("custom", "arg")
```

This is the extensibility escape hatch. The compiled code calls the virtual `HasPseudoClass()` or `HasPseudoElement()` method on the element, which applications can override to define custom pseudo-classes.

### Combining it all: a complete example

For the simple selector `div.container#main[data-active]`:

- Element name: `div`
- Filter 1: `.container` (class)
- Filter 2: `#main` (ID)
- Filter 3: `[data-active]` (has-attribute)

The generated expression tree is equivalent to:

```csharp
(Element element) =>
       element.NodeName.Equals("div", StringComparison.OrdinalIgnoreCase)
    && element.ClassNames.Contains("container")
    && element.Id == "main"
    && element.Attributes.ContainsKey("data-active")
```

All four tests are chained with `AndAlso` (short-circuit AND), so if the element isn't a `<div>`, the class/ID/attribute tests are never evaluated. After compilation, this is a single native function with no virtual dispatch, no array iteration, and no heap allocation.

### Reflection metadata caching

Each `SelectorFilter` subclass caches the `PropertyInfo` and `MethodInfo` objects it needs for expression tree construction as `private static readonly` fields. These are looked up once via reflection (at class initialization time) and reused for every expression tree built. For example, `SelectorFilterId` caches:

```csharp
private static readonly PropertyInfo _idProperty =
    typeof(Element).GetProperty(nameof(Element.Id),
        BindingFlags.Instance | BindingFlags.Public)!;
```

And `SelectorFilterAttrib` caches seven `MethodInfo` references (`TryGetValue`, `Equals`, `StartsWith`, `EndsWith`, `Contains`, `StringIncludes`, `StringDashMatches`). This avoids any reflection overhead during expression tree construction.

---

## `CompoundSelector.IsMatch()` — OR logic over selectors

`CompoundSelector` wraps one or more `Selector` objects (separated by commas in the CSS source). Its `IsMatch()` is a simple OR:

```csharp
public bool IsMatch(Node? node)
{
    if (!(node is Element element))
        return false;

    foreach (Selector selector in Selectors)
    {
        if (selector.IsMatch(element))
            return true;
    }
    return false;
}
```

Note that `CompoundSelector.IsMatch()` accepts `Node?` (not `Element?`) and does the type check itself. This is convenient for callers that have a `Node` reference and don't want to cast.

---

## `Closest()` — finding the nearest matching ancestor

Both `Selector` and `CompoundSelector` provide a `Closest()` method that walks up the ancestor chain (including the starting node) and returns the first element that matches:

```csharp
// Selector.Closest():
public Element? Closest(Node node)
{
    for (Node? current = node; current != null; current = current.Parent)
    {
        if (current is Element element && IsMatch(element))
            return element;
    }
    return null;
}
```

`CompoundSelector.Closest()` tries each of its selectors in order and returns the first match from any of them.

---

## Performance characteristics

### Complexity

| Operation | Typical | Worst case |
|-----------|---------|------------|
| `SimpleSelector.IsMatch()` (compiled) | O(filters) — one native call | Same |
| `SimpleSelector.IsMatch()` (interpreted) | O(filters) — virtual calls | Same |
| `Selector.IsMatch()` with `Child` / `AdjacentSibling` / `Self` only | O(path length) | O(path length) |
| `Selector.IsMatch()` with `Descendant` combinator | O(path length × tree depth) | O(path length ^ tree depth) — exponential, but never observed in practice |
| `Selector.IsMatch()` with `GeneralSibling` combinator | O(path length × sibling count) | O(path length × sibling count ^ path length) — similarly theoretical |
| `CompoundSelector.IsMatch()` | O(selectors × above) | Same |
| `Closest()` | O(ancestors × IsMatch) | Same |

The exponential worst cases for `Descendant` and `GeneralSibling` arise from backtracking: a selector like `a b c d e f g h` tested against a very deep tree with many matching ancestors could explore a combinatorial number of ancestor combinations. In practice, CSS selectors are short (2–4 components), tree depth is moderate, and matching ancestors are sparse, so the recursion stays close to linear.

### Compilation cost and amortization

Expression tree compilation has a one-time cost. The compiled delegate is cached in `_compiledMatchFunc` on the `SimpleSelector` instance and reused for all subsequent calls. Since `SimpleSelector` objects are shared across all uses of the same parsed selector, the compilation cost is amortized across all elements tested.

The 3-use threshold means:

| Usage pattern | Behavior |
|---------------|----------|
| One-off `IsMatch()` call (e.g., from application code) | Interpreted — no compilation overhead |
| Selector used in a CSS stylesheet | Compiled after testing the first 2 elements; compiled for the remaining thousands |
| Selectors inside `:is()` / `:not()` | Inner selectors have their own usage counters and compile independently |

### Why right-to-left?

Matching right-to-left is an optimization borrowed from browser engines. The rightmost component of a selector is the one that must match the *candidate element*, and it is typically the most specific (e.g., `.active`, `#header`). By testing it first, non-matching elements are rejected immediately — without ever examining the DOM tree structure. The ancestor/sibling walk (which is more expensive) only happens for the subset of elements that pass the rightmost test.

---

## Design philosophy: selectors as a first-class tool

The adaptive compilation system is not just a performance optimization — it reflects a deliberate design choice about how Onyx is meant to be used.

In a traditional browser DOM, selectors were grafted on long after the core API was established. The "classic" way to find elements was imperative tree traversal: walk child nodes, test tag names, check attributes, maintain your own lists. Methods like `querySelectorAll()` arrived years later and were always slightly suspect from a performance standpoint — convenient, but potentially slower than hand-written traversal.

Onyx inverts this. Because compiled selectors produce IL equivalent to hand-written property accesses and method calls, **there is no performance penalty for using selectors instead of manual element tests**. A selector like `div.container > span.active` matches just as efficiently as the equivalent hand-written code:

```csharp
// This selector-based query...
var elements = document.Find("div.container > span.active");

// ...runs with the same per-element cost as this hand-written traversal:
foreach (var span in document.Descendants().OfType<Element>())
{
    if (span.NodeName == "span"
        && span.ClassNames.Contains("active")
        && span.Parent is Element parent
        && parent.NodeName == "div"
        && parent.ClassNames.Contains("container"))
    {
        // matched
    }
}
```

This is intentional. It encourages programmers to express their intent using selectors — a declarative, composable, well-understood notation — rather than writing imperative traversal code. Selectors are the preferred first-class tool for querying and traversing a document in Onyx. This matches modern usage patterns (where CSS selectors and jQuery-style queries have largely replaced manual DOM walking) much better than the "classic" DOM node-traversal APIs do.

---

# The Find Engine

## Overview

The Find engine answers a different question from IsMatch. Where IsMatch asks "Does this specific element satisfy the selector?", Find asks "Which elements in this subtree satisfy the selector?" The naive approach — iterate every descendant and call `IsMatch()` on each — works, but scales linearly with the size of the document. For a 10,000-element document with 200 CSS rules, that means 2,000,000 IsMatch calls during style computation alone.

The Find engine improves on this with a **query planner** — an optimizer that analyzes the selector, inspects the document's index structures, and chooses a starting strategy that produces the smallest possible candidate set. Only the candidates in this narrowed set are then tested with `IsMatch()`. This is conceptually similar to how a database query optimizer chooses between a table scan and an index lookup, and the approach is (to our knowledge) unique to Onyx among CSS selector engines.

## `Selector.Find()` — the two-phase architecture

`Find()` works in two phases:

1. **Phase 1: Candidate generation.** Use the query planner (if available) to produce a small set of candidate elements that *might* match the selector.
2. **Phase 2: Candidate verification.** Test each candidate with `IsMatch()` to confirm it actually matches the full selector, and add confirmed matches to the result set.

```csharp
public int Find(Node root, ISet<Element> result)
{
    Node? trueRoot = root?.Root;
    if (trueRoot == null)
        throw new ArgumentNullException("Node subtree must not be null.");

    if (_path.Length == 0)
        return 0;

    // --- Phase 1: Candidate generation ---
    IReadOnlyCollection<Node> baseSet;
    if (trueRoot is IElementLookupContainer fastLookupContainer)
    {
        // Optimized path: use the query planner.
        baseSet = QueryPlanner.ExecuteQuery(this, trueRoot,
            fastLookupContainer.ElementLookupTables);
    }
    else
    {
        // Fallback path: scan all descendants.
        baseSet = trueRoot.Descendants().ToList();
    }

    // --- Phase 2: Candidate verification ---
    int numAdded = 0;
    foreach (Node candidate in baseSet)
    {
        if (!(candidate is Element element))
            continue;

        if (!IsMatch(element))
            continue;

        // If searching a subtree, verify the candidate is under the given root.
        if (root != trueRoot && !root!.ContainsOrIs(element))
            continue;

        if (result.Add(element))
            numAdded++;
    }

    return numAdded;
}
```

The `IElementLookupContainer` check is the critical branch. `Document` implements this interface (it maintains index structures); `DocumentFragment` does not. So queries against a full `Document` get the optimized path, while queries against a fragment fall back to brute-force scanning.

### Subtree scoping

Note that `Find()` always retrieves candidates from the **true root** of the tree (the `Document`), not from the subtree root passed by the caller. This is because the query planner's index structures are document-wide. If the caller passed a subtree root (e.g., searching within a specific `<div>`), the second phase includes a `ContainsOrIs()` check to filter out candidates that fall outside the requested subtree.

### Result type

`Find()` returns an `IReadOnlySet<Element>` (backed by `HashSet<Element>`). The set semantics are intentional: results are unordered, duplicate-free, and support efficient set operations (union, intersection, etc.) for combining results from multiple queries. `CompoundSelector.Find()` delegates to each of its child `Selector.Find()` calls, passing the same `HashSet`, so the union of all selectors' results is computed naturally by the set's deduplication.

---

## The Query Planner

The query planner is the core innovation of the Find engine. Its job is to choose the single best "starting point" for a query — the index lookup that produces the smallest candidate set — by analyzing the selector and consulting the document's live index statistics.

### The key insight

Consider the selector `#sidebar .menu-item`. A naive engine would scan all descendants of the document and test each one with `IsMatch()`. But the query planner observes:

- `#sidebar` matches (probably) 1 element via the ID index.
- `.menu-item` matches (say) 50 elements via the classname index.
- The full document has 10,000 elements.

The planner has three choices for generating candidates:

1. **Scan all** — 10,000 candidates, 10,000 IsMatch calls.
2. **Start from `.menu-item`** — 50 candidates, 50 IsMatch calls.
3. **Start from `#sidebar`, take descendants** — 1 element + its ~30 descendants = ~31 candidates, 31 IsMatch calls.

Option 3 is cheapest. The planner picks it, even though `#sidebar` is *not* the rightmost component of the selector. This is the fundamental difference from how browsers typically process `querySelectorAll()`: the planner examines *every* simple selector in the path, not just the rightmost one, and chooses the one that produces the fewest candidates.

### Architecture

The query planner consists of several cooperating types, all *(internal)*:

| Type | Role |
|------|------|
| `QueryPlanner` | Static class containing all planning and execution logic |
| `SelectorQueryPlan` | The chosen plan for a complete `Selector`, wrapping a `SimpleSelectorQueryPlan` with runtime metrics |
| `SimpleSelectorQueryPlan` | A single execution strategy: a source (which index to use), a traversal (self/children/descendants), an estimated cost, and an execute delegate |
| `SimpleSelectorQueryPlanSet` | Three plans for the same `SimpleSelector`, one for each traversal context (self, children, descendants) |
| `SimpleSelectorQueryPlanKind` | A flags enum encoding both the source strategy and the traversal strategy |
| `ElementLookupTables` | The live indexes maintained by `Document`, providing O(1) lookups by ID, classname, element type, `name` attribute, and `type` attribute |

### `ElementLookupTables` — the index structures *(internal)*

The query planner depends on five hash-based indexes that the `Document` maintains in real time as elements are added and removed:

| Index | Key | Contents | Used for |
|-------|-----|----------|----------|
| `_elementsById` | ID string | All elements with that `id` attribute | `#header` |
| `_elementsByClassName` | Class string | All elements with that class | `.active` |
| `_elementsByElementType` | Tag name | All elements with that tag | `div`, `span` |
| `_elementsByName` | `name` attribute value | All elements with `[name=value]` | `[name=username]` |
| `_elementsByTypeAttribute` | `type` attribute value | All elements with `[type=value]` | `[type=text]` |

Each index is a `Dictionary<string, HashSet<Element>>`. The `AddElement()` and `RemoveElement()` methods keep all five indexes in sync with the DOM. Empty sets are recycled via a small pool (`_unusedSets`, capped at 64) to reduce allocation pressure during DOM mutations.

The `ElementLookupTables` also holds two plan caches:

- `SimpleSelectorQueryPlans` — maps `SimpleSelector` → `SimpleSelectorQueryPlanSet` (reusable across selectors)
- `SelectorQueryPlans` — maps `Selector` → `SelectorQueryPlan` (the final chosen plan)

### How plan creation works

Plan creation happens in `MakeQueryPlan()` *(private)* and flows through three steps.

#### Step 1: Generate plans for each simple selector

For each `SimpleSelector` in the path, the planner calls `MakeSimpleQueryPlan()`, which calls `GenerateFastQueryPlan()` *(private)*. This method inspects the simple selector's element name and filters, looks up each one in the index tables, and measures how many elements are in each index set. For every index that is relevant to the selector, it creates plans for all three traversal modes (self, children, descendants) and keeps the cheapest one for each mode.

The inspection order and what each looks for:

1. **Element type** — if the selector has a tag name (not `*`), look up the tag name index. A selector for `input` might find 12 elements in the index.
2. **Class names** — for each `.class` filter, look up the classname index. `.active` might find 50 elements; `.menu-item` might find 8.
3. **ID** — for each `#id` filter, look up the ID index. `#sidebar` almost certainly finds 0 or 1 element.
4. **`[name=...]`** — if there is an exact `[name=value]` attribute filter, look up the name index.
5. **`[type=...]`** — if there is an exact `[type=value]` attribute filter, look up the type index.

For each index hit, `UpdatePlanSet()` *(private)* compares the new index set's size against the current best plan for each traversal mode and keeps the smaller one:

```csharp
private static SimpleSelectorQueryPlanSet? UpdatePlanSet(
    SimpleSelectorQueryPlanSet? planSet,
    IReadOnlyCollection<Node> currentSet, string arg, SimpleSelector simpleSelector,
    Func<SimpleSelector, string, int, SimpleSelectorQueryPlan> makeSelfPlan,
    Func<SimpleSelector, string, int, SimpleSelectorQueryPlan> makeChildrenPlan,
    Func<SimpleSelector, string, int, SimpleSelectorQueryPlan> makeDescendantsPlan)
{
    planSet ??= SimpleSelectorQueryPlanSet.Empty;

    int estimatedSelfCost = currentSet.Count;
    if (planSet.Self == null || estimatedSelfCost < planSet.Self.EstimatedCost)
        planSet = planSet.WithSelf(makeSelfPlan(simpleSelector, arg, estimatedSelfCost));

    int estimatedChildrenCost = ChildrenCost(currentSet);
    if (planSet.Children == null || estimatedChildrenCost < planSet.Children.EstimatedCost)
        planSet = planSet.WithChildren(makeChildrenPlan(simpleSelector, arg,
            estimatedChildrenCost + estimatedSelfCost));

    int estimatedDescendantCost = DescendantCost(currentSet);
    if (planSet.Descendants == null || estimatedDescendantCost < planSet.Descendants.EstimatedCost)
        planSet = planSet.WithDescendants(makeDescendantsPlan(simpleSelector, arg,
            estimatedDescendantCost + estimatedSelfCost));

    return planSet;
}
```

**Cost estimation** uses the actual live counts from the index:

- **Self cost** = `currentSet.Count` (how many elements are in the index for this key).
- **Children cost** = `Sum(element.ChildElementCount for each element in the set)` + the self cost. This is the number of child elements the plan would need to iterate.
- **Descendants cost** = `Sum(element.DescendantElementCount for each element in the set)` + the self cost. This uses the DOM's maintained `DescendantElementCount` counters to estimate without traversing.

If no index applies to the simple selector (e.g., it only has an attribute filter for a non-indexed attribute like `[data-custom]`), the plan falls back to `ScanAll`, which has a cost equal to the total number of elements in the document.

#### Step 2: Choose the best simple selector across the full path

`MakeQueryPlan()` walks the selector's path **right-to-left** and, for each simple selector, selects the plan variant that matches the combinator to its right:

| Combinator to the right | Plan variant used |
|--------------------------|-------------------|
| `Self` (rightmost position) | `planSet.Self` |
| `Child` (`>`) | `planSet.Children` |
| `Descendant` (space) | `planSet.Descendants` |
| `AdjacentSibling` (`+`) | *Not supported — skipped* |
| `GeneralSibling` (`~`) | *Not supported — skipped* |

It then compares the estimated cost of this plan against the best plan found so far, keeping the cheapest:

```csharp
SimpleSelectorQueryPlan? bestQueryPlan = null;
Combinator lastCombinator = Combinator.Self;

for (int i = path.Count - 1; i >= 0; i--)
{
    SimpleSelectorQueryPlan? simpleQueryPlan = null;
    if (lastCombinator == Combinator.Self)
        simpleQueryPlan = planSet.Self;
    else if (lastCombinator == Combinator.Child)
        simpleQueryPlan = planSet.Children;
    else if (lastCombinator == Combinator.Descendant)
        simpleQueryPlan = planSet.Descendants;

    if (simpleQueryPlan != null)
        if (bestQueryPlan == null
            || simpleQueryPlan.EstimatedCost < bestQueryPlan.EstimatedCost)
            bestQueryPlan = simpleQueryPlan;

    lastCombinator = path[i].Combinator;
}
```

The winning `SimpleSelectorQueryPlan` is wrapped in a `SelectorQueryPlan` and cached.

**Why skip `+` and `~`?** The adjacent-sibling and general-sibling combinators don't lend themselves to efficient index-based lookups. If a simple selector is reached only via `+` or `~`, the planner does not know how to generate a narrowed candidate set from it, so it simply skips that selector during plan selection. This means selectors like `h2 + p` will not benefit from the planner's index lookup for the `h2` component, and the planner will fall back to either the `p` component's index or a full scan.

#### Step 3: Fall back to ScanAll if nothing better was found

If no simple selector in the entire path yielded an indexed plan (all were skipped or had no applicable index), the planner produces a `ScanAll` plan that retrieves all descendants:

```csharp
return new SelectorQueryPlan(
    bestQueryPlan ?? MakeScanAllPlan(trueRoot.DescendantElementCount));
```

### The 15 plan strategies

Each plan is a combination of a **source** (which index to use) and a **traversal** (what to return from that index). The `SimpleSelectorQueryPlanKind` flags enum encodes both:

| Source (low byte) | Description |
|-------------------|-------------|
| `ScanAll` (0) | No index — scan entire document |
| `ElementType` | Use the tag name index |
| `Id` | Use the ID index |
| `Classname` | Use the classname index |
| `Name` | Use the `[name]` attribute index |
| `TypeAttribute` | Use the `[type]` attribute index |

| Traversal (high byte) | Description |
|------------------------|-------------|
| `Self` | Return the index set directly as candidates |
| `Children` | Return the children of elements in the index set |
| `Descendants` | Return the descendants of elements in the index set |

This yields 5 sources × 3 traversals = 15 concrete plan strategies (plus `ScanAll` as the 16th fallback). Each is implemented as a factory method in `QueryPlanner` that creates a `SimpleSelectorQueryPlan` with a delegate that performs the lookup at execution time.

For example, the `Id + Descendants` plan for `#sidebar` in `#sidebar .menu-item`:

```csharp
private static SimpleSelectorQueryPlan MakeGetElementsByIdDescendantsPlan(
    SimpleSelector simpleSelector, string id, int estimatedCost)
    => new SimpleSelectorQueryPlan(
        SimpleSelectorQueryPlanKind.Id | SimpleSelectorQueryPlanKind.Descendants,
        id, estimatedCost,
        (trueRoot, tables) => {
            IReadOnlyCollection<Node> baseSet = tables.GetElementsById(id);
            return (DescendantsOf(simpleSelector, baseSet), DescendantCost(baseSet));
        });
```

The delegate captures the ID string and the simple selector. At execution time, it looks up the ID in the index, verifies each element matches the full simple selector (not just the ID — the selector might also have classes, attributes, etc.), collects their descendants, and returns both the result set and a cost re-estimate.

### Traversal helpers: `ChildrenOf` and `DescendantsOf` *(private)*

When a plan uses the `Children` or `Descendants` traversal, the query planner needs to expand the index set. Two helper methods do this:

**`ChildrenOf`** — for `>` (child combinator) plans. Takes the index set, filters it through `IsMatch()` to verify each element fully matches the simple selector, then collects their child elements:

```csharp
private static IReadOnlyCollection<Node> ChildrenOf(
    SimpleSelector simpleSelector, IReadOnlyCollection<Node> baseSet)
{
    List<Node> result = new List<Node>();
    foreach (Node node in baseSet)
    {
        if (node is Element element && simpleSelector.IsMatch(element))
        {
            foreach (Node child in node.ChildNodes)
            {
                if (child is Element)
                    result.Add(node);
            }
        }
    }
    return result;
}
```

**`DescendantsOf`** — for space (descendant combinator) plans. Similar, but collects all descendants:

```csharp
private static IReadOnlyCollection<Node> DescendantsOf(
    SimpleSelector simpleSelector, IReadOnlyCollection<Node> baseSet)
{
    List<Element> result = new List<Element>();
    foreach (Node node in baseSet)
    {
        if (node is Element element && simpleSelector.IsMatch(element))
            node.GetDescendants(result);
    }
    return result;
}
```

Note that both methods call `simpleSelector.IsMatch()` on the elements from the index set. This is important: an element might be in the classname index for `.menu` but might not match the full simple selector `.menu[data-active]`. The traversal helpers filter before expanding to avoid wasting time collecting descendants of non-matching elements.

### Adaptive plan invalidation

Query plans are cached, but the DOM is mutable — elements are added, removed, and modified. A plan that was optimal yesterday may be suboptimal today. The query planner handles this with **adaptive invalidation** in `ExecuteQuery()`.

After executing a plan, the planner makes two checks:

#### Check 1: Has the estimated cost drifted?

Each plan execution returns a **cost re-estimate** (the current size of the index set used). The planner compares this against the estimated cost that was used when the plan was created:

```csharp
if (costReestimate * DiffNumer < lastEstimatedCost * DiffDenom
    || costReestimate * DiffDenom > lastEstimatedCost * DiffNumer)
{
    innerQueryPlan.EstimatedCost = costReestimate;
    ResetQueryPlan(selector, trueRoot, tables);
}
```

If the re-estimate has shrunk to less than 2/3 of the original estimate, or grown to more than 3/2, the plan is considered stale and is evicted from the cache. The next query will trigger a fresh `MakeQueryPlan()`.

This handles the case where the index set for the chosen simple selector has grown or shrunk significantly — for example, if many `.menu-item` elements were added to the document.

#### Check 2: Has the result count drifted?

Even if the chosen plan's own cost is stable, the *overall* plan may be suboptimal because a different simple selector now has a smaller index set. The planner tracks the actual result count and compares it against the last known count:

```csharp
if (result.Count * DiffNumer < lastActualCost * DiffDenom
    || result.Count * DiffDenom > lastActualCost * DiffNumer)
{
    ResetQueryPlan(selector, trueRoot, tables);
}
```

If the result count has changed by more than 50%, the outer plan is reset. The inner `SimpleSelectorQueryPlan` metrics are preserved so they don't need to be re-measured, but the plan selection logic runs again and may choose a different simple selector as the starting point.

The 2/3 and 3/2 thresholds (expressed as integer ratios `DiffDenom=2, DiffNumer=3` to avoid floating-point arithmetic) are deliberately generous. The goal is not to maintain a perfectly optimal plan at all times — it is to detect when a plan has become *significantly* suboptimal and trigger re-planning. Small fluctuations (a few elements added or removed) are not worth re-planning for.

### Two-level plan cache

Plans are cached at two levels within `ElementLookupTables`:

| Cache | Key | Value | Scope |
|-------|-----|-------|-------|
| `SimpleSelectorQueryPlans` | `SimpleSelector` | `SimpleSelectorQueryPlanSet` (self/children/descendants plans) | Shared across all `Selector` objects that contain the same `SimpleSelector` |
| `SelectorQueryPlans` | `Selector` | `SelectorQueryPlan` (the chosen winning plan + runtime metrics) | Specific to one `Selector` |

The two-level design means that when a new selector is encountered that contains a previously-seen simple selector (e.g., a new rule with `.active` when `.active` plans already exist), the planner can reuse the existing simple-selector plans without re-querying the indexes. Only the plan-selection step (choosing which simple selector is cheapest) needs to run.

Plan cache entries live as long as the `Document` does. They are evicted only by the adaptive invalidation logic described above, or if the document itself is discarded.

### Worked example

Consider a document with 5,000 elements, containing:
- 200 `<div>` elements
- 50 elements with class `widget`
- 1 element with id `main-content`
- 15 elements with class `active`

The selector is `#main-content div.widget .active`.

**Path:**
```
Path[0]:  Self         #main-content
Path[1]:  Descendant   div.widget
Path[2]:  Descendant   .active
```

**Plan creation** walks right-to-left:

1. **`Path[2]`: `.active`** — combinator to right is `Self` (rightmost position), so use `planSet.Self`.
   - Classname index for `active` has 15 elements → self cost = 15.
   - Best so far: `.active Self`, estimated cost = 15.

2. **`Path[1]`: `div.widget`** — combinator to right is `Descendant`, so use `planSet.Descendants`.
   - Element type index for `div` has 200 elements → descendants cost = 200 + sum of descendant counts.
   - Classname index for `widget` has 50 elements → descendants cost = 50 + sum of descendant counts.
   - The planner picks classname (50 < 200). Suppose total descendant cost is 300.
   - Best so far: still `.active Self` at 15 (15 < 300).

3. **`Path[0]`: `#main-content`** — combinator to right is `Descendant`, so use `planSet.Descendants`.
   - ID index for `main-content` has 1 element → descendants cost = 1 + its descendant count.
   - Suppose `#main-content` has 800 descendants → cost = 801.
   - Best so far: still `.active Self` at 15 (15 < 801).

**Winning plan:** Start from the classname index for `active`, take self (the 15 elements directly). Even though `#main-content` is the most visually specific part of the selector, the planner determined that `.active` produces fewer candidates.

**Execution:** The 15 `.active` elements are retrieved from the index, and each is tested with the full `IsMatch()` — which will walk up the ancestor chain to verify the `div.widget` and `#main-content` parts. Of the 15 candidates, perhaps only 3 are actually inside `#main-content` under a `div.widget`. Those 3 are added to the result set.

**Total work:** 15 IsMatch calls instead of 5,000. A 333× reduction.

Now suppose the document changes and 500 elements with class `active` are added. On the next `Find()` call, the plan executes and the cost re-estimate returns 515 (up from 15). Since 515 × 3 > 15 × 2 (the 3/2 threshold), the plan is invalidated. The next call triggers `MakeQueryPlan()` again. This time, the planner might find that `#main-content` descendants (801) is cheaper than `.active` self (515), and switch strategies.

### Differences from browser selector engines

| Aspect | Typical browser | Onyx query planner |
|--------|----------------|-------------------|
| Candidate generation | Always starts from the rightmost selector; scans all matching elements | Examines every selector in the path; starts from whichever produces the fewest candidates |
| Index usage | May use an index for the rightmost component only | Uses indexes for any component in any position |
| Plan caching | Generally none (stateless per query) | Plans are cached and reused across queries |
| Adaptive re-planning | None | Plans are invalidated when costs drift by >50% |
| Traversal direction | May search the starting set's descendants only | May search descendants, children, or use the set directly, depending on combinator context |
| Sibling combinators | Supported in candidate generation | Not supported by the planner (fall back to scan) |

The result is that Onyx's `Find()` may produce execution patterns that look very different from what a browser would do for `querySelectorAll()`. A browser always starts from the rightmost component; Onyx might start from a component in the middle or at the left of the selector, if that component has the smallest index set. The correctness guarantee is the same — `IsMatch()` always verifies the full selector — but the candidate set may be generated from a completely different starting point.

---

## The Style Rule Engine

### Purpose

The style rule engine answers the inverse question from `Find()`: instead of "given a selector, which elements match?", it asks "given an element, which style rules match?" This is the question that must be answered every time an element's computed style is calculated — the engine must efficiently locate, from potentially thousands of stylesheet rules, just the handful that apply to a given element.

The engine is implemented in `StyleManager` (`Onyx/Css/StyleManager.cs`) and invoked by `Element.GetComputedStyle()` → `StyleManager.ComputeStyle()` → `StyleManager.GetStyleRules()`.

### Architecture: two-phase candidate filtering

Like the Find engine, the style rule engine uses a two-phase approach:

1. **Candidate generation** (`FindCandidateRules`) — uses indexes to quickly assemble a *superset* of rules that could possibly match the element. This phase is designed to be fast and is allowed to include false positives.
2. **Verification** (`GetStyleRules`) — iterates the candidates and calls `Selector.IsMatch()` on each one to determine which actually match. This phase is exact.

The key insight is the same as the Find engine's query planner: avoid calling `IsMatch()` on every rule in every stylesheet. In a document with thousands of rules, most rules won't match any given element, so the goal is to narrow the candidate set as aggressively as possible before invoking the full matching machinery.

### The style rule indexes

When a stylesheet is added to a `StyleManager` via `AddStylesheet()`, every rule in the stylesheet is indexed. The indexing examines the **last simple selector** in each selector's path — i.e., the rightmost component, which describes the element that the rule directly targets.

There are four indexes:

| Index | Key | Populated when |
|-------|-----|----------------|
| `_elementNameIndex` | Tag name (e.g., `"div"`) | Last simple selector has an element name other than `*` |
| `_classNameIndex` | Class name (e.g., `"widget"`) | Last simple selector has a `SelectorFilterClass` |
| `_idIndex` | ID (e.g., `"main"`) | Last simple selector has a `SelectorFilterId` |
| `_genericIndex` | *(no key — a flat list)* | Last simple selector has **none** of the above (e.g., `*`, `[attr]`, `:hover`) |

A single rule may be indexed in multiple places. For example, `div.widget#main { ... }` would be added to all three keyed indexes (by element name `"div"`, by class `"widget"`, and by ID `"main"`). A rule like `*[data-active] { ... }` has no element name, no class, and no ID in its last simple selector, so it goes into the `_genericIndex`.

#### Why index the last simple selector?

The last simple selector (the rightmost in the path) describes the *subject* of the selector — the element that will actually receive the styles. By indexing on properties of the subject, `FindCandidateRules()` can look up only those rules whose subject *could* match a given element, based on the element's own ID, classnames, and tag name.

This is analogous to how the Find engine's query planner works, but the direction is reversed: the Find engine indexes *elements* and looks up which ones might match a *selector*, while the style rule engine indexes *selectors* (rules) and looks up which ones might match an *element*.

#### Indexing walkthrough

When `AddStylesheet()` is called, for each rule, for each selector in the rule's `CompoundSelector`:

1. **All simple selectors** are scanned for classname and attribute references, which are tracked in `ClassnamesUsedByStyles` and `AttributesUsedByStyles`. These reference-counted dictionaries are used by `Element.OnAttrChange()` to decide whether an attribute change might affect styling (if a changed attribute isn't referenced by any selector, no style invalidation is needed).

2. **The last simple selector** is examined for indexable features:
   - If it has an element name (not `*`), the rule is added to `_elementNameIndex[elementName]`.
   - If it has class filters, the rule is added to `_classNameIndex[className]` for each class.
   - If it has ID filters, the rule is added to `_idIndex[id]` for each ID.
   - If *none* of the above produced an index entry, the rule is added to `_genericIndex`.

The class and ID index insertions use `AddToDictionaryOfListWithUniq`, which prevents duplicate entries (since a single rule with a compound selector like `div.a, div.b` might try to add the same rule under the same key twice).

### FindCandidateRules: assembling the candidate set

```
FindCandidateRules(element) → HashSet<StyleRule>
```

Given an element, this method assembles a superset of all rules that could possibly match:

1. Start with **all generic rules** (the `_genericIndex`). These rules have no element name, class, or ID in their last simple selector, so they could match anything.
2. If the element has an **ID**, look up `_idIndex[element.Id]` and union the results in.
3. For each of the element's **classnames**, look up `_classNameIndex[className]` and union the results in.

The result is a `HashSet<StyleRule>` — a deduplicated set of candidate rules that includes every rule that *might* match the element. Rules that reference a class or ID that the element doesn't have are excluded, which can eliminate the vast majority of rules in a large stylesheet.

### GetStyleRules: verification and specificity tracking

```
GetStyleRules(element) → IReadOnlyCollection<StylePropertySetWithSpecificity>
```

This is the main entry point. It calls `FindCandidateRules()` to get candidates, then verifies each one:

```
for each candidate rule:
    for each selector in rule.Selector.Selectors:     // (the CompoundSelector's comma-separated list)
        if selector.IsMatch(element):
            track the highest specificity among matching selectors
    if any selector matched:
        emit (rule.Properties, highestSpecificity)
```

The result is a list of `StylePropertySetWithSpecificity` — each one is a bag of CSS property declarations paired with the specificity of the most-specific selector that caused it to match. Note that this is the *raw* result: properties may overlap or conflict, and shorthand properties are not yet expanded. The caller (`ComputeStyle()`) handles resolution.

#### Specificity tracking

When multiple selectors in the same `CompoundSelector` match the same element (e.g., `div.foo, .foo.bar { ... }` where the element is `<div class="foo bar">`), `GetStyleRules()` records the *highest* specificity among the matching selectors. This is important because CSS specificity determines which declarations win when properties conflict, and the same rule can match through selectors of different specificities.

### ComputeStyle: the full pipeline

`StyleManager.ComputeStyle()` orchestrates the complete style computation for an element:

1. **Collect matching rules**: Call `GetStyleRules(element)` to get all matching rule property sets with their specificities.
2. **Add inline styles**: If the element has a `style` attribute, its parsed inline styles are added with `Specificity.MaxValue` (ensuring they override all stylesheet rules, per CSS specification).
3. **Resolve conflicts** (`ExtractMostSpecificStyles`): All property declarations are decomposed (shorthand properties like `margin` are expanded into `margin-top`, `margin-right`, etc.), then for each property kind, only the declaration with the highest specificity is kept. Ties at the same specificity are broken by the `!important` flag.
4. **Apply to computed style**: Starting from the parent element's computed style (for inheritance) or the default style, each winning property declaration is applied to produce the final `ComputedStyle`.

### Style invalidation

The style rule engine doesn't operate in isolation — it's connected to a change-tracking system that knows when recomputation is needed.

#### Attribute-driven invalidation

`Element.OnAttrChange()` is the central hook. When an attribute changes, it consults the `StyleManager`'s tracking dictionaries to decide whether the change could affect styling:

- **ID changes**: Always invalidate (IDs are always potentially significant).
- **Class changes**: Compute the *symmetric difference* between the old and new classname sets. Only invalidate if at least one of the changed classnames appears in `ClassnamesUsedByStyles`.
- **Style attribute changes**: Always invalidate (inline styles directly affect computed style).
- **Other attribute changes**: Only invalidate if the attribute name appears in `AttributesUsedByStyles`.

This selective invalidation is an optimization: if a document has thousands of elements but the stylesheets don't reference a `data-custom` attribute, changing `data-custom` on an element won't trigger any style recomputation.

#### The style queue

When `InvalidateComputedStyle()` is called on an element, two things happen:

1. The element's cached `_computedStyle` is set to `null`.
2. The element is added to the document's `StyleQueue`.

The `StyleQueue` is a set of elements (subtree roots) whose styles need recomputation. It can be processed in two ways:

- **Eagerly**: Calling `element.GetComputedStyle()` immediately recomputes that element's style (and recursively its ancestors' styles if they are also invalid).
- **Batched**: Calling `document.ValidateComputedStyles()` drains the entire queue, recomputing styles for all queued elements and their descendants.

The batched approach is more efficient when many changes are made in sequence (e.g., adding multiple classes or modifying the DOM structure), because it defers recomputation until all changes are complete.

#### Stylesheet-level invalidation

When a stylesheet is added or removed, the `StyleManager` raises the `StylesheetsChanged` event, which causes the `Document` to call `InvalidateChildComputedStyles()` — invalidating the computed styles of the *entire* tree. This is a blunt instrument, but it's correct: any rule in the added or removed stylesheet could affect any element.

This whole-tree invalidation is deliberate, not a shortcut, and is motivated by several design principles:

- **Stylesheets are immutable objects.** There is no API to modify a single rule within a stylesheet — the only operations are adding or removing entire `Stylesheet` objects. This encourages programmers to batch their styling decisions into complete stylesheets rather than making incremental rule-by-rule changes, and it means there is no meaningful "partial" invalidation to perform.

- **Stylesheets are expected to be static.** Programmers who have worked on the web are already used to the pattern that while the DOM frequently mutates, stylesheets are generally created once and then never changed. Stylesheets represent *possible* appearances of an element, so there's minimal cost to including all rules that could apply, even if some are never used. Because programmers already think of stylesheets as "set up once at startup," Onyx's architecture takes advantage of this assumption.

- **The cost model is intentionally asymmetric.** Adding a stylesheet is an expensive operation — it rebuilds indexes and invalidates the entire tree — but one that typically only takes place at program startup. Modifying an element at runtime to switch which style rules it uses, the far more common operation, is as cheap as possible: changing a class or attribute triggers only the selective invalidation described above, which may invalidate nothing at all if the changed attribute isn't referenced by any selector. This asymmetry is the right tradeoff for the expected usage patterns.

### Comparison with the Find engine

| Aspect | Find engine | Style rule engine |
|--------|-------------|-------------------|
| Question answered | "Which elements match this selector?" | "Which rules match this element?" |
| Indexes are over | Elements (by ID, class, tag, etc.) | Rules (by the last simple selector's ID, class, tag) |
| Indexes live in | `ElementLookupTables` (on the Document) | `StyleManager` (on the Document) |
| Candidate set is | Elements from the cheapest index | Rules from the element's ID + classname indexes |
| Verification | `IsMatch()` on each candidate element | `IsMatch()` on each candidate rule's selectors |
| Plan caching | Cached and adaptively invalidated | No plan caching (indexes are static until stylesheets change) |
| Result | Set of matching elements | List of property sets with specificities |

The two engines are mirror images of each other, both built on the same `IsMatch()` core, but with their indexes oriented in opposite directions.

---

## The Ergonomic API

Despite all of the complex algorithms documented above — adaptive JIT compilation, database-style query planning, LINQ expression trees, cost-based plan invalidation — the normal usage of selectors in Onyx is intentionally simple. A programmer does not need to understand any of the internals to take full advantage of them.

The public API is spread across three classes:

- **`Node`** — instance methods like `Find()`, `IsMatch()`, and `Get()`, available on every node in the tree
- **`Document`** — inherits the `Node` methods, plus direct lookup methods like `GetElementsById()`
- **`IEnumerableOfNodeExtensions`** — extension methods on `IEnumerable<Node>` that provide jQuery-style chaining over collections

### Node: the core methods

Every `Node` in the tree — including `Element`, `Document`, and any other node type — exposes the selector engines directly. All of these accept either a `string` selector, a pre-parsed `Selector`, or a `CompoundSelector`:

```csharp
// Find all matching elements in this subtree (uses the Find engine + query planner):
IReadOnlySet<Element> results = myNode.Find(".foo");

// Test whether this node matches a selector (uses the IsMatch engine):
bool matches = myNode.IsMatch("div.highlighted");

// Get the *first* matching element in document order:
Element? first = myNode.Get("ul > li.active");
```

The `string` overloads handle selector parsing transparently. Under the hood, `GetCompoundSelector()` parses the string and caches the result in the document's `ElementLookupTables.ParsedSelectors` LRU cache (up to 1,024 entries), so repeated calls with the same string don't re-parse.

#### Fast-path for trivial selectors

`Node.Find(string)` includes a special fast path for the most common case: a bare ID (`"#foo"`) or classname (`".bar"`) called from the document root. When this is detected, `Find()` bypasses the selector parser and query planner entirely and performs a direct hash-table lookup via `GetElementsById()` or `GetElementsByClassname()`, returning the result in essentially constant time. The check is deliberately conservative — any whitespace, combinators, or other syntax falls through to the full engine.

### Document: direct lookups

`Document` implements `IElementLookupContainer` and exposes the five live hash indexes directly for cases where a programmer already knows what they're looking for:

```csharp
Document doc = new Document(html);

IReadOnlyCollection<Element> byId    = doc.GetElementsById("main");
IReadOnlyCollection<Element> byClass = doc.GetElementsByClassname("widget");
IReadOnlyCollection<Element> byType  = doc.GetElementsByType("div");
IReadOnlyCollection<Element> byName  = doc.GetElementsByName("email");
IReadOnlyCollection<Element> byTAttr = doc.GetElementsByTypeAttribute("submit");
```

These are the same indexes the query planner uses internally. Calling them directly avoids even the cost of parsing a selector string. They are most useful when you are writing very hot code paths and already know exactly the index you want.

### IEnumerableOfNodeExtensions: collection chaining

The extension methods on `IEnumerable<Node>` make it possible to chain selector operations across collections, much like jQuery or LINQ. Every method that accepts a `string` selector will lazily parse it on first use and share the parsed form across all elements in the collection.

#### Searching

```csharp
// Find descendants matching a selector across multiple subtrees:
IReadOnlySet<Element> items = someNodes.Find("li.active");

// Find the closest matching ancestor for each node:
IReadOnlyCollection<Element> containers = someNodes.Closest(".container");
```

#### Filtering

```csharp
// Keep only the nodes that match:
IEnumerable<Element> matches = someNodes.Where("div.highlighted");

// Keep only the nodes that do *not* match:
IEnumerable<Node> nonMatches = someNodes.Except(".hidden");

// Test existence:
bool anyActive = someNodes.Any(".active");
bool allValid  = someNodes.All("[required]");
```

#### Traversal

```csharp
// Project to children, optionally filtered:
IReadOnlyCollection<Element> kids       = someNodes.Children();
IReadOnlyCollection<Element> activeKids = someNodes.Children(".active");

// Project to descendants, optionally filtered:
IReadOnlyCollection<Element> all        = someNodes.Descendants();
IReadOnlyCollection<Element> allFoos    = someNodes.Descendants(".foo");

// Project to parents, optionally filtered:
IReadOnlyCollection<Element> parents      = someNodes.Parents();
IReadOnlyCollection<Element> navParents   = someNodes.Parents("nav");

// Project to ancestors, optionally filtered:
IReadOnlyCollection<Element> ancestors      = someNodes.Ancestors();
IReadOnlyCollection<Element> formAncestors  = someNodes.Ancestors("form");
```

#### Ordering

```csharp
// Sort into document order (or reverse):
IEnumerable<Element> ordered = elements.OrderByPosition();
IEnumerable<Element> reversed = elements.OrderByPositionDescending();
```

#### Classname manipulation

The extension methods also provide bulk classname operations that don't involve the selector engine at all, but complement it naturally:

```csharp
// Filter to elements with a classname:
IEnumerable<Element> foos = someNodes.HasClass("foo");

// Add, remove, toggle, or update classnames in bulk:
someNodes.AddClass("highlighted");
someNodes.RemoveClass("hidden");
someNodes.ToggleClass("selected");
someNodes.UpdateClass(add: "active", remove: "inactive");
```

### Putting it all together

A typical workflow might look like this:

```csharp
// Parse some HTML into a document.
Document doc = new Document("<div class='app'><ul><li class='item'>A</li>...</ul></div>");

// Find all list items inside the app container.
IReadOnlySet<Element> items = doc.Find(".app li.item");

// Check if any are marked as selected.
if (items.Any(".selected"))
{
    // Get just the selected ones.
    IEnumerable<Element> selected = items.Where(".selected");

    // Walk up to find their containing lists.
    IReadOnlyCollection<Element> lists = selected.Closest("ul");

    // Add a class to those lists.
    lists.AddClass("has-selection");
}
```

Behind this simple code, the full power of the selector engines is at work: the `Find()` call invokes the query planner to pick the cheapest index, the `Any()` and `Where()` calls use the adaptive JIT-compiled `IsMatch()` engine, and `Closest()` walks the ancestor chain testing each element. But none of that complexity is visible to the programmer — the API reads like straightforward collection operations, which is exactly the point.
