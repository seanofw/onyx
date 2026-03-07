# CSS Media Queries

Onyx supports CSS Media Queries Level 4, enabling stylesheets to conditionally apply
rules based on the properties of the output device.  The rationale for adding media
query support is pragmatic: real-world CSS almost always contains `@media` rules, and
being able to accept existing CSS without manual cleanup makes Onyx far more useful as
a drop-in technology.

This document covers how media queries are **parsed** — how the text of a media query
is transformed into an evaluatable expression tree — and how they are **evaluated** —
how the expression tree is executed against a concrete device context to determine
whether its rules should apply.

## Architecture overview

The media query system is structured in three layers:

1. **`CssParser`** — the top-level stylesheet parser, which recognizes `@media` blocks
   (and, in the future, `@supports` blocks) and delegates to the media query parser.
   It threads the resulting `MediaQuery` through to every `StyleRule` declared inside
   the block.
2. **`CssMediaQueryParser`** — a recursive-descent parser that consumes CSS tokens from
   a `CssLexer` and produces a tree of `MediaQuery` nodes.
3. **`MediaQuery` and its subclasses** — an expression tree data model that represents
   the parsed query in a form suitable for evaluation.

`CssParser` orchestrates four sub-parsers that share a single `Messages` collection:
`CssSelectorParser` for selectors, `CssPropertyParser` for property declarations,
`CssMediaQueryParser` for `@media` conditions, and `CssSupportsQueryParser` for
`@supports` conditions (not yet implemented).  When `CssParser` encounters an `@media`
block, it parses the media query, then recursively parses the block's contents as
nested top-level declarations, passing the media query down so that it is attached to
every `StyleRule` produced inside that block.  If `@media` blocks are nested, the
queries are combined with `MediaQueryAnd`.

`CssMediaQueryParser` follows the same patterns as Onyx's other CSS parsers: it takes
an optional shared `Messages` collection and an optional strict mode flag; it uses
`CssLexer` positions for backtracking; and it degrades gracefully on invalid input by
recording warnings and producing `MediaQueryError` nodes rather than throwing
exceptions.

```
CSS text                    CssMediaQueryParser              MediaQuery tree
─────────                   ───────────────────              ───────────────
"screen and                 ParseMediaQueryList()
 (min-width: 800px),        ├─ ParseMediaQuery()             MediaQueryOr
 print"                     │  ├─ media type                 ├─ MediaQueryAnd
                            │  └─ ParseMediaCondition()      │  ├─ MediaQueryMediaType(Screen)
                            │     └─ ParseMediaFeature()     │  └─ MediaQueryComparison(Ge, Width, 800px)
                            └─ ParseMediaQuery()             └─ MediaQueryMediaType(Print)
```

More precisely, for `screen and (min-width: 800px), print`:

```
MediaQueryOr (comma)
├── MediaQueryAnd
│   ├── MediaQueryMediaType(Screen)
│   └── MediaQueryComparison(Ge, Width, Measure(800, Px))
└── MediaQueryMediaType(Print)
```

Note how `min-width: 800px` is normalized during parsing into the comparison
`width >= 800px`.  This is one of several transformations the parser performs to
simplify the expression tree for evaluation.

## The grammar

The parser implements the CSS Media Queries Level 4 grammar.  The grammar rules are
annotated in comments throughout the parser code, but collected here for reference:

```
<media-query-list> = <media-query> [ ',' <media-query> ]*

<media-query> = <media-condition>
              | [ 'not' | 'only' ]? <media-type> [ 'and' <media-condition-without-or> ]?

<media-condition> = <media-not> | <media-in-parens> [ <media-and>* | <media-or>* ]
<media-condition-without-or> = <media-not> | <media-in-parens> <media-and>*

<media-not>  = 'not' <media-in-parens>
<media-and>  = 'and' <media-in-parens>
<media-or>   = 'or' <media-in-parens>

<media-in-parens> = '(' <media-condition> ')'
                  | '(' <media-feature> ')'
                  | <general-enclosed>

<media-feature> = <mf-plain> | <mf-boolean> | <mf-range>
<mf-plain>   = <mf-name> ':' <mf-value>
<mf-boolean> = <mf-name>
<mf-range>   = <mf-name> <mf-comparison> <mf-value>
             | <mf-value> <mf-comparison> <mf-name>
             | <mf-value> <mf-comparison> <mf-name> <mf-comparison> <mf-value>

<mf-comparison> = '<' | '>' | '<=' | '>=' | '='
<mf-value>      = <number> | <dimension> | <ident> | <ratio>
```

The key complexity of this grammar is that `<media-feature>` has three different forms
(plain, boolean, range) and that the range form supports both `name op value` and
`value op name` ordering, as well as double-ended ranges like `200px <= width <= 800px`.

## How `CssParser` handles `@media` blocks

