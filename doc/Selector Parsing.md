# CSS Selector Parsing

## Overview

The Onyx CSS selector parser transforms selector text like `div.container > #header span.active` into an in-memory object tree that can be matched against DOM elements, used by `Find()` to search for elements, or inspected for specificity. The parser follows the CSS 2.1 selector grammar with enhancements from CSS 3 (general sibling combinator, `^=`/`$=`/`*=` attribute operators, `::` pseudo-element syntax, and case-sensitivity flags).

Like the HTML parser, the CSS selector parser is split into two cooperating layers:

- **`CssLexer`** — a low-level lexical analyzer that converts raw CSS text into a stream of `CssToken` objects (identifiers, punctuation, strings, numbers, etc.).
- **`CssSelectorParser`** — a recursive-descent parser that consumes tokens from the lexer and assembles them into a selector object tree.

The selector parser does *not* apply error recovery the way the HTML parser does. If the input is malformed, the parser emits a diagnostic and returns `null`; the broader CSS parser (which calls the selector parser) handles recovery at the rule level by skipping the malformed rule entirely.

> **Visibility conventions in this document:** Items marked *(internal)* are accessible within the Onyx assembly but are not part of the public API. Items marked *(private)* are implementation details of their containing class.

### What the parser produces

The parser transforms a selector string into a hierarchy of objects:

```
CompoundSelector           "div, span.active"     (comma-separated list)
 └─ Selector[]             "div" and "span.active" (individual selector chains)
     └─ SelectorComponent[]                        (combinator + simple selector pairs)
         ├─ Combinator                              (how to traverse: space, >, +, ~)
         └─ SimpleSelector                          (what to match)
             ├─ ElementName                         (tag name or "*")
             └─ SelectorFilter[]                    (ID, class, attribute, pseudo-class/element)
```

### What the parser does not do

- It does not match selectors against elements. That is the job of `IsMatch()` and `Find()` on the selector objects.
- It does not compute specificity. Specificity is lazy-evaluated on the selector objects after construction.
- It does not validate that pseudo-classes or pseudo-elements are semantically meaningful. Unknown pseudo-classes are wrapped in `SelectorUnknownPseudoClass` and delegated to `Element.HasPseudoClass()` at match time, which allows applications to define custom pseudo-classes.

---

## Usage

### Parsing a single selector

```csharp
Selector selector = Selector.Parse("div.container > span");
```

This is a convenience method that constructs a `CssLexer` and `CssSelectorParser` internally. It throws `ArgumentException` on malformed input.

### Parsing a compound selector (with commas)

```csharp
CompoundSelector selector = CompoundSelector.Parse("h1, h2, h3");
```

### Safe parsing (no exceptions)

```csharp
if (Selector.TryParse("div > p", out Selector? selector))
{
    // Use selector...
}
```

`TryParse` returns `false` and sets the output to `null` if the input is malformed, without throwing. Diagnostics are recorded in the parser's `Messages` collection but are not surfaced through the convenience API.

### Using the parser directly

```csharp
CssSelectorParser parser = new CssSelectorParser();
CssLexer lexer = new CssLexer("div.foo, span#bar", "<source>");
CompoundSelector? result = parser.ParseCompoundSelector(lexer);
// Warnings/errors available in parser.Messages
```

The `CssSelectorParser` constructor accepts an optional `Messages` collection and a `strict` flag. In strict mode, all diagnostics are emitted as errors; in non-strict mode (the default), they are emitted as warnings, because the CSS property parser recovers from selector errors by skipping the entire rule.

### Using the lexer standalone

```csharp
CssLexer lexer = new CssLexer("div > .active:hover", "<source>");
CssToken token;
while ((token = lexer.Next()).Kind != CssTokenKind.Eoi)
{
    Console.WriteLine($"{token.Kind}: {token}");
}

// Output:
// Ident: div
// Space:
// GreaterThan: >
// Space:
// Dot: .
// Ident: active
// Colon: :
// Ident: hover
```

---

## Grammar

The implemented grammar is documented in the source file header of `CssSelectorParser.cs`. It follows CSS 2.1 with CSS 3 extensions:

