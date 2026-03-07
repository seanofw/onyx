# HTML Parsing

## Overview

The Onyx HTML parser is a standards-compliant HTML5 parser implemented entirely in managed C#. It takes raw HTML text as input, and it produces a DOM tree of `Node` objects as output. The parser is designed for Onyx's primary use case of building application UIs, not for rendering arbitrary web pages, so it focuses on body content and ignores metadata elements like `<head>` and `<title>`, which are simply treated as ordinary block elements.

(If needed, you can write custom logic to evaluate `<head>` and its child elements for broader compatibility with existing web content.  As an example, see the section below on [Document titles](#document-titles).)

The parser is split into two cooperating layers:

- **`HtmlLexer`** — a low-level lexical analyzer that converts raw HTML text into a stream of `HtmlToken` objects (start tags, end tags, text, comments).
- **`HtmlParser`** — a tree builder that consumes tokens from the lexer and assembles them into a DOM tree, applying HTML5 recovery rules for malformed markup along the way.

These two layers are always used together during parsing, but `HtmlLexer` can be used independently if you only need tokenization.

> **Visibility conventions in this document:** Items marked *(internal)* are accessible within the Onyx assembly but are not part of the public API. Items marked *(private)* are implementation details of their containing class. Both are documented here for completeness, but external consumers of the library should not depend on them.

### What the parser produces

A call to `HtmlParser.Parse()` returns a `Document` containing a tree of:

- **`Element`** nodes — one per HTML tag, with attributes stored in an `AttributeDictionary`.
- **`TextNode`** nodes — for runs of plain text between tags, with HTML entities decoded.
- **`CommentNode`** nodes — for `<!-- ... -->` comments, with the marker syntax stripped.

Every node carries a `SourceLocation` recording the filename, line, column, character offset, and length of the original source text that produced it.

### What the parser does not do

- It does not fetch resources. There is no HTTP client, no image loading, no `<link>` resolution.  Creating, loading, or retrieving resources is the programmer's responsibility.
- It does not execute scripts. `<script>` content is captured as raw text inside the element, but never evaluated.  No JavaScript parser or runtime is included with Onyx.
- It does not construct a `<head>`/`<body>` structure. There is no implicit `<html>` or `<body>` wrapper; the `Document` directly contains whatever elements appear in the input.
- It does not support CDATA sections.  Future versions may support CDATA.

---

## Usage

### Parsing a full document

```csharp
HtmlParser parser = new HtmlParser();
Document doc = parser.Parse("<div><p>Hello, world!</p></div>", "example.html");
```

The `filename` parameter is used only for error reporting — it appears in `SourceLocation` objects and in warning messages. It does not need to refer to a real file.

After parsing, any warnings about malformed markup are available in `parser.Messages`.

### Parsing a document fragment

```csharp
HtmlParser parser = new HtmlParser();
DocumentFragment fragment = parser.ParseDocumentFragment("<li>Item 1</li><li>Item 2</li>", "fragment.html");
```

`ParseDocumentFragment()` works identically to `Parse()`, but returns a `DocumentFragment` instead of a `Document`. A `DocumentFragment` is a lightweight container that omits the selector-lookup indexes and style management that `Document` provides. Use it when you need a throwaway container for temporary HTML content, or for when you do not intend to render HTML content and are only using Onyx as an HTML parser.

### Parsing into an existing element (InnerHtml)

*(Internal — documented for completeness. Not accessible outside the Onyx assembly.)*

```csharp
// This is used internally by the Element.InnerHtml setter.
HtmlParser.ParseInnerHtml("<b>bold</b>", someElement);
```

`ParseInnerHtml()` and `ParseOuterHtml()` are `internal static` methods. They are the mechanisms behind the `Element.InnerHtml` and `Element.OuterHtml` property setters, respectively. `ParseInnerHtml()` clears the target element's children and re-parses new HTML into it; `ParseOuterHtml()` parses HTML and returns a `DocumentFragment`. Both use a `[ThreadStatic]` `HtmlParser` instance — one per thread, created lazily — to avoid allocation overhead while remaining safe for concurrent use from different threads. External callers should use the `Element.InnerHtml` and `Element.OuterHtml` properties instead of calling these methods directly.

### Using the lexer standalone

```csharp
HtmlLexer lexer = new HtmlLexer("<p class=\"big\">Hello</p>", "test.html");
HtmlToken token;
while ((token = lexer.Next()).Kind != HtmlTokenKind.Eoi)
{
    Console.WriteLine($"{token.Kind}: {token.Text}");
}

// Output:
//   StartTag: p
//   Text: Hello
//   EndTag: p
```

The lexer can be useful on its own when you need to scan HTML without building a full DOM tree — for example, to extract tag names, count elements, or locate specific attributes.

---

## Architecture

### Data flow

```
  Raw HTML string
       │
       ▼
  ┌───────────┐     HtmlToken stream     ┌──────────┐
  │ HtmlLexer │ ───────────────────────► │HtmlParser│
  └───────────┘                          └──────────┘
                                              │
                                              ▼
                                     Document / DOM tree
                                     (Element, TextNode,
                                      CommentNode)
```

The lexer is created by the parser and is not exposed to the caller. The parser reads tokens one at a time via `lexer.Next()`, and either attaches nodes to the tree or manipulates the open-element stack in response.

### Key classes

| Class | Role |
|---|---|
| `HtmlLexer` | Tokenizes raw HTML text into `HtmlToken` objects. |
| `HtmlToken` | An immutable token: a start tag, end tag, text chunk, or comment. |
| `HtmlTokenKind` | Enum distinguishing token types: `StartTag`, `EndTag`, `Text`, `Comment`, `Eoi`. |
| `HtmlParser` | Consumes tokens and builds a DOM tree, applying HTML5 error-recovery rules. |
| `NodeStack<T>` | *(internal)* A stack of open `ContainerNode` objects used during tree construction. Not accessible outside the Onyx assembly. |

### Dependencies

The parser depends on these Onyx types (all found within the `Onyx` assembly):

| Type | Purpose |
|---|---|
| `Document` | The root of a parsed DOM tree. Subclass of `ContainerNode`. |
| `DocumentFragment` | A lightweight alternative root for parsed fragments. Subclass of `ContainerNode`. |
| `ContainerNode` | Abstract base for nodes that can contain children (`Document`, `DocumentFragment`, `Element`). |
| `Node` | Abstract base class for all nodes, including `Document`, `DocumentFragment`, `ContainerNode`, `Element`, `TextNode`, and `CommentNode`. |
| `Element` | Represents an HTML element with a tag name, attributes, and children. |
| `TextNode` | A leaf node containing decoded plain text. |
| `CommentNode` | A leaf node containing comment text (without `<!--`/`-->` markers). |
| `AttributeDictionary` | *(internal)* An `IDictionary<string, string>` that wraps an element's attributes and notifies the element on changes. |
| `SourceLocation` | Tracks where in the source a token or node originated (filename, line, column, offset, length). Immutable. |
| `Messages` / `Message` / `MessageKind` | A thread-safe collection of parser warnings. Each `Message` has a `Kind`, `Text`, and optional `Location`. |
| `HtmlEntities` | Provides HTML entity decoding (`&amp;` → `&`, `&#x20;` → ` `, etc.). |
| `StringExtensions` | Provides `FastLowercase()` *(internal)*, `HtmlDecode()`, and `HtmlDecodeTo()` extension methods. |

The parser has no external dependencies — it requires nothing beyond .NET 8+ and the Onyx core assembly.

---

## HTML Entities (`HtmlEntities`)

`HtmlEntities` is a public static class that provides HTML entity encoding and decoding. It is the engine behind all entity handling in the lexer, and can also be used independently.

### Public API

| Method | Description |
|---|---|
| `Escape(ReadOnlySpan<char>, bool pureAscii, bool controlCodes)` | Encodes text by replacing known characters with named entities (e.g., `&` → `&amp;`), or numeric entities for characters without names. Returns a new string. |
| `EscapeTo(ReadOnlySpan<char>, StringBuilder, ...)` | Same as `Escape()`, but appends to an existing `StringBuilder` to avoid allocation. |
| `Unescape(ReadOnlySpan<char>)` | Decodes text by replacing `&entity;` forms with their equivalent characters. Returns a new string. |
| `UnescapeTo(ReadOnlySpan<char>, StringBuilder)` | Same as `Unescape()`, but appends to an existing `StringBuilder`. |
| `IsKnownEntity(char)` | Returns `true` if the character has a known named HTML entity. Optimized to a few machine instructions via a direct memory lookup. |

| Property | Description |
|---|---|
| `EntitiesToValues` | `IReadOnlyDictionary<string, int>` mapping entity names to Unicode code points. |
| `ValuesToEntities` | `IReadOnlyDictionary<int, string>` mapping Unicode code points to entity names. |
| `Entities` | `IReadOnlyList<(string, int)>` of all known entities in numerical order. |

### Entity table

The entity table covers the standard HTML 4 named entity set — approximately 250 named entities spanning Latin characters, Greek letters, mathematical symbols, typographic marks, arrows, and card suits. Entity names are **case-sensitive**, matching the HTML standard (e.g., `&Omega;` and `&omega;` are distinct entities).

The extended HTML 5 named entity set is not supported.

### Decoding behavior

The `Unescape()` / `UnescapeTo()` methods follow HTML 5 rules:

- **Named entities** are matched against the entity table. Unrecognized names are passed through verbatim (the `&` and the name are copied to the output as-is).
- **Decimal numeric entities** (`&#60;`, `&#169;`) are supported for code points 0 through 0x110000.
- **Hexadecimal numeric entities** (`&#x3C;`, `&#x2014;`) are supported, case-insensitively (`&#x3c;` and `&#x3C;` are equivalent). Up to 6 hex digits are accepted, covering the full Unicode range.
- **The trailing semicolon is optional.** Both `&amp;` and `&amp` are decoded to `&`. This matches HTML 5 parsing behavior.
- **A bare `&`** that is not followed by `#` or a letter/digit sequence is copied to the output verbatim.

### Encoding behavior

The `Escape()` / `EscapeTo()` methods replace characters that have known entities with their named form (`&amp;`, `&lt;`, etc.), and use decimal numeric entities (`&#NNN;`) for characters that don't have a named entity but still need escaping.

Two optional flags control escaping scope:

- `pureAscii` — when `true`, all characters above code point 127 are escaped, producing output that is safe for ASCII-only contexts.
- `controlCodes` — when `true`, characters in the range 0–31 are also escaped.

### Minor differences from the HTML 5 standard

- **Named entity coverage is HTML 4, not HTML 5.** The full HTML 5 named entity set contains approximately 2,000 entries; Onyx's table contains only the ~250 entities defined in HTML 4 and earlier. In practice, the HTML 4 set covers nearly all entities encountered in real-world markup. Unrecognized HTML 5 entity names will pass through undecoded (e.g., `&Hat;` will remain as the literal text `&Hat;`).

### Performance notes

Entity decoding is on the hot path during HTML lexing, so `HtmlEntities` is heavily optimized:

- **`IsKnownEntity()`** uses a 16 KB direct-lookup bitmap allocated off the managed heap (via `Marshal.AllocHGlobal`). Testing whether a character has a named entity reduces to a bounds check and a single byte read — no hashing, no branching.
- **`MakeEntityKey()`** *(private)* packs up to 8 ASCII characters into a `ulong` to avoid allocating a temporary string for dictionary lookups during decoding. This works because the longest entity name in the table is 8 characters (`thetasym`). The method uses an unrolled switch with fall-through for maximum throughput.
- **The `To` variants** (`EscapeTo`, `UnescapeTo`) accept a `StringBuilder` to allow the caller to reuse a buffer, avoiding allocation on repeated calls. The lexer uses `UnescapeTo` internally for this reason.

### Thread safety

`HtmlEntities` is a static class with immutable lookup tables initialized once in the static constructor. All public methods are stateless and safe to call from any thread.

---

## The Lexer (`HtmlLexer`)

### Construction

```csharp
HtmlLexer lexer = new HtmlLexer(text, filename, messages);
```

- `text` — the full HTML source as a single string.
- `filename` — a label for error messages; does not need to be a real file path.
- `messages` — an optional `Messages` collection for warnings; if null, the lexer creates its own.

A lexer instance is single-use: it reads one document from beginning to end, maintaining internal pointer state. To parse another document, create a new lexer.

### Token types

| `HtmlTokenKind` | Meaning | `Token.Text` contains | `Token.Attributes` |
|---|---|---|---|
| `StartTag` | An opening tag like `<div>` or `<img>` | The tag name (original case) | The attribute list (ordered, not deduplicated) |
| `EndTag` | A closing tag like `</div>` | The tag name (original case) | Always null |
| `Text` | A run of plain text between tags | The decoded text content | Always null |
| `Comment` | An HTML comment `<!-- ... -->` | The comment body (without markers) | Always null |
| `Eoi` | End of input | Empty string | Always null |

### Tokenization rules

**Plain text** is everything up to the next `<` character. HTML entities within text are decoded during lexing (e.g., `&amp;` becomes `&`).

**Start tags** begin with `<` followed by a letter. The lexer collects the tag name (everything up to whitespace, `>`, `/`, `=`, or `"`), then collects zero or more attributes. Each attribute is a name, optionally followed by `=` and a value. Values may be double-quoted, single-quoted, or unquoted. HTML entities are decoded in both attribute names and values. Self-closing slashes (e.g., `<br/>`) are silently discarded — in HTML5, self-closing syntax is meaningless, and the parser handles auto-closing separately.

**End tags** begin with `</` followed by a letter. Only the tag name is collected; attributes on end tags are not supported. A missing closing `>` generates a warning but the tag is still recognized.

**Comments** begin with `<!--` and end at `-->`. If the closing `-->` is never found, the rest of the input is consumed as comment text and a warning is emitted.

**Bare `<` characters** that are not followed by a letter, `/`, or `!` are emitted as literal `Text` tokens with a warning. Similarly, `</` not followed by a letter is emitted as literal text. This matches the HTML5 spec's treatment of invalid markup.

### Entity decoding

All text content, tag names, and attribute names/values are HTML-entity-decoded during lexing. This includes named entities (`&amp;`, `&lt;`, `&copy;`, etc.), decimal numeric entities (`&#60;`, `&#169;`), and hexadecimal numeric entities (`&#x3C;`, `&#x2014;`). Decoding is performed via `HtmlEntities.Unescape()` / `HtmlEntities.UnescapeTo()`. See the [HTML Entities](#html-entities-htmlentities) section below for full details on entity support, including known limitations.

### Newline handling

The lexer tracks line numbers for `SourceLocation` reporting. It recognizes all four common newline conventions: `\n`, `\r`, `\r\n`, and `\n\r`. Each of these counts as a single newline. The implementation uses a bitwise expression, `ch ^ 7`, to convert `\r` (0x0D) to `\n` (0x0A) and vice versa, allowing it to efficiently detect and skip the second character of a two-character newline sequence.

### Lookahead

The lexer supports one token of lookahead via `Peek()` and `Unget()`. The unget buffer is exactly one token deep; calling `Unget()` twice without an intervening `Next()` throws `InvalidOperationException`.

### Raw content consumption

`ConsumeToMarker(string marker)` is a special method used by the parser to handle raw-content tags. It scans forward from the current position to find the marker string (e.g., `</script>`) and returns everything before it as a plain string, without any HTML parsing or entity decoding. The comparison can be case-insensitive.

---

## The Parser (`HtmlParser`)

### Construction

```csharp
HtmlParser parser = new HtmlParser(messages);
```

- `messages` — an optional `Messages` collection for warnings. If provided, warnings are written to this collection; if null, a new `Messages` is created. Warnings are accessible via `parser.Messages` after parsing.

A parser instance can be reused for multiple `Parse()` calls. The `Messages` collection accumulates across calls unless cleared manually.

### The parsing loop (`ParseTokens`)

*(Private — documented for completeness.)*

The core of the parser is the private `ParseTokens()` method, which maintains a `NodeStack<ContainerNode>` — a stack of currently-open container nodes — and operates as a push-down automaton. The root node (either a `Document` or `DocumentFragment`) is always at the bottom of the stack.

For each token from the lexer:

1. **`Text`** → Creates a `TextNode` and appends it to the top node of the stack.
2. **`Comment`** → Creates a `CommentNode` and appends it to the top node.
3. **`StartTag`** → Creates an `Element` via `MakeElement()`, then:
   - Calls `EnsureTreeAllowsStartTag()` to fix up the stack if this element can't legally appear at the current position.
   - Appends the element to the top node.
   - If the tag is **auto-closing** (`<br>`, `<hr>`, `<img>`, `<input>`, `<link>`, `<meta>`) or starts with `!` (e.g., `<!DOCTYPE>`): the element is added but not pushed onto the stack.
   - If the tag is **raw-content** (`<script>`, `<style>`, `<xmp>`, `<plaintext>`): the lexer's `ConsumeToMarker()` is called to capture everything up to the matching `</tag>` as a single `TextNode` child. The element is not pushed onto the stack.
   - Otherwise: the element is pushed onto the stack as the new current node.
4. **`EndTag`** → If the tag name matches the current node's name, the current node is popped (clean close). Otherwise, `RecoverFromMismatchedEndTag()` is invoked to apply HTML5 recovery rules.

### Element creation (`MakeElement`)

*(Private — documented for completeness.)*

`MakeElement()` converts an `HtmlToken` into an `Element`:

- The tag name is lowercased via `FastLowercase()`.
- Attributes are stored in an `AttributeDictionary` backed by a `Dictionary<string, string>`.
- Duplicate attribute names are resolved by keeping only the first occurrence (`TryAdd` semantics).
- `Element.OnAttrChange()` is called for each attribute, which handles special attributes like `class` and `id`.

### Auto-closing tags

These elements are recognized as self-closing and are never pushed onto the open-element stack. The set is defined by `Element.AutoClosingTags` *(internal)*:

* `br`, `hr`, `img`, `input`, `link`, `meta`

Tags beginning with `!` (like `<!DOCTYPE>`) are also treated as auto-closing.

### Raw-content tags

These elements have their content consumed as raw text (no HTML parsing inside them). The set is defined by `Element.RawContentTags` *(internal)*:

* `script`, `style`, `xmp`, `plaintext`

The closing tag is found by a case-insensitive search for the marker `</tagname>`. Everything between the start tag and the closing marker becomes a single `TextNode` child. This means HTML tags inside `<style>` or `<script>` are not parsed — they're just text.

---

## Error Recovery

The parser never throws on malformed input. Instead, it applies HTML5-inspired recovery rules and emits warnings to `parser.Messages`. All warnings have `MessageKind.Warning` severity.

### Mismatched end tags (`RecoverFromMismatchedEndTag`)

*(Private — documented for completeness.)*

When an `</endtag>` is encountered and it does not match the current node, the parser walks backward up the `NodeStack` looking for a matching open element to close. The search is governed by **mismatch rules**, which define two things for each tag:

- **Closing tags** — which open elements this end tag is allowed to close.
- **Interrupting tags** — which open elements should stop the backward search (because they represent a structural boundary).

The mismatch rules cover all the standard HTML5 cases:

| End tag | Can close | Search stops at |
|---|---|---|
| `</li>` | `<li>` | `<ul>`, `<ol>`, `<menu>` |
| `</dt>` | `<dt>`, `<dd>` | `<dl>` |
| `</dd>` | `<dt>`, `<dd>` | `<dl>` |
| `</p>` | `<p>` | Any block-level element |
| `</rt>` | `<rt>`, `<rp>` | `<ruby>` |
| `</rp>` | `<rt>`, `<rp>` | `<ruby>` |
| `</optgroup>` | `<option>`, `<optgroup>` | `<select>` |
| `</option>` | `<option>` | `<optgroup>`, `<select>` |
| `</thead>` | `<tbody>`, `<tfoot>` | `<table>` |
| `</tbody>` | `<tbody>` | `<table>` |
| `</tfoot>` | `<tbody>`, `<tfoot>` | `<table>` |
| `</tr>` | `<tr>` | `<tbody>`, `<thead>`, `<tfoot>`, `<table>` |
| `</td>` | `<td>`, `<th>` | `<tr>`, `<tbody>`, `<thead>`, `<tfoot>`, `<table>` |
| `</th>` | `<td>`, `<th>` | `<tr>`, `<tbody>`, `<thead>`, `<tfoot>`, `<table>` |
| `</colgroup>` | `<colgroup>` | `<table>` |

For end tags not listed above (like `</div>` or `</span>`), the parser searches backward for a matching start tag and stops searching if it hits any block-level element.  (For a list of known block-level elements, see the [Structural Enforcement](#structural-enforcement-ensuretreeallowsstarttag) section below.)

If a matching open element is found, everything above it on the stack is popped (with warnings about the unclosed elements). If the search is interrupted or no match is found, the end tag is silently discarded.

The root node is never popped — the search always stops before it.

### Structural enforcement (`EnsureTreeAllowsStartTag`)

*(Private — documented for completeness.)*

Before a start tag is inserted into the tree, the parser verifies that the current position in the tree is structurally valid. There are two categories of enforcement:

**Block-level elements**, as defined by `Element.BlockLevelElements` *(internal)* must appear inside other block-level elements. If the current node is inline (like `<span>` or `<a>`), inline ancestors are popped until a block-level ancestor is reached. The `<p>` element is a special case: it is always auto-closed by any block-level element, even though `<p>` is itself block-level.  These are the recognized block-level elements, in alphabetical order:

* `article`, `aside`
* `blockquote`
* `column`*
* `details`, `dialog`, `div`, `dl`
* `fieldset`, `figcaption`, `figure`, `footer`, `form`
* `h1`, `h2`, `h3`, `h4`, `h5`, `h6`, `header`, `hgroup`, `hr`
* `main`
* `nav`
* `row`*
* `ol`
* `p`, `pre`
* `section`
* `table`
* `ul`

\*For more details on the nonstandard `row`/`column` elements, see [Row/column elements](#rowcolumn-elements) below.

**Special elements** have specific parent requirements:

| Element | Required ancestor | Synthesized if missing |
|---|---|---|
| `<li>` | `<ol>` or `<ul>` | `<ul>` |
| `<dt>`, `<dd>` | `<dl>`, `<dt>`, or `<dd>` | `<dl>` |
| `<tbody>` | `<table>` | `<table>` |
| `<tr>` | `<thead>`, `<tbody>`, or `<tfoot>` | `<tbody>` |
| `<td>`, `<th>` | `<tr>` | `<tr>` |

When a required ancestor is missing entirely, the parser synthesizes one and inserts it into the tree (which may recursively trigger further synthesis — for example, a bare `<td>` will cause both a `<tr>` and a `<tbody>` to be synthesized). When the required ancestor exists but is buried under other elements on the stack, those intervening elements are popped.

For `<dt>` and `<dd>`, if the current node is already a `<dt>` or `<dd>`, it is auto-closed before the new one is inserted (since definition terms and descriptions are siblings, not nested).

### Row/column elements

The `<row>` and `<column>` elements are unique to Onyx and are not part of any web standard.  However, while they are recognized by the Onyx parser as block-level elements, they are otherwise not special for parsing, rendering, input, or layout.  Onyx's default stylesheet assigns two style rules to make these two elements useful:

* `row { display: flex; flex-flow: row nowrap }`
* `column { display: flex; flex-flow: column nowrap }`

These two elements exist so that they can produce cleaner layout markup than "div soup" often does.  They also provide an easy transition from other UI frameworks that have a prebuilt construct describing a "row of components" or a "column of components."  Existing HTML markup will work verbatim without these elements.  To use these elements inside a web browser, only the above two CSS declarations are required (possibly with a polyfill, depending on the browser).

The inclusion of these two elements is the only place where Onyx wilfully violates the HTML5 standard, and it does so only to simplify application development for the most common of all use cases:  basic layout.  Everywhere else, Onyx attempts to match web standards as closely as possible.

---

## The Node Stack (`NodeStack<T>`)

*(Internal — documented for completeness. Not accessible outside the Onyx assembly.)*

`NodeStack<T>` is an `internal ref struct` (stack-allocated, cannot escape to the heap) that tracks which container nodes are currently open during parsing.

- **`CurrentNode`** — the topmost node, to which new children are appended.
- **`PushNode(T)`** — pushes a new open element.
- **`PopNode()`** — pops the top element (closing it). Updates `CurrentNode`.
- **`FindAncestor(string[])`** — searches downward from the top for a node whose `NodeName` matches any of the given names. Used by `EnforceAncestor()`.
- **Indexed access** — `stack[i]` accesses by position, from oldest (0) to newest (Count-1).

The stack starts with an initial capacity (64 in practice) and doubles when full (which rarely happens in real-world documents). The root node is always at position 0 of the stack and is never popped.

---

## Warnings and Messages

The parser never throws exceptions on bad HTML input. Instead, it records warnings in the `Messages` collection, which is accessible as `parser.Messages` (and also `lexer.Messages`, though the parser and lexer share the same collection when used together).

Warnings are issued for situations including:

- Bare `<` characters that don't begin a valid tag.
- Missing closing `>` on end tags.
- Missing closing quote on attribute values.
- Illegal characters inside tags.
- Self-closing `/` characters in start tags (HTML, not XML).
- Mismatched end tags.
- Unclosed start tags (discovered during recovery).
- Structural violations (e.g., `<td>` outside a `<tr>`).
- Missing ancestors that had to be synthesized.
- Unterminated comments.
- Extra text after an attribute value's closing quote.

All warnings have `MessageKind.Warning` severity. The parser does not produce errors or halt on bad input — it always produces a DOM tree, no matter how broken the input is.

Each `Message` carries:
- `Kind` — always `Warning` for parser messages.
- `Text` — a human-readable description of the problem.
- `Location` — a `SourceLocation` pointing to where in the source the problem was detected.

While the parser and lexer produce warning messages as output, they use standards-compliant error recovery, and the messages may be ignord.  The `Messages` collection is included only so that developers can identify mistakes in markup and correct them, if so desired.

---

## Exceptions

The parser and lexer do not throw exceptions on malformed HTML. The only exceptions that can be thrown are:

- **`InvalidOperationException`** — if `HtmlLexer.Unget()` is called twice without an intervening `Next()`. This is a programming error, not a data error.
- **`ArgumentOutOfRangeException`** — if a `NodeStack` is constructed with a size less than 1. This is also a programming error.

Standard .NET exceptions like `ArgumentNullException` or `NullReferenceException` may occur if null is passed where a non-null string is expected (e.g., `null` for the `text` parameter of `Parse()`), but these are not part of the parser's contract — don't pass null.

---

## Thread Safety

`HtmlParser` instances are **not thread safe**. Do not call `Parse()` on the same parser instance from multiple threads simultaneously. The shared `Messages` collection would be corrupted, and the internal state of the lexer would be undefined.

However, there is no thread affinity — you can create a parser on one thread and use it on another, as long as only one thread uses it at a time.

`HtmlToken` and `SourceLocation` are immutable and therefore safe to share across threads after creation.

The internal `ParseInnerHtml()` and `ParseOuterHtml()` methods use a `[ThreadStatic]` `HtmlParser` instance, so each thread gets its own parser. Setting `Element.InnerHtml` or `Element.OuterHtml` concurrently from different threads is safe with respect to the parser itself (though the usual threading rule still applies: don't mutate the same DOM tree from multiple threads).

---

## Unusual Behaviors

### Tag names are lowercased

All tag names are lowercased during element creation. `<DIV>`, `<Div>`, and `<div>` all produce an element with `NodeName` equal to `"div"`. Attribute names are also lowercased by `AttributeDictionary`. Attribute values are preserved as-is.

Onyx does this case-folding to enforce the HTML standard's mandatory case-insensitivity for tag names and attribute names. Rather than performing case-insensitive comparisons throughout the codebase (which would be both slower and error-prone), Onyx folds case exactly once at the input boundary — during parsing — so that all internal comparisons can be simple ordinal string equality. This ensures case-folding is performed once and only once per input datum.

### First attribute wins

When a start tag contains duplicate attribute names (e.g., `<div class="a" class="b">`), only the first value is kept. This matches the HTML5 specification.

### No implicit `<html>` or `<body>`

Unlike a browser, the parser does not wrap content in `<html>` and `<body>` elements. If you write `<p>Hello</p>`, the resulting `Document` directly contains the `<p>` element. Elements like `<html>`, `<head>`, and `<body>` are parsed as ordinary elements with no special treatment.

This is intentional: Onyx is not a browser. Most of what a `<head>` section normally does for a document — loading resources, parsing scripts, setting titles, handling encodings — can be handled better in direct C# code than `<head>` markup would normally allow. Onyx leaves the programmer responsible for metadata and loading behaviors that they would typically *want* to be responsible for, rather than attempting to handle it implicitly and often incorrectly.

### `<!DOCTYPE>` is ignored

`<!DOCTYPE html>` is effectively meaningless to Onyx. The parser will accept it without error, but no part of Onyx assigns it any meaning:  It is treated as an auto-closing tag, producing an element node named `!doctype` in the DOM. There is no quirks mode, no older compatibility parsing, and no XHTML parsing — these omissions are intentional, as all of these are dead older standards. Onyx targets web standards and only web standards, and for those purposes, `<!DOCTYPE html>` is a meaningless addition to the markup. You may include it or omit it: It makes no difference to parsing or layout.

### Attribute values are always strings

Attributes with no value (e.g., `disabled` in `<input disabled>`) are stored with a `null` value at the lexer level, but the parser converts all null attribute values to `string.Empty` before storing them in the `AttributeDictionary`.  This disparity is intentional:  Any direct consumer of a lexer can tell which form was used, but the DOM specification requires that valueless attributes are treated as equivalent to the empty string, so Onyx complies with the standard by converting them to the empty string when populating an `AttributeDictionary`.

### Sharing a `Messages` collection

You can pass a `Messages` collection to the `HtmlParser` constructor to share it with other components (e.g., a `CssParser`). If you don't pass one, the parser creates its own. Either way, warnings are always accessible via `parser.Messages`.

A `Messages` collection is fully thread-safe, but sharing it among many threads may result in confusing output, as the messages will be intermixed in the order in which they were issued.  However, sharing a single `Messages` collection across threads will work, if you wish to do so.

### `ParseInnerHtml` uses a thread-static parser

*(Internal — documented for completeness.)*

The `ParseInnerHtml()` and `ParseOuterHtml()` methods *(internal)* use a `[ThreadStatic]` `HtmlParser` instance — one per thread, created lazily on first use. This avoids both allocation overhead on repeated calls and concurrency issues across threads.

### Raw content tags consume greedily

For raw-content tags (`script`, `style`, `xmp`, `plaintext`), the lexer scans for the closing tag marker using a simple string search. Everything inside is captured as a single undecoded `TextNode`. No nesting is recognized — a `</script>` inside a JavaScript string literal will still end the `<script>` element.  This matches HTML5 parsing rules, and is the same behavior expected in a browser.

Note that the deprecated HTML 1.0 `<plaintext>` tag is supported, and will consume all remaining input to a closing `</plaintext>` tag (unlike the HTML 1.0 `<plaintext>` tag which always consumes the remainder of the document).  This tag is included because it, like `<xmp>` can be an easy way to quickly display plain text in a UI.  The `Element` class also contains an `InnerText` property (matching the browser DOM property of the same name) that can support similar use cases.

## Style tags and link tags

The HTML parser does not parse `<style>` tags or `<link>` tags directly.  Onyx includes full support for CSS, but loading resources is intentionally the programmer's responsibility.

However, as Onyx includes all of the necessary pieces to support CSS and the programmer only needs to glue them together, handling `<style>` tags and `<link>` tags is trivial and can be easily customized.  Here is a short code snippet that supports inline stylesheets via `<style>` tags:

```cs
HtmlParser htmlParser = new HtmlParser();
CssParser cssParser = new CssParser();

Document document = htmlParser.Parse(...);

foreach (Element styleTag in document.Find("style"))
{
    Stylesheet stylesheet = cssParser.Parse(styleTag.InnerText);
    document.AddStylesheet(stylesheet);
}
```

That `foreach` loop is shown in a simple, easy-to-understand form, but it can be packed down to a one-liner via Linq:

```cs
document.AddStylesheets(document.Find("style").Select(s => cssParser.Parse(s.InnerText));
```

Similarly, `<link>` tags can be supported by providing your own logic for loading an external resource:

```cs
foreach (Element styleTag in document.Find("link[rel=stylesheet]"))
{
    Uri uri = new Uri(styleTag["href"]);

    string cssText = /* ...do something to load the given URI... */ ;

    Stylesheet stylesheet = cssParser.Parse(cssText);
    document.AddStylesheet(stylesheet);
}
```

(Notice the selector in the `Find()` call:  When in doubt, prefer using selectors rather than manual searching and filtering of elements.)

Because resource loading is not implicit, the programmer can apply whichever loading policies are desired:  Loading from the entire Internet, or only loading from specific servers, or prohibiting sources for security reasons, or only loading from local files, or even loading from custom sources like embedded resources.  Onyx has no opinions on resource loading, so the programmer is free to supply any opinions required.

## Document titles

Onyx does not directly support `<head>` or its children, but you can mimic support for `<head>` where required, as in the above stylesheet example.  This section includes another simple example:  Setting a window title from a document's `<title>` element.

If you are using the Win32 bindings, you create a window in Onyx with `new Window()`.  This `Window` object has a `Title` property, so all that's needed to support the `<title>` element is to proxy the data from the `<title>` element to the `Title` property.  With selectors, this is an easy one-liner, as in the example below:

```cs
HtmlParser htmlParser = new HtmlParser();
Document document = htmlParser.Parse(...);

Window window = new Window();
window.Document = document;
window.Title = document.Find("> head > title")
    .OrderByDocumentPosition()
    .FirstOrDefault()?.InnerText;
```

Notice how the selector is used to find the `<head>` element and the `<title>` elements under it, and then the `.OrderByDocumentPosition()` and `.FirstOrDefault()` pare that set down to just the first `<title>`.  This is the full general case, however.  If you know for certain that your document always contains one `title` element, you can write this far more simply:

```cs
window.Title = document.Find("title").First().InnerText;
```

Processing `<head>` elements is simple enough that Onyx doesn't include special built-in code to do so:  If you need support for `<title>` or `<link>` or `<style>` or `<meta>`, you can trivially code support for those yourself.