When `CssParser.ParseOneTopLevelDeclaration()` encounters an `@` token, it reads the
following identifier to determine the at-rule type.  For `@media`, it delegates to
`ParseMediaQuery()`, which:

1. Calls `CssMediaQueryParser.ParseMediaQueryList()` to parse the media query condition.
2. If the current scope already has a media query (from an enclosing `@media` block),
   the new query is combined with it using `MediaQueryAnd` — this correctly handles
   nested `@media` blocks.
3. Expects a `{` to open the block body.
4. Recursively calls `ParseTopLevelDeclarations()` to parse the block's contents,
   passing the combined media query down.  Every `StyleRule` produced inside the block
   receives this media query in its constructor.

For `@supports`, the same pattern will apply once `CssSupportsQueryParser` is
implemented.  The stub is in place, and the intent is that `@supports` conditions will
be resolved at parse time (by attempting to parse the referenced property or selector)
and converted to `MediaQueryTrue` or `MediaQueryNotSupported`, so that the existing
media query evaluation machinery can gate their rules without any additional runtime
cost.

Unknown at-rules (e.g., `@charset`, `@import`, `@keyframes`) are treated as errors
and their contents are consumed and discarded via `CollectInvalidTokens`.

## Parsing in detail

### `ParseMediaQueryList()` — the entry point

This is the top-level entry point.  It parses a comma-separated list of media queries,
where each query is separated by a `,` token.  The comma operator represents logical
OR in CSS media queries — any one matching query makes the whole list match.

The parser handles each query independently: if one query contains a syntax error, the
parser recovers by consuming tokens until the next `,`, `;`, `{`, `}`, `]`, `)`, or
end-of-input, then continues parsing subsequent queries.  A broken query gets a
`MediaQueryError` node attached via `MediaQueryAnd(parsedSoFar, MediaQueryError)`, so
the error propagates through the tree's `HasErrors` flag without poisoning the entire
list.

Multiple queries are joined by `MediaQueryOr` nodes with `isComma: true`, which
distinguishes the comma-separated OR from the keyword `or` that appears inside
parenthesized conditions.

If no queries are present at all, the result is `MediaQueryNull.Instance` — a sentinel
that represents an empty/absent media query.

### `ParseMediaQuery()` — a single query

A single media query has two possible forms:

1. **A media condition** — a parenthesized boolean expression like `(min-width: 800px)`
   or `(color) and (hover: hover)`.

2. **A media type** with optional condition — something like `screen`,
   `not print`, `only screen and (min-width: 600px)`.

The parser tries the media condition form first (via `ParseMediaCondition`).  If that
fails, it falls back to looking for the `not` or `only` prefix keywords, then a media
type identifier, and then an optional `and <media-condition-without-or>` suffix.

The `not` keyword before a media type wraps the result in `MediaQueryNot`.  The `only`
keyword is accepted for compatibility but has no semantic effect — it exists in the CSS
spec purely to prevent older browsers from misinterpreting modern queries.

### `ParseMediaCondition()` — boolean expressions

A media condition is either:

- `not <media-in-parens>` — a negation, or
- `<media-in-parens>` optionally followed by a chain of `and` or `or` operators (but
  not both — CSS forbids mixing `and` and `or` at the same level without parentheses).

The `allowOr` parameter controls whether `or` chains are permitted.  When a media
condition appears after `and` in a media type query (e.g.,
`screen and (color) or (hover)` would be illegal), `allowOr` is set to `false`.

Chains of `and` or `or` are parsed left-associatively, building a left-leaning tree of
`MediaQueryAnd` or `MediaQueryOr` nodes.

### `ParseMediaNotAndOr()` — shared keyword+parens parsing

This is a small utility that handles the common pattern of consuming a keyword (`not`,
`and`, or `or`) followed by a `<media-in-parens>` expression.  The keyword to match is
passed as a parameter, making this one method serve three grammar rules.

### `ParseMediaInParens()` — parenthesized expressions

A parenthesized expression can contain:

1. A nested `<media-condition>` — enabling arbitrary nesting of boolean logic.
2. A `<media-feature>` — a feature test like `width: 800px`.
3. A `<general-enclosed>` — an unknown function or parenthesized expression that the
   parser doesn't recognize, which produces a `MediaQueryError` node.

The parser tries media condition first, then media feature.  If neither succeeds, it
falls through to general-enclosed handling, which collects and discards the tokens
inside the parentheses and returns `MediaQueryError`.  This graceful degradation is
required by the CSS spec: unknown features inside parentheses must not break the rest
of the query.

### `ParseMediaFeature()` — feature expressions

This is the most complex parsing rule, because CSS media features support three
different syntactic forms and the parser must also handle several transformations:

**Plain form** (`<mf-name> : <mf-value>`):  The classic CSS 2.1 syntax.  The parser
reads a feature name, expects a colon, then reads a value.  If the feature name has a
`min-` or `max-` prefix, the parser strips the prefix and converts it into a comparison:
`min-width: 800px` becomes `width >= 800px`, and `max-width: 800px` becomes
`width <= 800px`.  This normalization means the expression tree never contains min/max
features — they are always represented as comparisons.