```
compound_selector:  selector [ ',' S* selector ]*
selector:           simple_selector [ combinator selector | S+ [ combinator? selector ]? ]?
combinator:         '+' S* | '>' S* | '~' S*
simple_selector:    element_name [ id | class | attrib | pseudo ]*
                  | [ id | class | attrib | pseudo ]+
id:                 '#' IDENT
class:              '.' IDENT
element_name:       IDENT | '*'
attrib:             '[' S* IDENT S* [ [ '=' | '~=' | '|=' | '*=' | '^=' | '$=' ]
                        S* [ IDENT | STRING ] S* [ 'i' | 's' ]? ]? ']'
pseudo:             pseudo_class | pseudo_element
pseudo_class:       ':' [ IDENT '(' S* compound_selector S* ')' | IDENT '(' ... ')' | IDENT ]
pseudo_element:     '::' [ IDENT '(' ... ')' | IDENT ]
```

Notable differences from the CSS 2.1 spec grammar:

| Change | Reason |
|--------|--------|
| `compound_selector` added as top-level nonterminal | The selector parser is invoked standalone, not as part of the full CSS grammar |
| `HASH` terminal replaced by `id` nonterminal | The lexer pre-parses `#foo` as a single `Id` token |
| `FUNCTION` terminal replaced by `IDENT '('` | Consumed greedily by the lexer (identifier followed by `(` becomes a `Func` token) |
| `INCLUDES` and `DASHMATCH` replaced by character strings | Extended with `*=`, `^=`, `$=` from CSS 3 |
| `pseudo` extended with `::` | CSS 3 pseudo-element syntax |
| `~` general sibling combinator added | CSS 3 combinator |
| `'i'` and `'s'` flags in attribute selectors | CSS Selectors Level 4 case-sensitivity control |

---

## CssLexer

`CssLexer` is a hand-written, single-pass lexical analyzer that tokenizes CSS text. It is a general-purpose CSS lexer shared by both the selector parser and the property parser.

### Construction

```csharp
// Simple form: text + filename for diagnostics.
CssLexer lexer = new CssLexer(text, filename);

// Extended form: for inline styles, where the source location needs to match
// the containing HTML document's line/column.
CssLexer lexer = new CssLexer(text, filename, line, column, messages);
```

The extended constructor is used by `Element.ParseInlineStyle()` so that error messages for inline `style="..."` attributes point back to the correct location in the HTML source.

### Token stream

The primary method is `Next()`, which returns the next `CssToken`. Unlike many lexers, **whitespace is significant in CSS** (it is the descendant combinator in selectors), so `CssLexer` returns whitespace as `CssTokenKind.Space` tokens rather than discarding it. The selector parser calls `SkipWhitespace()` explicitly when whitespace is not significant.

Additional navigation methods:

| Method | Description |
|--------|-------------|
| `Next()` | Consume and return the next token |
| `Peek()` | Return the next token without consuming it |
| `Unget(token)` | Push one token back (single-token lookahead; calling twice throws) |
| `Here()` | Snapshot the current position as a `CssLexerPosition` |
| `Rewind(position)` | Restore a previously-saved position |

### Token types

`CssTokenKind` is an `sbyte` enum. The token types relevant to selector parsing are:

| Token | Example | Notes |
|-------|---------|-------|
| `Ident` | `div`, `hover` | CSS identifier; supports Unicode and `\` escapes |
| `Id` | `#header` | The `#` is consumed by the lexer; `Token.Text` holds just the name |
| `String` | `"value"` | Quoted string with escape support |
| `Star` | `*` | Also `*=` → `StarEq` |
| `Dot` | `.` | Starts a class selector; also starts numbers like `.5` |
| `Colon` | `:` | Starts pseudo-classes/elements |
| `Comma` | `,` | Separates selectors in a compound selector |
| `Space` | ` ` | Whitespace (including tabs, newlines); descendant combinator |
| `Plus` | `+` | Adjacent sibling combinator |
| `GreaterThan` | `>` | Child combinator |
| `Tilde` | `~` | General sibling combinator; also `~=` → `TildeEq` |
| `LeftBracket` / `RightBracket` | `[` / `]` | Attribute selector delimiters |
| `LeftParen` / `RightParen` | `(` / `)` | Pseudo-class function arguments |
| `Equal` | `=` | Attribute value comparison |
| `TildeEq` | `~=` | Attribute whitespace-includes operator |
| `BarEq` | `\|=` | Attribute dash-match operator |
| `CaretEq` | `^=` | Attribute starts-with operator |
| `DollarEq` | `$=` | Attribute ends-with operator |
| `StarEq` | `*=` | Attribute contains operator |
| `Eoi` | | End of input |