**Boolean form** (`<mf-name>`):  Just a feature name with no operator or value.  This
tests whether the feature has a non-zero/non-none value.  The parser must know the
feature's type to produce the correct "is not zero" comparison:

- For `Measure` features (like `width`): `not (width = 0)`
- For `double` features (like `aspect-ratio`): `not (aspect-ratio = 0.0)`
- For enum features (like `hover`): `not (hover = none)`

This is what the parser's comment calls "very cursed parsing rules" — the boolean form
requires type-specific dispatch just to construct the negation.

**Range form** (`<mf-name> <op> <mf-value>` or `<mf-value> <op> <mf-name>`):  The CSS
Media Queries Level 4 range syntax.  If the feature name appears on the left, parsing
is straightforward.  If a value appears first (detected because the next token isn't an
identifier), the parser delegates to `ParseRange()`.

### `ParseRange()` — reversed and double-ended ranges

This method handles two cases that `ParseMediaFeature()` cannot:

1. **Reversed range** (`<value> <op> <name>`):  For example, `800px <= width`.  The
   parser reads the value, the comparison operator, and the feature name, then flips
   the comparison direction: `800px <= width` becomes `width >= 800px`.

2. **Double-ended range** (`<value> <op> <name> <op> <value>`):  For example,
   `200px <= width <= 800px`.  The parser validates that both comparison operators are
   compatible (both must be `<`/`<=` or both must be `>`/`>=`; you cannot mix
   directions), then decomposes the range into a pair of single comparisons joined by
   `MediaQueryAnd`:

   ```
   200px <= width <= 800px
   ```
   becomes:
   ```
   MediaQueryAnd(
       MediaQueryComparison(Ge, Width, 200px),
       MediaQueryComparison(Le, Width, 800px)
   )
   ```

   The comment in the code explains the rationale: "That's how it'll execute by the
   time it gets all the way to the bottom of the expression tree anyway, so there's no
   point in holding onto it as a first-class range."

### `ParseComparison()` — comparison operators

Parses the six comparison operators:

| Token(s)  | Result              |
|-----------|---------------------|
| `=`       | `MediaQueryKind.Eq` |
| `<`       | `MediaQueryKind.Lt` |
| `>`       | `MediaQueryKind.Gt` |
| `<=`      | `MediaQueryKind.Le` |
| `>=`      | `MediaQueryKind.Ge` |

The `<=` and `>=` operators are two tokens each (`<` followed by `=`), so the parser
must look ahead after seeing `<` or `>` to check for a trailing `=`.

### `ParseValue()` — media feature values

Values in media features can be:

- **Dimensions** (number with units): Parsed into a `Measure` object.  Unknown units
  produce a warning.
- **Plain numbers**: Parsed as `double`.
- **Ratios** (`number / number`): Parsed when a `/` follows a plain number.  Ratios are
  converted to a single `double` by dividing numerator by denominator.  Division by zero
  produces positive or negative infinity (preserving sign).
- **Identifiers**: Returned as a `string` for later enum resolution.

### `CreateMediaQueryComparison()` — type-safe comparison construction

This method bridges between the loosely-typed parsed value (`object?`) and the
strongly-typed comparison nodes.  It dispatches on the value's runtime type:

- `Measure` → `MediaQueryComparison.Create(kind, feature, measure)`
- `double` → `MediaQueryComparison.Create(kind, feature, number)`
- enum string → `MediaQueryComparison.CreateEnum(kind, feature, enumType, resolvedValue)`

If the value type doesn't match what the feature expects, the method emits a warning
and returns `MediaQueryError`.

## The enum parser

The parser uses a generic helper class, `MediaQueryEnumParser<TEnum>`, to handle both
media types and media features.  Two static instances are pre-created:

- `_mediaTypeParser` — maps CSS keywords to `MediaType` enum values
- `_mediaFeatureParser` — maps CSS keywords to `MediaFeature` enum values

Additional enum parsers are created on demand for feature value types (like
`MediaHoverKind`, `MediaPointerKind`, etc.) and cached in a `ConcurrentDictionary`.

The enum parser uses the same `Hyphenize()` trick as the CSS property parser: C# enum
names in PascalCase are automatically converted to hyphenated-lowercase CSS keywords.
For example, `MediaType.Screen` maps to `"screen"`, and `MediaFeature.MinWidth` maps to
`"min-width"`.  This means adding a new media type or feature is as simple as adding an
enum value — no string tables to maintain.

## The expression tree data model

The parser produces a tree of `MediaQuery` nodes.  `MediaQuery` is an abstract base
class with these key properties:

- **`Kind`** (`MediaQueryKind`): Identifies the node type — `And`, `Or`, `Not`,
  `MediaType`, `Feature`, `Measure`, `Number`, `Enum`, `Lt`, `Gt`, `Le`, `Ge`, `Eq`,
  `Error`, `NotSupported`, `Null`, `False`, `True`.
- **`UsesDimensions`** (`bool`): Whether this subtree references any dimension-dependent
  features (width, height, aspect-ratio, orientation).  This flag is aggregated up the
  tree and is used to split dynamic queries (which must be re-evaluated when the display
  is resized) from static queries (which depend only on fixed device properties and can
  be evaluated once).
- **`HasErrors`** (`bool`): Whether this subtree contains any `MediaQueryError` nodes,
  also aggregated up the tree.

### Node hierarchy

```
MediaQuery (abstract)
├── MediaQueryBinary (abstract) ─── Left, Right
│   ├── MediaQueryAnd .............. Kleene AND; represents "X and Y"
│   ├── MediaQueryOr ............... Kleene OR; represents "X, Y" or "X or Y"
│   └── MediaQueryComparison (abstract) ─── Feature, value
│       ├── [MeasureComparison] .... Compares Measure values (width, height)
│       ├── [DoubleComparison] ..... Compares double values (aspect-ratio)
│       └── [EnumComparison<T>] .... Compares enum values (orientation, hover)
├── MediaQueryUnary (abstract) ─── Child
│   └── MediaQueryNot .............. Kleene NOT; represents "not X"
├── MediaQueryMediaType ............ Tests media type (screen, print, all)
├── MediaQueryFeature .............. Accesses a feature value from context
├── MediaQueryMeasure .............. Leaf: holds a Measure constant
├── MediaQueryNumber ............... Leaf: holds a double constant
├── MediaQueryEnum<T> .............. Leaf: holds an enum constant
├── MediaQueryTrue ................. Singleton: unconditional true
├── MediaQueryFalse ................ Singleton: unconditional false
├── MediaQueryNull ................. Singleton: absent/unknown
├── MediaQueryError ................ Singleton: parse error occurred
└── MediaQueryNotSupported ......... Singleton: feature not supported by Onyx
```

The comparison nodes are nested private classes inside `MediaQueryComparison`, which
provides factory methods (`Create`, `CreateEnum`) and shared logic like
`FlipComparison()` for reversing operator direction.

### Three-valued (Kleene) logic

The CSS Media Queries Level 4 specification requires that media queries use three-valued
(Kleene) logic, in order to provide better forward compatibility with future versions of
the spec.  A browser that encounters an unknown media feature should not treat it as
simply `false`, because doing so would cause `not (unknown-feature)` to evaluate as
`true` — which would be incorrect if a future spec version defines that feature.
Instead, unknown features evaluate to `null` (indeterminate), and Kleene logic ensures
that the indeterminacy propagates correctly through boolean operators.

Every `Eval()` method returns `bool?`, where:

- `true` — the query matches
- `false` — the query does not match
- `null` — the result is indeterminate

The boolean operators implement Kleene's strong logic:

| Operation       | `true`  | `false` | `null`  |
|-----------------|---------|---------|---------|
| `not X`         | `false` | `true`  | `null`  |
| `X and true`    | `true`  | `false` | `null`  |
| `X and false`   | `false` | `false` | `false` |
| `X and null`    | `null`  | `false` | `null`  |
| `X or true`     | `true`  | `true`  | `true`  |
| `X or false`    | `true`  | `false` | `null`  |
| `X or null`     | `true`  | `null`  | `null`  |

This complicates the design somewhat compared to simple boolean logic, but it matches
the standard, and standards compliance is non-negotiable.

### Singleton terminals

The terminal nodes `MediaQueryTrue`, `MediaQueryFalse`, `MediaQueryNull`,
`MediaQueryError`, and `MediaQueryNotSupported` are all singletons accessed via
`Instance` properties.  This avoids unnecessary allocations for these common leaf
values.

## Supported media types

The `MediaType` enum defines:

| CSS keyword    | Enum value   | Status     |
|----------------|--------------|------------|
| `all`          | `All`        | Current    |
| `screen`       | `Screen`     | Current    |
| `print`        | `Print`      | Current    |
| `tty`          | `Tty`        | Deprecated |
| `tv`           | `Tv`         | Deprecated |
| `projection`   | `Projection` | Deprecated |
| `handheld`     | `Handheld`   | Deprecated |
| `braille`      | `Braille`    | Deprecated |
| `embossed`     | `Embossed`   | Deprecated |
| `aural`        | `Aural`      | Deprecated |
| `speech`       | `Speech`     | Deprecated |

Deprecated media types are accepted by the parser (for compatibility with existing CSS)
but will never match in practice.

## Supported media features

The `MediaFeature` enum uses bit flags to encode both the feature identity and the
min/max prefix:

```csharp
MediaFeature.Min   = 0x10000   // Flag: "min-" prefix
MediaFeature.Max   = 0x20000   // Flag: "max-" prefix
```

So `min-width` is parsed as `MediaFeature.Width | MediaFeature.Min`, and the parser
strips the prefix by masking off the flag bits.

### Dimension features (dynamic — set `UsesDimensions`)

| CSS name         | Feature        | Value type  |
|------------------|----------------|-------------|
| `width`          | `Width`        | `Measure`   |
| `height`         | `Height`       | `Measure`   |
| `aspect-ratio`   | `AspectRatio`  | `double`    |
| `orientation`    | `Orientation`  | `MediaOrientation` |

These four features are intentionally separated from the rest because they depend on
the display dimensions, which can change if the media is resized.  The `UsesDimensions`
flag, aggregated up the expression tree, allows the evaluator to distinguish queries
that depend on layout geometry (and must be re-evaluated on resize) from queries that
depend only on static device properties (and can be evaluated once).  This split between
dynamic and static data is a deliberate optimization.

### Device capability features (static)

| CSS name           | Feature          | Value type          |
|--------------------|------------------|---------------------|
| `hover`            | `Hover`          | `MediaHoverKind`    |
| `any-hover`        | `AnyHover`       | `MediaHoverKind`    |
| `pointer`          | `Pointer`        | `MediaPointerKind`  |
| `any-pointer`      | `AnyPointer`     | `MediaPointerKind`  |
| `update`           | `Update`         | `MediaUpdateMode`   |
| `overflow-block`   | `OverflowBlock`  | `MediaOverflowMode` |
| `overflow-inline`  | `OverflowInline` | `MediaOverflowMode` |

### Color features

| CSS name       | Feature      | Value type | Notes                          |
|----------------|--------------|------------|--------------------------------|
| `color`        | `Color`      | `double`   | Bits per channel if truecolor  |
| `color-index`  | `ColorIndex` | `double`   | Palette size if paletted       |
| `monochrome`   | `Monochrome` | `double`   | Bits if monochrome             |

### Not yet supported

| CSS name       | Feature      | Status              |
|----------------|--------------|---------------------|
| `resolution`   | `Resolution` | Returns null        |
| `scan`         | `Scan`       | Returns null        |
| `grid`         | `Grid`       | Always returns 0    |
| `color-gamut`  | `ColorGamut` | Returns null        |

Unsupported features do not cause parse errors; they produce `MediaQueryNotSupported`
or null values that propagate correctly through Kleene logic, ensuring that the rest of
the query still evaluates as well as it can.

## Error recovery

The parser is designed to be resilient.  Several strategies are used:

1. **Per-query recovery in lists**: If one query in a comma-separated list is broken,
   the parser consumes tokens until the next comma and continues parsing subsequent
   queries.  Only the broken query gets a `MediaQueryError`.

2. **General-enclosed fallback**: Unknown functions or parenthesized expressions that
   don't match any known grammar rule are consumed as `<general-enclosed>` and produce
   `MediaQueryError` nodes.  This is required by the spec for forward compatibility.

3. **Unknown features**: A feature name that isn't recognized is consumed and recorded
   as `MediaFeature.Unknown`.  The resulting comparison will evaluate to null/false
   rather than crashing.

4. **Invalid min/max in ranges**: Using a min/max-prefixed feature name in a range
   expression (e.g., `min-width >= 800px`) is warned about and the feature is set to
   `Unknown`, but parsing continues.

5. **Error propagation**: The `HasErrors` flag propagates up the tree, so callers can
   detect whether any part of a query had problems, but the overall query structure
   remains intact.

## Walked example

Consider the media query:

```css
@media screen and (min-width: 768px) and (orientation: landscape), print
```

### Step 1: ParseMediaQueryList

The parser enters `ParseMediaQueryList`, which will parse comma-separated queries.

### Step 2: First query — `screen and (min-width: 768px) and (orientation: landscape)`