Multi-character operators like `~=`, `|=`, `^=`, `$=`, and `*=` are recognized greedily by the lexer, so the selector parser never needs to look ahead to distinguish `~` (general sibling) from `~=` (attribute includes).

### CSS comments

CSS comments (`/* ... */`) are consumed entirely by the lexer and never emitted as tokens. The lexer simply skips the comment content and loops back to read the next token (via `goto retry` in `Next()`). Unclosed comments emit an error and fall back to treating `/*` as a plain `/` token.

### Escape sequences

The lexer supports CSS escape sequences in identifiers and strings:

- **Unicode escapes:** `\` followed by 1–6 hex digits, with an optional trailing whitespace character consumed. Values above U+FFFF are replaced by U+FFFD (replacement character). The `EatUnicode()` *(private)* method handles this.
- **Character escapes:** `\` followed by any non-hex, non-newline character represents that character literally. This allows identifiers to contain characters that would otherwise be special, like `\.` or `\:`.
- **Line continuations:** `\` followed by a newline in strings is treated as a line continuation (the newline is consumed but not included in the string).

### Identifier parsing

*(Private — `ParseIdent()`, `ParseName()`)*

CSS identifiers have unusual rules compared to most languages: they may start with a letter, underscore, Unicode character above U+0080, or an escape; they may also start with a hyphen followed by any of those. The continuation characters additionally include digits and hyphens. The `_nameCharKind` lookup table (128-byte array) classifies ASCII characters for fast identifier parsing:

| Value | Meaning | Characters |
|-------|---------|------------|
| 0 | Not part of an identifier | Whitespace, most punctuation |
| 1 | Identifier start character | Letters, underscore |
| 2 | Identifier continuation only | Digits, hyphen |
| 5 | Backslash (escape) | `\` |

Characters above U+0080 are always treated as valid identifier characters.

`ParseName()` is a variant of `ParseIdent()` that does not require a leading letter — it is used for parsing the name part of `#id` tokens, where digits are allowed at the start.

### Chunk-based string building

*(Private — `StartParsingChunks()`, `NextChunk()`, `FinishParsingChunks()`)*

The lexer uses a "chunking" strategy to avoid unnecessary `StringBuilder` allocations. When parsing an identifier or string:

1. **`StartParsingChunks()`** records the start position. No `StringBuilder` is allocated yet.
2. If an escape sequence is encountered, **`NextChunk()`** flushes the plain-text span since the last chunk into a `StringBuilder` (allocated lazily).
3. The decoded escape character is appended to the `StringBuilder`.
4. **`FinishParsingChunks()`** returns either a simple `Substring` (if no escapes were encountered and no `StringBuilder` was needed) or the `StringBuilder`'s content.

This means identifiers and strings with no escapes — the vast majority in practice — are parsed as zero-allocation substrings.

### Newline tracking

Like the HTML lexer, the CSS lexer uses the `ch ^ 7` trick for CRLF/LFCR handling: `'\r' ^ 7 == '\n'` and `'\n' ^ 7 == '\r'`, so after consuming one newline character, it checks if the next character is `text[ptr] == (ch ^ 7)` to consume the other half of a two-character newline sequence.

---

## CssSelectorParser

`CssSelectorParser` is a recursive-descent parser that consumes tokens from a `CssLexer` and produces selector objects. It is not a general CSS parser — it only knows how to parse selectors. (The property/declaration parser is `CssPropertyParser`, documented separately.)

### Construction

```csharp
CssSelectorParser parser = new CssSelectorParser(messages, strict);
```

Both parameters are optional. In strict mode, all diagnostics are errors; in non-strict mode, they are warnings.

If a `Messages` collection is provided, the parser's warnings and errors will be added to it; if omitted, the parser creates its own.

### Top-level entry points

| Method | Returns | Description |
|--------|---------|-------------|
| `ParseCompoundSelector(lexer, expectEoi, throwOnError)` | `CompoundSelector?` | Parses a comma-separated list of selectors |
| `ParseSelector(lexer, expectEoi, throwOnError)` | `Selector?` | Parses a single selector (no commas) |

Both methods accept:

- `expectEoi` (default `true`) — whether to require that nothing follows the selector except whitespace. Set to `false` when parsing selectors embedded in larger constructs (like `:is(...)` arguments or CSS rules where `{` follows the selector).
- `throwOnError` (default `false`) — whether to throw `ArgumentException` on parse failure instead of returning `null`. When `true`, clears `Messages` first. The convenience methods on `Selector` and `CompoundSelector` (`Parse()`) use `throwOnError: true`; the `TryParse()` methods use `throwOnError: false`.

### How parsing works

Parsing proceeds top-down through the grammar, with each nonterminal handled by a dedicated method:

#### 1. `ParseCompoundSelector` — comma-separated selectors

```
compound_selector: selector [ ',' S* selector ]*
```

Parses the first selector, then loops: if the next token is a comma, skip whitespace and parse another selector. Collects them into a `CompoundSelector`.

#### 2. `ParseSelector` — a single selector chain

```
selector: simple_selector [ combinator selector | S+ [ combinator? selector ]? ]?
```

Parses the first `SimpleSelector` and pushes it onto the path as a `SelectorComponent` with `Combinator.Self`. Then enters a loop to detect combinators and subsequent simple selectors.

The combinator-detection loop is the most interesting part of the parser, because **whitespace is ambiguous**: a space can be a descendant combinator (`div span`), or it can precede a non-space combinator (`div > span`). The parser handles this with a `goto retry` pattern:

```csharp
while (true)
{
    Combinator combinator = Combinator.Self;

retry:
    CssToken token = lexer.Next();
    switch (token.Kind)
    {
        case CssTokenKind.Space:
            combinator = Combinator.Descendant;
            goto retry;       // Tentatively record as descendant; retry to see
                               // if the *next* token overrides it with >, +, or ~.

        case CssTokenKind.GreaterThan:
            combinator = Combinator.Child;
            goto parseSimple;  // Concrete combinator found; parse next simple selector.

        case CssTokenKind.Plus:
            combinator = Combinator.AdjacentSibling;
            goto parseSimple;

        case CssTokenKind.Tilde:
            combinator = Combinator.GeneralSibling;
            goto parseSimple;

        case CssTokenKind.Dot:
        case CssTokenKind.Ident:
        // ... other simple-selector start tokens:
            lexer.Unget(token);
            goto parseSimple;  // Start of next simple selector; use whatever
                               // combinator we've accumulated (space or Self).

        default:
            // Not a selector token — selector is complete.
            lexer.Unget(token);
            return new Selector(path);
    }
}
```

This means `div   >   span` works correctly: the spaces set `combinator = Descendant`, but then `>` overrides it to `Child` before the next simple selector is parsed.

#### 3. `ParseSimpleSelector` *(private)* — element name + filters

```
simple_selector: element_name [ id | class | attrib | pseudo ]*
               | [ id | class | attrib | pseudo ]+
```

First checks for an element name (`Ident` → tag name, `Star` → universal `*`). Then loops to collect zero or more filters. The loop exits when it encounters a token that isn't the start of a filter (`.`, `#`, `:`, `[`), pushing that token back.