`ParseMediaQuery` is called.  It first tries `ParseMediaCondition`, which fails (the
input doesn't start with `(` or `not`).  So it falls back to media-type parsing:

1. No `not` or `only` prefix is found.
2. `screen` is consumed and matched to `MediaType.Screen` → `MediaQueryMediaType(Screen)`.
3. The keyword `and` is found, so `ParseMediaCondition(allowOr: false)` is called.

### Step 3: ParseMediaCondition for `(min-width: 768px) and (orientation: landscape)`

This calls `ParseMediaInParens`, which finds `(`, then tries `ParseMediaFeature`.

### Step 4: ParseMediaFeature for `min-width: 768px`

1. `min-width` is recognized as `MediaFeature.Width | MediaFeature.Min`.
2. A `:` is found → plain form.
3. The `Min` flag is detected, stripped, and `kind` is set to `Ge`.
4. `ParseValue` reads `768px` → `Measure(768, Px)`.
5. Result: `MediaQueryComparison(Ge, Width, Measure(768, Px))` — i.e., `width >= 768px`.

### Step 5: Back in ParseMediaCondition

The closing `)` is consumed.  The parser finds `and`, so it enters the `and` loop:

1. `ParseMediaNotAndOr("and")` consumes `and` and calls `ParseMediaInParens`.
2. Inside the parentheses: `ParseMediaFeature` handles `orientation: landscape`.
3. `orientation` is recognized as `MediaFeature.Orientation`.
4. A `:` is found → plain form.  No min/max prefix, so `kind` is `Eq`.
5. `ParseValue` reads `landscape` → string `"landscape"`.
6. `CreateMediaQueryComparison` resolves the string against `MediaOrientation` →
   `MediaQueryComparison.CreateEnum(Eq, Orientation, MediaOrientation.Landscape)`.

The `and` loop produces:
```
MediaQueryAnd(
    MediaQueryComparison(Ge, Width, Measure(768, Px)),
    MediaQueryComparison(Eq, Orientation, Landscape)
)
```

### Step 6: Back in ParseMediaQuery

The condition is joined to the media type:
```
MediaQueryAnd(
    MediaQueryMediaType(Screen),
    MediaQueryAnd(
        MediaQueryComparison(Ge, Width, Measure(768, Px)),
        MediaQueryComparison(Eq, Orientation, Landscape)
    )
)
```

### Step 7: Second query — `print`

After the comma, `ParseMediaQuery` is called again.  `ParseMediaCondition` fails,
no prefix keyword is found, and `print` matches `MediaType.Print`:
```
MediaQueryMediaType(Print)
```

### Step 8: Final tree

```
MediaQueryOr (comma)
├── MediaQueryAnd
│   ├── MediaQueryMediaType(Screen)
│   └── MediaQueryAnd
│       ├── MediaQueryComparison(Ge, Width, Measure(768, Px))
│       └── MediaQueryComparison(Eq, Orientation, Landscape)
└── MediaQueryMediaType(Print)
```

This tree reads as: "matches if the media type is screen AND the width is at least
768px AND the orientation is landscape; OR if the media type is print."

---

## Media query evaluation

Once the parser has produced a `MediaQuery` expression tree, the tree must be
**evaluated** against a concrete device context to determine whether it matches.
Evaluation is performed by two parallel mechanisms: an interpreted `Eval()` method for
direct use, and a compiled `GetEval()` method that JIT-compiles the expression tree
into a native delegate for repeated evaluation.

### The evaluation context

Evaluation requires a `MediaQueryContext`, a readonly struct that bundles together the
two halves of the device description:

```
MediaQueryContext
├── MediaDimensions ─── dynamic (may change on resize)
│   ├── Width ......... Measure
│   ├── Height ........ Measure
│   ├── AspectRatio ... double (computed: Width / Height)
│   └── Orientation ... MediaOrientation (computed: Landscape if aspect > 1)
│
└── MediaInfo ──────── static (set once at startup)
    ├── Type ........... MediaType (Screen, Print, All, ...)
    ├── UpdateMode ..... MediaUpdateMode (None, Slow, Fast)
    ├── ColorMode ...... MediaColorMode (Truecolor, Paletted, Monochrome)
    ├── ColorDepth ..... ushort (bits per channel, palette size, or brightness bits)
    ├── OverflowBlock .. MediaOverflowMode (None, Scroll, Paged)
    ├── OverflowInline . MediaOverflowMode
    ├── PointerKind .... MediaPointerKind (None, Coarse, Fine)
    ├── HoverKind ...... MediaHoverKind (None, Hover)
    ├── Color .......... int (computed: ColorDepth if Truecolor, else 0)
    ├── Monochrome ..... int (computed: ColorDepth if Monochrome, else 0)
    └── ColorIndex ..... int (computed: ColorDepth if Paletted, else 0)
```

This two-struct split is the physical manifestation of the `UsesDimensions` design:
`MediaDimensions` holds the values that can change at runtime (because the display can
be resized), while `MediaInfo` holds the values that are fixed for the lifetime of the
document (the device's capabilities).  The `MediaQueryContext` combines both into a
single value that is passed to evaluators.

### How `Document` provides the context

The `Document` class implements the internal `IStyleRoot` interface, which requires
three things: a `StyleManager`, a `StyleQueue`, and a `MediaQueryContext`.  Document
exposes `MediaInfo` and `MediaDimensions` as settable properties, and constructs the
`MediaQueryContext` on demand from these two values:

```csharp
MediaQueryContext IStyleRoot.MediaQueryContext
    => new MediaQueryContext(MediaDimensions, MediaInfo);
```

Both properties have change detection: when either `MediaInfo` or `MediaDimensions` is
set to a new value, the setter calls `InvalidateChildComputedStyles()`, which
invalidates all computed styles in the entire tree and enqueues every element for
restyling.  Currently this invalidates every element, but the design is intended to
support a future optimization: because each `MediaQuery` node carries a
`UsesDimensions` flag, and each `StyleRule` carries its `MediaQuery`, it is possible
to track which elements depend on dimension-sensitive media queries and invalidate only
those elements when `MediaDimensions` changes.  This selective invalidation is not yet
implemented, but the data model already supports it, and it can be a substantial
performance win for window resizing — one of Onyx's primary use cases is desktop UIs,
where re-layout after a window resize is a frequent and important operation.  Changes
to `MediaInfo` (which is expected to be set once at startup and never changed) would
continue to invalidate the entire tree.

The `IStyleRoot` interface itself is internal, keeping the media query plumbing hidden
from end users.  Its definition is minimal:

```csharp
internal interface IStyleRoot
{
    IStyleManager StyleManager { get; }
    IStyleQueue StyleQueue { get; }
    MediaQueryContext MediaQueryContext { get; }
}
```

### How `StyleManager` uses media queries

When `Element.GetComputedStyle()` needs to compute an element's style, it delegates to
`StyleManager.ComputeStyle()`, passing along the `MediaQueryContext` obtained from the
`IStyleRoot`.  Inside `ComputeStyle`, the call chain is:

1. `GetStyleRules(element, context)` — finds all matching style rules.
2. For each candidate `StyleRule`, if `rule.MediaQuery` is not null, the media query is
   evaluated against the context.
3. Only rules whose media query evaluates to `true` are included.

The key code in `GetStyleRules`:

```csharp
if (rule.MediaQuery != null)
{
    Func<MediaQueryContext, bool?> mediaQueryEval = rule.MediaQuery.GetEval();
    bool? isMediaQuerySatisfied = mediaQueryEval(context);
    if (isMediaQuerySatisfied != true)
        continue;
}
```

This is where the three-valued logic has its final say: only `true` passes the gate.
Both `false` and `null` (indeterminate) cause the rule to be skipped.  This is the
correct interpretation per the CSS spec — an indeterminate media query must not cause
its rules to apply.

Note that `GetEval()` is used rather than `Eval()`: the compiled delegate is cached on
the `MediaQuery` node, so after the first evaluation, subsequent calls execute JIT-
compiled native code rather than interpreting the tree.

### The `StyleRule` object

Each `StyleRule` carries an optional `MediaQuery?` alongside its `CompoundSelector` and
`StylePropertySet`.  When the CSS parser encounters an `@media` block, it attaches the
parsed media query to every rule inside that block.  Rules outside `@media` blocks have
a null `MediaQuery`, meaning they always apply.  This keeps the filtering logic simple:
a null media query is treated as unconditionally true.

### Dual evaluation: interpreted and compiled

Every `MediaQuery` node implements two evaluation methods:

**`Eval(MediaQueryContext)`** — direct interpretation.  Each node type implements this
as a simple virtual method call.  For example, `MediaQueryAnd.Eval` calls
`KleeneAnd(Left.Eval(context), Right.Eval(context))`.  This is straightforward but
involves virtual dispatch at every node in the tree.

**`GetExpression(ParameterExpression)`** — expression tree generation.  Each node type
produces a `System.Linq.Expressions.Expression` that represents the same computation
but as a data structure that the .NET runtime can compile into a native delegate.  The
base class's `GetEval()` method compiles the expression tree once and caches the result:

```csharp
public Func<MediaQueryContext, bool?> GetEval()
{
    if (_eval != null)
        return _eval;

    ParameterExpression param = Expression.Parameter(typeof(MediaQueryContext), "x");
    Expression body = GetExpression(param);
    var evalExpr = Expression.Lambda<Func<MediaQueryContext, bool?>>(body, param);

    _eval = evalExpr.Compile();
    return _eval;
}
```

The compiled delegate eliminates all virtual dispatch, all boxing, and all tree
traversal overhead.  For a media query that is evaluated against every candidate style
rule on every element in the tree, this is a significant optimization.

### How each node type evaluates

**`MediaQueryMediaType`**: Compares `context.MediaInfo.Type` against the stored
`MediaType` enum value.  Always returns `true` or `false`, never null.

**`MediaQueryFeature`**: Accesses a feature value from the context via a switch on the
`MediaFeature` enum, then resolves it to a boolean: non-zero `Measure` or `double`
values are true; zero is false; null (unsupported) is null.  The expression tree
version generates property access chains like
`Expression.MakeMemberAccess(param, _mediaDimensions, _mediaDimensions_Width)`.

**`MediaQueryComparison`** (three variants):

- *Measure comparison*: Calls `Measure.TryConvert()` to convert both sides to the same
  units, then calls `CompareTo()`.  If unit conversion fails (incompatible units), the
  result is null.  The expression tree version calls a static `ConvertAndCompare` method
  to handle the conversion and comparison in one step.

- *Double comparison*: Calls `double.CompareTo()` directly.  Always returns a definite
  result.

- *Enum comparison*: Uses `object.Equals()` for equality testing.  Only the `=`
  operator is supported for enum comparisons; inequality operators produce
  `MediaQueryError` at parse time.

All three comparison types support the `IsFlipped` flag for reversed operand order
(`value op name` → `name flippedOp value`).  At evaluation time, flipping is
implemented by negating the `CompareTo` result.

**`MediaQueryAnd`**: Calls `KleeneAnd(left, right)` — returns false if either operand
is false, true if both are true, null otherwise.  Marked with `AggressiveInlining` and
`AggressiveOptimization` for hot-path performance.

**`MediaQueryOr`**: Calls `KleeneOr(left, right)` — returns true if either operand is
true, false if both are false, null otherwise.  Same optimization attributes.

**`MediaQueryNot`**: Calls `KleeneNot(child)` — flips true/false, passes null through.

**Terminal singletons**:

| Node                    | `Eval()` returns | Meaning                    |
|-------------------------|------------------|----------------------------|
| `MediaQueryTrue`        | `true`           | Unconditional match        |
| `MediaQueryFalse`       | `false`          | Unconditional non-match    |
| `MediaQueryNull`        | `null`           | Absent/empty query         |
| `MediaQueryError`       | `null`           | Parse error occurred       |
| `MediaQueryNotSupported`| `false`          | Feature not supported      |

Note that `MediaQueryError` evaluates to `null` (not `false`), so that `not error`
does not accidentally evaluate to `true`.  `MediaQueryNotSupported` evaluates to
`false` because it represents a known feature that Onyx simply does not implement —
its behavior is defined, just not available.

### Expression tree mechanics

The expression tree generation deserves a closer look, because it is doing something
quite clever: it is building a specialized native function at runtime, tailored to the
specific structure of each media query.

For a simple query like `(width >= 768px)`, the generated expression tree is
approximately:

```csharp
(MediaQueryContext x) => {
    int? nullableValue = ConvertAndCompare(x.MediaDimensions.Width, Measure(768, Px));
    return nullableValue.HasValue ? (bool?)(nullableValue.Value >= 0) : (bool?)null;
}
```

For a compound query like `screen and (width >= 768px)`, the tree composes:

```csharp
(MediaQueryContext x) =>
    KleeneAnd(
        (bool?)(x.MediaInfo.Type == MediaType.Screen),
        /* the width comparison block from above */
    )
```

The `GetExpression` method on each node returns an `Expression` fragment, and the
parent node combines fragments using `Expression.Call` to the Kleene logic methods.
The base class's `GetEval()` wraps the entire composed expression in a lambda and
compiles it with `Expression.Lambda<...>().Compile()`.

The reflection `PropertyInfo` objects used to generate property access expressions are
cached as static fields on the `MediaQuery` base class, so they are resolved once at
class load time, not on every expression tree construction.

### Invalidation and the style pipeline

Media query evaluation is embedded in the style computation pipeline at a specific
point: after candidate rules have been found by the selector indexes, but before
selector matching and specificity resolution.  This means that media queries act as a
**pre-filter** on style rules:

```
FindCandidateRules(element)     ← index lookup, fast
    │
    ▼
for each candidate rule:
    ├─ if rule.MediaQuery != null:
    │      evaluate media query   ← compiled delegate, fast
    │      skip if not true
    │
    ├─ for each selector:
    │      IsMatch(element)       ← full selector matching
    │      track highest specificity
    │
    └─ if matched:
           add to results with specificity
```

When `MediaDimensions` or `MediaInfo` changes on the `Document`, the setters
invalidate all computed styles in the tree.  This is appropriate because media query
results are not cached per-rule — they are evaluated fresh each time `GetStyleRules`
is called.  The compiled delegate makes this fast enough that re-evaluation on every
style computation is not a performance concern.

### Putting it all together

A typical Onyx application sets up its media context once at startup:

```csharp
document.MediaInfo = new MediaInfo(
    type: MediaType.Screen,
    updateMode: MediaUpdateMode.Fast,
    colorMode: MediaColorMode.Truecolor,
    colorDepth: 8,
    overflowBlock: MediaOverflowMode.Scroll,
    pointerKind: MediaPointerKind.Fine,
    hoverKind: MediaHoverKind.Hover
);

document.MediaDimensions = new MediaDimensions(
    new Measure(Units.Px, 1920),
    new Measure(Units.Px, 1080)
);
```

From this point on, any `@media` rules in the document's stylesheets will be
automatically evaluated during style computation.  If the application window is
resized, updating `MediaDimensions` invalidates all styles and triggers recomputation.
In the future, the `UsesDimensions` flag will allow Onyx to narrow this invalidation
to only those elements whose styles depend on dimension-sensitive media queries,
avoiding unnecessary restyling of elements governed by static-only queries.