Returns `null` if neither an element name nor any filter was found (i.e., the input doesn't start a simple selector at all).

#### 4. `ParseClassFilter` *(private)* — `.classname`

Expects an `Ident` token after the `.` dot. Creates `SelectorFilterClass`. Emits "Missing classname after '.'" if the next token isn't an identifier.

#### 5. `ParsePseudoFilter` *(private)* — `:pseudo` and `::pseudo-element`

This method handles the most complex selector syntax:

1. Check for a second `:` to distinguish pseudo-classes from pseudo-elements.
2. Read the name (`Ident`).
3. If the next token is *not* `(`: look up the name in the known pseudo-class/element tables and return the singleton instance, or wrap it as `SelectorUnknownPseudoClass` for extensibility.
4. If the next token *is* `(`: this is a functional pseudo-class.
   - If the name is `is` or `not`: **recursively call `ParseCompoundSelector`** to parse the argument as a full selector list, then wrap it in `SelectorPseudoIsNot`.
   - Otherwise: call `ParseCustomSelectorContent()` *(private)* to collect identifiers and whitespace as a plain string, then wrap it as `SelectorUnknownPseudoClass` with a value.
5. Expect `)` to close the function.

**Known pseudo-classes** (returned as singleton instances):

| Name | Class | Match logic |
|------|-------|-------------|
| `first-child` | `SelectorPseudoFirstChild` | `element.PreviousSibling == null` |
| `last-child` | `SelectorPseudoLastChild` | `element.NextSibling == null` |
| `empty` | `SelectorPseudoEmpty` | `element.Count == 0` |
| `link` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `visited` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `hover` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `active` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `focus` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `enabled` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `disabled` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `checked` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |
| `indeterminate` | `SelectorPseudoStyleFlag` | Bitwise test of `element.StyleFlags` |

**Known pseudo-classes with child selectors:**

| Name | Class | Notes |
|------|-------|-------|
| `:is(...)` | `SelectorPseudoIsNot` | Matches if any child selector matches |
| `:not(...)` | `SelectorPseudoIsNot` | Matches if no child selector matches (XOR inversion) |

**Known pseudo-elements:** The `_knownPseudoElements` table is currently empty. All `::name` forms are treated as unknown and delegated to `Element.HasPseudoElement()`.

#### 6. `ParseAttributeFilter` *(private)* — `[attr op value]`

Parses the full attribute selector syntax after the opening `[`:

1. Skip whitespace, read the attribute name (`Ident`, lowercased).
2. If the next token is `]`: return `SelectorFilterHasAttrib` (existence check only).
3. Otherwise, read the operator:

| Token | SelectorFilterKind | CSS syntax |
|-------|--------------------|------------|
| `Equal` | `AttribEq` | `[attr=value]` |
| `TildeEq` | `AttribIncludes` | `[attr~=value]` |
| `BarEq` | `AttribDashMatch` | `[attr\|=value]` |
| `CaretEq` | `AttribStartsWith` | `[attr^=value]` |
| `DollarEq` | `AttribEndsWith` | `[attr$=value]` |
| `StarEq` | `AttribContains` | `[attr*=value]` |

4. Read the value (`Ident` or `String`).
5. Optionally read a case-sensitivity flag (`i` for case-insensitive, `s` for case-sensitive). The flag shifts the `SelectorFilterKind` into the `CaseInsensitive` or `CaseSensitive` range using arithmetic on the enum values.
6. Expect `]` to close.

### Error handling

The selector parser does not attempt recovery. On any syntax error, it:

1. Records a diagnostic message (warning in non-strict mode, error in strict mode).
2. Pushes the unexpected token back via `Unget()`.
3. Returns `null` to the caller.

The CSS property parser, which calls the selector parser when parsing CSS rules, handles recovery at a higher level — it skips forward to the next `{` or `}` to find the rule boundary and continues parsing.

Error messages are descriptive:

| Error | When |
|-------|------|
| "Missing classname after '.'" | `.` not followed by an identifier |
| "Missing pseudo-class after ':'" | `:` not followed by an identifier |
| "Missing pseudo-element after '::'" | `::` not followed by an identifier |
| "Missing attribute name after '['" | `[` not followed by an identifier |
| "Invalid operator in attribute selector" | Unexpected token after attribute name |
| "Invalid value in attribute selector" | Attribute operator not followed by identifier or string |
| "Attribute suffix must be either 'i' or 's'" | Unknown case-sensitivity flag |
| "Missing ']' at end of attribute selector" | Unclosed attribute selector |
| "Missing ')' after pseudo-class function" | Unclosed pseudo-class function |
| "Extra content at end of selector" | Content after selector when `expectEoi` is true |

---

## Selector Object Model

The parser produces instances of the following types:

### `CompoundSelector`

A comma-separated list of selectors. `IsMatch()` uses **OR logic** — an element matches the compound selector if it matches *any* of the individual selectors.

```csharp
CompoundSelector compound = CompoundSelector.Parse("h1, h2, h3");
// compound.Selectors.Count == 3
```

Each `Selector` and `CompoundSelector` holds a static `CssSelectorParser` instance for its `Parse()` and `TryParse()` convenience methods.

### `Selector`

A single selector chain, stored as an array of `SelectorComponent` in left-to-right order. The `Path` property exposes this as `IReadOnlyList<SelectorComponent>`.

```csharp
Selector selector = Selector.Parse("div.container > span.active");
// selector.Path has 2 components:
//   [0] Self        div.container
//   [1] Child       span.active
```

The first component always has `Combinator.Self`, indicating "start matching from here." Subsequent components carry the combinator that relates them to the component before them.

### `SelectorComponent`

A `readonly struct` pairing a `Combinator` with a `SimpleSelector`. This is a value type to avoid heap allocation for what is essentially a tuple.

### `Combinator`

An enum with six values:

| Value | CSS syntax | Match semantics |
|-------|------------|-----------------|
| `None` | — | Not used in practice |
| `Self` | — | Test the same element (first component, or chained selectors) |
| `Descendant` | `A B` (space) | Walk up ancestors |
| `Child` | `A > B` | Test immediate parent only |
| `AdjacentSibling` | `A + B` | Test previous element sibling only |
| `GeneralSibling` | `A ~ B` | Test all siblings |

### `SimpleSelector`

The leaf-level matching unit: an optional element name (lowercased at construction time) plus zero or more `SelectorFilter` instances. Construction lowercases the element name, so matching is always case-insensitive (consistent with the HTML parser's case-folding).

### `SelectorFilter` (abstract)

Base class for all filter types. Every filter provides:

- `Kind` — a `SelectorFilterKind` enum discriminator
- `Specificity` — the filter's contribution to specificity
- `IsMatch(Element)` — runtime match test
- `GetMatchExpression(ParameterExpression)` — LINQ expression tree for compiled matching
- `ToString(StringBuilder)` — serializes back to CSS syntax

The concrete subclasses:

| Class | Syntax | Specificity |
|-------|--------|-------------|
| `SelectorFilterId` | `#id` | 1 ID |
| `SelectorFilterClass` | `.class` | 1 attribute |
| `SelectorFilterAttrib` | `[attr=value]` etc. | 1 attribute |
| `SelectorFilterHasAttrib` | `[attr]` | 1 attribute |
| `SelectorPseudoFirstChild` | `:first-child` | 1 attribute |
| `SelectorPseudoLastChild` | `:last-child` | 1 attribute |
| `SelectorPseudoEmpty` | `:empty` | 1 attribute |
| `SelectorPseudoStyleFlag` | `:hover`, `:focus`, etc. | 1 attribute |
| `SelectorPseudoIsNot` | `:is(...)`, `:not(...)` | 1 attribute + child specificity |
| `SelectorUnknownPseudoClass` | `:custom`, `::custom` | 1 attribute |
| `SelectorFilterPseudoElement` subclasses | `::name` | 1 element |

### `Specificity`

A `readonly struct` that packs six fields into a single `ulong` (64-bit value) for fast comparison:

```
Bit 63:     Inline style flag
Bits 52–62: ID count (0–1023)
Bits 41–51: Attribute/class/pseudo-class count (0–1023)
Bits 30–40: Element/pseudo-element count (0–1023)
Bits 19–29: Stylesheet number (0–1023)
Bits 0–18:  Rule index (0–131071)
```

The bit layout is carefully designed so that **a simple integer comparison of the packed value produces the correct specificity ordering**. Inline styles always win (highest bit), then ID count, then attribute count, then element count, then source order (stylesheet number and rule index).

The `+` operator sums two specificities with overflow detection — if any field overflows its bit range, it throws a descriptive exception.

`WithoutLocation()` returns a copy with the stylesheet/rule-index fields zeroed, for comparing selectors without considering source order.

### `SelectorFilterKind`

An enum organized by ranges:

| Range | Purpose |
|-------|---------|
| `0x00–0x0F` | Basic filters (None, Id, Class) |
| `0x10–0x1E` | Attribute operators (default comparison) |
| `0x1F` | HasAttrib (existence check) |
| `0x20–0x2F` | Case-sensitive attribute operators |
| `0x30–0x3F` | Case-insensitive attribute operators |
| `0x40+` | Pseudo-classes and pseudo-elements |

The range organization allows case-sensitivity to be applied by simple arithmetic: `kind - AttribEq + CaseInsensitive` shifts from the default range to the case-insensitive range.

---

## Differences from Browser CSS Parsing

| Behavior | Browsers | Onyx |
|----------|----------|------|
| `:nth-child(an+b)` | Full An+B microsyntax | Not yet implemented; treated as unknown pseudo-class |
| `:where(...)` | Supported (zero specificity) | Not yet implemented |
| `:has(...)` | Supported (CSS Selectors Level 4) | Not yet implemented |
| Namespace selectors (`ns\|element`) | Supported | Not supported |
| Error recovery in selectors | Forgiving in some contexts | Always fails the entire selector |
| Case sensitivity of element names | Depends on document type | Always case-insensitive (matches HTML parser case-folding) |

---

## Thread Safety

`CssLexer` and `CssSelectorParser` are not thread-safe — each instance maintains mutable parsing state. However, the **parsed selector objects** (`CompoundSelector`, `Selector`, `SimpleSelector`, `SelectorFilter` subclasses) are effectively immutable after construction and safe to share across threads.

The static `CssSelectorParser` instances held by `Selector` and `CompoundSelector` for their `Parse()`/`TryParse()` convenience methods are *not* thread-safe. If selectors need to be parsed concurrently, use separate `CssSelectorParser` instances per thread.
