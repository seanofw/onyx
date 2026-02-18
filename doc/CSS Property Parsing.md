# CSS Property Parsing

## Overview

Onyx's CSS property parser is responsible for taking a property declaration like `margin: 10px 20px auto` and producing a strongly-typed `StyleProperty` object that can later be applied to a computed style. Unlike the selector parser (documented in [CSS Selector Parsing.md](CSS%20Selector%20Parsing.md)), which is a monolithic recursive-descent parser, the property parser is built on a **composable mini-parser DSL** — a small combinator library that lets each CSS property's syntax be declared as a data structure rather than written as imperative parsing code.

The key insight is that CSS property grammars are highly regular: most properties are combinations of a small number of value types (colors, lengths, enums, URIs) arranged using a small number of structural patterns (sequences, alternatives, repetition, any-order groups). Rather than writing a custom parser for each of 100+ CSS properties, Onyx defines a library of reusable parsing primitives and combinators, then declares each property's syntax as a composition of those primitives.

### Files involved

| File | Role |
|------|------|
| `Css/Parsing/CssPropertyParser.cs` | The top-level entry point that reads `name: value` and dispatches to the mini-parser |
| `Css/Properties/PropertySyntaxDefinitions.Pieces.cs` | Reusable sub-parsers for common value patterns (e.g., shadow, border edge, font family) |
| `Css/Properties/PropertySyntaxDefinitions.Table.cs` | The master table mapping `KnownPropertyKind` → `MiniParser` for every supported property |
| `Css/Properties/SyntaxBuilder.cs` | The fluent API that constructs `Syntax<TProp>` nodes from lambda expressions |
| `Css/Properties/MiniParser.cs` | Pairs a `Syntax` tree with a factory function to create the target property object |
| `Css/Properties/SyntaxDefinitions/Syntax.cs` | The abstract base class for all syntax nodes |
| `Css/Properties/SyntaxDefinitions/*.cs` | ~30 concrete syntax node types (one per combinator or value type) |
| `Css/Properties/StyleProperty.cs` | The abstract base `record class` for all parsed property values |
| `Css/Properties/UnknownProperty.cs` | Fallback for properties that can't be parsed |

---

## Architecture

### The three layers

The property parser has three layers:

1. **CssPropertyParser** (top level) — reads the property name, looks up the syntax definition, handles `inherit`/`initial`/`unset` and `!important`, and delegates value parsing to the syntax tree.
2. **PropertySyntaxDefinitions** (declaration layer) — declares every CSS property's grammar as a composition of syntax primitives, using a fluent builder DSL.
3. **Syntax nodes** (execution layer) — a tree of composable parser objects, each of which knows how to parse one piece of a property value and transform the result into a strongly-typed property object.

### Data flow

```
CSS text: "margin: 10px 20px auto"
    │
    ▼
CssPropertyParser.ParseStyleProperty()
    │
    ├─ Reads "margin" → looks up KnownPropertyKind.Margin
    ├─ Reads ":"
    ├─ Looks up PropertySyntaxDefinitions.Syntaxes[KnownPropertyKind.Margin]
    │      → gets a MiniParser wrapping a Syntax tree
    ├─ Creates a new MarginProperty via MiniParser.MakeNew()
    ├─ Calls syntax.OuterParse(lexer, property)
    │      → the syntax tree recursively parses "10px 20px auto"
    │      → each parsed value is folded into the property via lambda callbacks
    ├─ Checks for "!important"
    │
    ▼
MarginProperty { Widths = [10px, 20px, auto] }
```

---

## CssPropertyParser: the top-level driver

`CssPropertyParser.ParseStyleProperty()` (`Css/Parsing/CssPropertyParser.cs`) is the single entry point for parsing a property declaration. It handles the structural framing around the value itself:

### Step by step

1. **Read the property name.** Consume an `Ident` token. Look it up in `StyleProperty.PropertyKindLookup` — a dictionary that maps CSS names like `"margin-top"` to `KnownPropertyKind.MarginTop`. The lookup table is built automatically from the `KnownPropertyKind` enum by converting PascalCase names to hyphenated-lowercase via the `Hyphenize()` extension method.

2. **Read the colon.** Consume the `:` delimiter. If missing, report an error and rewind.

3. **Look up the syntax definition.** Check `PropertySyntaxDefinitions.Syntaxes` for this `KnownPropertyKind`. If no syntax is registered (the property is unknown), collect the remaining tokens as an `UnknownProperty` and return.

4. **Check for global keywords.** If the next token is `inherit`, `initial`, or `unset`, skip the real parser and return the property with the appropriate flag set. These CSS-wide values are handled identically for every property, so they're handled once here rather than in each syntax definition. The three keywords are distinguished by a fast character test (`text[2]`): `'h'` for in**h**erit, `'i'` for in**i**tial, else unset.

5. **Parse the value.** Call `miniParser.Syntax.OuterParse(lexer, styleProperty)`. This invokes the syntax tree, which consumes tokens from the lexer and returns a new property object with the parsed values folded in. If parsing fails (returns `null`), rewind to the start of the value and collect the tokens as an `UnknownProperty`.

6. **Check for `!important`.** After the value, look for `!` followed by `important`. If found, set the `Important` flag on the property.

### Error recovery

When parsing fails — either because the property name is unknown or because the syntax tree rejects the value — the parser uses `CollectInvalidTokens()` to consume tokens up to the next `;`, `}`, or matching close bracket. This method respects nesting: if it encounters `(`, `[`, or `{`, it recursively consumes until the matching close delimiter. This follows the CSS specification's error recovery rules, which require unknown content to be skipped without corrupting the parse of surrounding declarations.

---

## The Syntax DSL

### MiniParser: the glue

A `MiniParser` pairs two things:

- A **`Syntax`** tree — a composed parser that knows how to parse a specific grammar from a token stream.
- A **`MakeNew`** factory — a `Func<object>` that creates a fresh, empty instance of the target property class (e.g., `() => new MarginProperty()`).

The generic `MiniParser<TProp>` provides type-safe access to both.

### Syntax nodes

`Syntax<TProp>` is the abstract base class for all parser nodes. Its core method is:

```csharp
public virtual TProp? Parse(CssLexer lexer, TProp property) => null;
```

Each syntax node:
- Receives the current lexer position and the current state of the property being built.
- Attempts to parse its expected grammar from the token stream.
- On success, returns a new property object with the parsed value incorporated (typically using `record class` `with` expressions).
- On failure, returns `null` and leaves the lexer at or near the position where parsing started (most nodes save and restore position via `lexer.Here()` / `lexer.Rewind()`).

The property object is **threaded through** the entire parse: each node receives the property as it currently exists, applies its own transformation, and passes the updated property to the next node. This is a functional-style fold over the token stream.

### SyntaxBuilder: the fluent API

`SyntaxBuilder<TProp>` is a zero-size `readonly struct` that serves as a namespace for constructing syntax nodes. It's the `x` parameter in syntax definitions like:

```csharp
DefineSyntax<DisplayProperty>(x =>
    x.Enum<DisplayKind>((p, d) => p with { Display = d })
)
```

The builder provides methods in two categories:

#### Value primitives

These parse a single CSS value type from the token stream:

| Method | Parses | Callback receives |
|--------|--------|-------------------|
| `Color(...)` | CSS color (`#rgb`, `rgb()`, named colors) | `Color32` |
| `Length(...)` | Length with units (`10px`, `2em`) | `Measure` |
| `LengthOrPercent(...)` | Length or percentage (`10px`, `50%`) | `Measure` |
| `Angle(...)` | Angle (`45deg`, `1rad`) | `Measure` |
| `Time(...)` | Duration (`300ms`, `1s`) | `Measure` |
| `Frequency(...)` | Frequency (`440hz`) | `Measure` |
| `Number(...)` | Bare number (`1.5`) | `double` |
| `Double(...)` | Bare number (alias) | `double` |
| `Integer(...)` | Integer (`3`) | `int` |
| `String(...)` | Quoted string (`"hello"`) | `string` |
| `Ident(...)` | Identifier (`foo`) | `string` |
| `IdentSequence(...)` | Whitespace-separated identifiers (`Segoe UI`) | `string` (joined) |
| `Uri(...)` | `url(...)` | `string` |
| `Keyword(name, ...)` | Specific keyword (`none`, `auto`) | `string` |
| `KeywordMulti(names, ...)` | One of several keywords | `string` |
| `Enum<TEnum>(...)` | Keyword mapped to a C# enum value | `TEnum` |
| `Rect(...)` | `rect(top, right, bottom, left)` | `CssRect` |
| `Counter(...)` | `counter(name, style?)` | name + style |
| `Counters(...)` | `counters(name, sep, style?)` | name + sep + style |
| `Attr(...)` | `attr(name)` | `string` |
| `BorderWidth(...)` | `thin` / `medium` / `thick` / length | `Measure` |
| `BackgroundPosition(...)` | Complex background position grammar | `BackgroundPosition` |
| `Punct(kind)` | Specific punctuation token (`,`, `/`, etc.) | `CssToken` |

#### Combinators

These compose other syntax nodes into larger grammars:

| Method | CSS notation | Semantics |
|--------|-------------|-----------|
| `Sequence(a, b, c)` | `a b c` | All must match, in order |
| `OneOf(a, b, c)` | `a \| b \| c` | First match wins (ordered alternatives) |
| `AnyOrder(a, b, c)` | `a \|\| b \|\| c` | All may match, in any order (CSS double-bar) |
| `Optional(a)` | `a?` | Matches zero or one time |
| `Range(min, max, a)` | `a{min,max}` | Matches between min and max times |
| `OneOrMoreOf(a)` | `a+` | Matches one or more times |
| `ZeroOrMoreOf(a)` | `a*` | Matches zero or more times |
| `OneOrMoreWithCommas(a)` | `a#` | One or more, comma-separated |
| `ZeroOrMoreWithCommas(a)` | `a#?` | Zero or more, comma-separated |
| `RequiredThenOptional(a, b)` | `a b?` | First is required, second is optional |
| `Derive(childSyntax, extract, apply)` | *(type change)* | Delegates to a child syntax of a different type |

### The Enum trick

One of the most elegant pieces of the DSL is `EnumSyntax`. Given a C# enum like:

```csharp
enum DisplayKind { Inline, Block, ListItem, InlineBlock, ... }
```

`EnumSyntax` automatically builds a lookup table by converting each enum name from PascalCase to CSS's hyphenated-lowercase (via `Hyphenize()`): `ListItem` → `"list-item"`, `InlineBlock` → `"inline-block"`. At parse time, it reads an `Ident` token and looks it up in this table. This means adding support for a new CSS keyword is often as simple as adding a new value to an existing C# enum — no parsing code changes required.

### The Derive combinator

`DerivedSyntax` is the key to handling **shorthand properties** — properties like `font` or `background` that combine multiple sub-properties into a single declaration. It works by bridging between two different property types:

```csharp
x.Derive(
    _fontSize.Syntax,                                    // child parser (Syntax<FontSizeProperty>)
    p => p.FontSize ?? FontSizeProperty.Default,         // extract child from parent
    (p, s) => p with { FontSize = s }                    // apply child back to parent
)
```

When parsing, `DerivedSyntax`:
1. Extracts the current child property from the parent (or a default).
2. Delegates parsing to the child syntax.
3. If successful, applies the parsed child back to the parent.

This allows the `font` property's syntax to reuse the `_fontSize`, `_fontFamily`, `_fontWeight`, etc. syntax definitions directly, composing them with `AnyOrder` and `Sequence` to match the CSS `font` shorthand grammar. The sub-property parsers don't need to know they're being used inside a shorthand — they parse and return their own type, and `Derive` handles the mapping.

---

## How combinators work internally

### OneOf (ordered alternatives)

`OneOfSyntax` tries each child syntax in order, saving the lexer position before each attempt. The first one that returns non-null wins. If all children fail, the lexer is rewound to the original position.

```
OneOf(A, B, C):
    save position
    try A → if success, return
    rewind, try B → if success, return
    rewind, try C → if success, return
    return null (or property if AllowNone)
```

### AnyOrder (CSS double-bar `||`)

`AnyOrderSyntax` implements the CSS "double bar" combinator, where components may appear in any order and each may appear at most once. It uses a bitmask to track which children have already matched:

```
AnyOrder(A, B, C):
    loop:
        for each unmatched child:
            save position
            try child → if success, mark as matched, break inner loop
            rewind
        until no child matches in a full pass
    if nothing matched at all → return null
    else return accumulated property
```

This means `border: 1px solid red` matches the same grammar as `border: red solid 1px` or `border: solid red 1px` — the width, style, and color can appear in any order.

### Sequence

`SequenceSyntax` requires all children to match in strict order. If any child fails, the entire sequence fails. The property object is threaded through each child in turn.

### Range

`RangeSyntax` matches a child syntax between `min` and `max` times. Each successful match updates the property object (typically calling an `Add` method to accumulate values into a list). CSS uses this for things like `margin: 10px 20px` (1 to 4 values) where the number of values determines which edges are specified.

### RepeatComma

`RepeatCommaSyntax` matches a child syntax one or more times (or zero or more, if optional), separated by commas. This handles CSS's `#` multiplier notation, used in properties like `background-image` and `box-shadow` where multiple layers are comma-separated.

---

## PropertySyntaxDefinitions: the declaration tables

### Pieces file

`PropertySyntaxDefinitions.Pieces.cs` defines reusable `MiniParser` instances for value patterns that appear in multiple properties:

- **`_backgroundColor`**, **`_backgroundImage`**, **`_backgroundPosition`**, etc. — individual background layer sub-properties, reused by both their standalone properties and the `background` shorthand.
- **`MakeBorderEdgeProperty<T>()`**, **`MakeBorderEdgeColorProperty<T>()`**, etc. — generic factory methods that produce the same syntax for `border-top`, `border-right`, `border-bottom`, and `border-left` (and their color/style/width variants).
- **`MakeWidthProperty<T>()`** — shared syntax for `<length> | <percentage> | auto`, reused by `top`, `right`, `bottom`, `left`, `width`, `height`, `text-indent`, and all the margin and padding edges.
- **`_shadow`** — the complex `<color>? && <length>{2} [<length> <length>?]? && inset?` grammar for `box-shadow` and `text-shadow`.
- **`_fontFamily`**, **`_fontSize`**, **`_fontStyle`**, **`_fontWeight`**, **`_lineHeight`** — sub-properties reused by both their standalone properties and the `font` shorthand.

Each piece is defined using `DefineSyntax<TProp>(x => ...)`, which constructs a `SyntaxBuilder<TProp>`, passes it to the builder lambda, and wraps the result in a `MiniParser<TProp>`.

### Table file

`PropertySyntaxDefinitions.Table.cs` contains the static constructor that populates `Syntaxes` — the master `Dictionary<KnownPropertyKind, MiniParser>` that maps every known CSS property to its parser. This is where all the pieces come together. The dictionary currently has entries for 90+ properties, covering CSS 2.1 and selected CSS 3 properties (flexbox, border-radius, box-shadow, text-shadow, background layers, etc.).

Properties deprecated after CSS 2.1 (aural properties like `cue`, `pause`, `pitch`, `speak`, etc.) are explicitly commented out as omitted.

### Example: a simple property

```csharp
{ KnownPropertyKind.Display,
    DefineSyntax<DisplayProperty>(x =>
        x.Enum<DisplayKind>((p, d) => p with { Display = d })
    )
}
```

This says: parse a CSS identifier, look it up in the `DisplayKind` enum (where `InlineBlock` maps to `"inline-block"`, etc.), and if found, return a new `DisplayProperty` with the `Display` field set.

### Example: a shorthand property

```csharp
{ KnownPropertyKind.Border,
    DefineSyntax<BorderProperty>(x =>
        x.AnyOrder(
            x.BorderWidth((p, w) => p with { BorderWidth = w }),
            x.Enum<BorderStyle>((p, s) => p with { BorderStyle = s }),
            x.Color((p, c) => p with { BorderColor = c })
        )
    )
}
```

This says: parse up to three values in any order — a border width (keyword or length), a border style (enum), and a color. Each is optional, but at least one must be present. This matches `border: 1px solid red` and `border: solid` and `border: red 2px` equally.

### Example: a complex shorthand with sub-property reuse

The `font` shorthand is one of the most complex:

```csharp
x.OneOf(
    x.Sequence(
        x.Optional(
            x.AnyOrder(
                x.Derive(_fontStyle.Syntax, ...),
                x.Derive(_fontVariant.Syntax, ...),
                x.Derive(_fontWeight.Syntax, ...)
            )
        ),
        x.Derive(_fontSize.Syntax, ...),
        x.Optional(
            x.Sequence(
                x.Punct(CssTokenKind.Slash),
                x.Derive(_lineHeight.Syntax, ...)
            )
        ),
        x.Derive(_fontFamily.Syntax, ...)
    ),
    x.Derive(DefineSyntax<SpecialFontProperty>(...).Syntax, ...)
)
```

This reads as: either parse the full `font` syntax (optional style/variant/weight in any order, then required font-size, then optional `/line-height`, then required font-family), or parse a system font keyword (`caption`, `icon`, `menu`, etc.). Each sub-property is parsed using its own standalone syntax definition via `Derive`, so the parsing logic is defined only once.

---

## StyleProperty: the output

Every parsed property is a subclass of `StyleProperty`, which is an abstract `record class`. The `record` keyword is important — it provides `with` expressions, which are how the syntax nodes produce updated property objects without mutation:

```csharp
(p, d) => p with { Display = d }
```

This creates a copy of `p` with only the `Display` field changed. Because syntax nodes receive and return property objects through a functional fold, the `with` expression is the natural mechanism for threading state through the parse.

### Common flags

`StyleProperty` carries several flags in a packed `StylePropertyFlags` field:

- **`Inherit`** — the value is `inherit` (copy from parent)
- **`Initial`** — the value is `initial` (use the property's initial/default value)
- **`Unset`** — the value is `unset` (use inherit for inherited properties, initial otherwise)
- **`Important`** — the declaration has `!important`
- **`Valid`** — the property was successfully parsed

These are checked by `HasSpecialApplication` — if any of `Inherit`, `Initial`, or `Unset` is set, the property value itself is irrelevant; the style system will handle it specially during computed style calculation.

### UnknownProperty

When parsing fails — either because the property name isn't recognized or because the syntax tree rejects the value — the result is an `UnknownProperty`. This stores the raw tokens so they can be round-tripped or inspected, but its `Apply()` method is a no-op: unknown properties don't affect computed styles.

---

## Design philosophy

### Correctness first

The CSS specification is notoriously complex, with numerous dangerous corner cases in its property grammars — ambiguous value types, order-independent shorthands, context-sensitive defaults, and interactions between sub-properties that are easy to get wrong. The property parser was designed with **correctness as the primary goal**, above all other considerations.

The key strategy for achieving correctness is making the code as directly comparable to the formal CSS specification as possible. The syntax definitions in `PropertySyntaxDefinitions` are effectively a PEG (parsing expression grammar), and they are written to be a close visual match to the grammar notation in the CSS spec. A developer can place the code and the spec side by side and verify that they agree. This is intentional: with a grammar as complex as CSS, correctness means being able to directly compare the rules in the code to the rules in the formal specification.

It is likely possible that a hand-written CSS property parser would run somewhat faster. But it would be orders of magnitude more complex, and likely much more buggy and harder to prove correct — both of which are bad safety tradeoffs for what is probably only a small gain in speed. CSS parsing typically only runs at application startup anyway, so the performance of parsing is far less critical than the performance of selector matching or style computation, which run repeatedly at runtime.

### Declarative over imperative

The most striking aspect of this system is that **almost no property has hand-written parsing code**. The grammar of each property is declared as a data structure built from composable pieces, and the parsing logic is entirely generic. The declarative approach costs some small startup time (constructing the syntax trees and their lookup tables), but provides enormous gains:

- **Consistency**: Every property follows the same error-recovery and backtracking rules, because they all go through the same combinator machinery.
- **Correctness**: CSS grammar rules like "double-bar means any order" are implemented once in `AnyOrderSyntax` and automatically correct for every property that uses `AnyOrder`.
- **Conciseness**: Adding a new CSS property is typically a few lines in the syntax table — declare the type, connect the combinators, provide the lambdas.
- **Readability**: The syntax definitions are a close visual match to the CSS specification's grammar notation. A developer familiar with CSS can read the definitions and understand what they accept.

### Immutable property threading

The use of `record class` `with` expressions means that parsing never mutates state. Each syntax node receives the property-so-far and returns a new copy with its contribution added. If a combinator like `OneOf` needs to backtrack, it simply discards the failed attempt's result and tries the next alternative with the original property — no state to undo. This makes the backtracking in `OneOfSyntax` and `AnyOrderSyntax` trivially correct.

The tradeoff of immutable objects having copy overhead versus mutable objects having revert overhead when a parsing rule must rewind is correctly decided in favor of immutable objects: they are easier to prove correct, and correctness is the overriding concern. Mutable state with backtracking is a well-known source of subtle bugs — forgetting to restore a field, restoring fields in the wrong order, or partially reverting when an exception interrupts the process. Immutable threading eliminates all of these failure modes by construction.

### Automatic name mapping

The `Hyphenize()` convention — converting C# PascalCase to CSS hyphenated-lowercase — means that the C# type system and the CSS name system stay in sync automatically. Adding `FlexDirection` to the `KnownPropertyKind` enum automatically registers `"flex-direction"` as a known property name. Adding `RowReverse` to a `FlexDirection` enum automatically makes `"row-reverse"` a valid keyword value. No string tables to maintain.

### Messages collection

`CssPropertyParser` accepts an optional `Messages` parameter. If provided, the parser's warnings and errors will be added to the caller's collection; if omitted, the parser creates its own. This is the same pattern used by `HtmlParser` and `CssSelectorParser`.

---

## Supported CSS Properties

The `KnownProperties` directory contains ~140 concrete property classes across ~90 files. Each class is a small `record class` holding the parsed property data, plus an `Apply()` method that writes its values into a `ComputedStyle`, a `CopyProperty()` method for `inherit`/`initial` support, and a `ToString()` that emits the original CSS text (or an equivalent). Many properties share common base classes to avoid repetition.

The properties are organized below by category. For each property, the class name and its data fields are listed. **Shorthand** properties decompose into multiple sub-properties via `DecomposeInternal()` — the style system expands them before specificity resolution.

### Layout and positioning

| CSS property | Class | Fields |
|-------------|-------|--------|
| `display` | `DisplayProperty` | `DisplayKind Display` |
| `position` | `PositionProperty` | `PositionKind Position` |
| `top` | `TopProperty` | `Width Width` |
| `right` | `RightProperty` | `Width Width` |
| `bottom` | `BottomProperty` | `Width Width` |
| `left` | `LeftProperty` | `Width Width` |
| `float` | `FloatProperty` | `FloatMode Float` |
| `clear` | `ClearProperty` | `ClearMode ClearMode` |
| `z-index` | `ZIndexProperty` | `int ZIndex`, `bool Auto` |
| `visibility` | `VisibilityProperty` | `VisibilityKind Visibility` |
| `opacity` | `OpacityProperty` | `double Opacity` |
| `overflow` | `OverflowProperty` | `OverflowKind Overflow` — **shorthand** → `overflow-x`, `overflow-y` |
| `overflow-x` | `OverflowXProperty` | `OverflowKind OverflowX` |
| `overflow-y` | `OverflowYProperty` | `OverflowKind OverflowY` |
| `clip` | `ClipProperty` | `CssRect? Rect`, `bool Auto` |
| `resize` | `ResizeProperty` | `ResizeKind Resize` |

The `top`, `right`, `bottom`, `left` properties (and many others below) share the `WidthPropertyBase` base class, which holds a single `Width` value — a struct that can represent a length, a percentage, or `auto`.

### Box model — sizing

| CSS property | Class | Fields |
|-------------|-------|--------|
| `width` | `WidthProperty` | `Width Width` |
| `height` | `HeightProperty` | `Width Width` |
| `min-width` | `MinWidthProperty` | `Measure Offset` |
| `min-height` | `MinHeightProperty` | `Measure Offset` |
| `max-width` | `MaxWidthProperty` | `Measure Offset`, `bool None` |
| `max-height` | `MaxHeightProperty` | `Measure Offset`, `bool None` |
| `box-sizing` | `BoxSizingProperty` | `BoxSizingMode Mode` |

### Box model — margin

| CSS property | Class | Fields |
|-------------|-------|--------|
| `margin` | `MarginProperty` | `IReadOnlyList<Width> Widths` — **shorthand** → 4 edges |
| `margin-top` | `MarginTopProperty` | `Width Width` |
| `margin-right` | `MarginRightProperty` | `Width Width` |
| `margin-bottom` | `MarginBottomProperty` | `Width Width` |
| `margin-left` | `MarginLeftProperty` | `Width Width` |

### Box model — padding

| CSS property | Class | Fields |
|-------------|-------|--------|
| `padding` | `PaddingProperty` | `IReadOnlyList<Width> Widths` — **shorthand** → 4 edges |
| `padding-top` | `PaddingTopProperty` | `Width Width` |
| `padding-right` | `PaddingRightProperty` | `Width Width` |
| `padding-bottom` | `PaddingBottomProperty` | `Width Width` |
| `padding-left` | `PaddingLeftProperty` | `Width Width` |

`MarginProperty` and `PaddingProperty` share the `WidthMultiPropertyBase` base class, which holds 1–4 `Width` values and decomposes them into individual edge properties using the standard CSS shorthand expansion (1 value = all edges, 2 = vertical/horizontal, 3 = top/horizontal/bottom, 4 = top/right/bottom/left).

### Border

| CSS property | Class | Fields |
|-------------|-------|--------|
| `border` | `BorderProperty` | `Measure BorderWidth`, `BorderStyle BorderStyle`, `Color32? BorderColor` — **shorthand** → all 12 edge sub-properties |
| `border-top` | `BorderTopProperty` | `Measure Width`, `BorderStyle Style`, `Color32? Color` — **shorthand** → width, style, color |
| `border-right` | `BorderRightProperty` | *(same as above)* |
| `border-bottom` | `BorderBottomProperty` | *(same as above)* |
| `border-left` | `BorderLeftProperty` | *(same as above)* |
| `border-width` | `BorderWidthProperty` | `IReadOnlyList<Measure> Widths` — **shorthand** → 4 edge widths |
| `border-top-width` | `BorderTopWidthProperty` | `Measure Width` |
| `border-right-width` | `BorderRightWidthProperty` | `Measure Width` |
| `border-bottom-width` | `BorderBottomWidthProperty` | `Measure Width` |
| `border-left-width` | `BorderLeftWidthProperty` | `Measure Width` |
| `border-style` | `BorderStyleProperty` | `IReadOnlyList<BorderStyle> Styles` — **shorthand** → 4 edge styles |
| `border-top-style` | `BorderTopStyleProperty` | `BorderStyle Style` |
| `border-right-style` | `BorderRightStyleProperty` | `BorderStyle Style` |
| `border-bottom-style` | `BorderBottomStyleProperty` | `BorderStyle Style` |
| `border-left-style` | `BorderLeftStyleProperty` | `BorderStyle Style` |
| `border-color` | `BorderColorProperty` | `IReadOnlyList<Color32> Colors` — **shorthand** → 4 edge colors |
| `border-top-color` | `BorderTopColorProperty` | `Color32 Color` |
| `border-right-color` | `BorderRightColorProperty` | `Color32 Color` |
| `border-bottom-color` | `BorderBottomColorProperty` | `Color32 Color` |
| `border-left-color` | `BorderLeftColorProperty` | `Color32 Color` |
| `border-radius` | `BorderRadiusProperty` | `IReadOnlyList<Measure> Radii` — **shorthand** → 4 corners |
| `border-top-left-radius` | `BorderTopLeftRadiusProperty` | `Measure Radius` |
| `border-top-right-radius` | `BorderTopRightRadiusProperty` | `Measure Radius` |
| `border-bottom-left-radius` | `BorderBottomLeftRadiusProperty` | `Measure Radius` |
| `border-bottom-right-radius` | `BorderBottomRightRadiusProperty` | `Measure Radius` |
| `border-collapse` | `BorderCollapseProperty` | `BorderCollapse Collapse` |
| `border-spacing` | `BorderSpacingProperty` | `Measure Length`, `Measure Length2` |

The per-edge border properties share three abstract base classes — `BorderEdgeWidthProperty`, `BorderEdgeStyleProperty`, and `BorderEdgeColorProperty` — one per aspect. The combined per-edge properties (`border-top`, etc.) share `BorderEdgeProperty`. The corner radius properties share `BorderCornerRadiusProperty`.

### Outline

| CSS property | Class | Fields |
|-------------|-------|--------|
| `outline` | `OutlineProperty` | `BorderStyle Style`, `Measure Width`, `Color32? Color`, `bool Invert` — **shorthand** → color, style, width |
| `outline-color` | `OutlineColorProperty` | `Color32 Color`, `bool Invert` |
| `outline-style` | `OutlineStyleProperty` | `BorderStyle Style` |
| `outline-width` | `OutlineWidthProperty` | `Measure Width` |
| `outline-offset` | `OutlineOffsetProperty` | `Measure Offset` |

### Background

| CSS property | Class | Fields |
|-------------|-------|--------|
| `background` | `BackgroundProperty` | Sub-property references — **shorthand** → all 7 sub-properties |
| `background-color` | `BackgroundColorProperty` | `Color32 Color` |
| `background-image` | `BackgroundImageProperty` | `IReadOnlyList<BackgroundLayerBase> BackgroundLayers` |
| `background-repeat` | `BackgroundRepeatProperty` | `IReadOnlyList<BackgroundRepeat> Repeats` |
| `background-attachment` | `BackgroundAttachmentProperty` | `IReadOnlyList<BackgroundAttachment> Attachments` |
| `background-position` | `BackgroundPositionProperty` | `IReadOnlyList<BackgroundPosition> Positions` |
| `background-origin` | `BackgroundOriginProperty` | `IReadOnlyList<BackgroundOrigin> Origins` |
| `background-size` | `BackgroundSizeProperty` | `IReadOnlyList<BackgroundSize> Sizes` |

The multi-layer background properties (all except `background-color`) hold lists of values, one per background layer. The `background` shorthand decomposes into all seven sub-properties.

### Text

| CSS property | Class | Fields |
|-------------|-------|--------|
| `color` | `ColorProperty` | `Color32 Color` |
| `text-align` | `TextAlignProperty` | `TextAlign TextAlign` |
| `text-decoration` | `TextDecorationProperty` | `TextDecorationLineKind? TextDecoration` |
| `text-transform` | `TextTransformProperty` | `TextTransform TextTransform` |
| `text-indent` | `TextIndentProperty` | `Width Width` |
| `text-shadow` | `TextShadowProperty` | `IReadOnlyList<Shadow> Shadows` |
| `letter-spacing` | `LetterSpacingProperty` | `bool Normal`, `Measure Length` |
| `word-spacing` | `WordSpacingProperty` | `bool Normal`, `Measure Length` |
| `line-height` | `LineHeightProperty` | `bool Normal`, `double? Number`, `Measure Measure` |
| `vertical-align` | `VerticalAlignProperty` | `VerticalAlign VerticalAlign`, `Measure VerticalAlignLength` |
| `white-space` | `WhiteSpaceProperty` | `WhiteSpaceKind WhiteSpace` |
| `direction` | `DirectionProperty` | `WritingDirection Direction` |
| `unicode-bidi` | `UnicodeBidiProperty` | `UnicodeBidi UnicodeBidi` |

### Font

| CSS property | Class | Fields |
|-------------|-------|--------|
| `font` | `FontProperty` | Sub-property references — **shorthand** → style, variant, weight, size, line-height, family |
| `font-family` | `FontFamilyProperty` | `IReadOnlyList<FontFamily> Families` |
| `font-size` | `FontSizeProperty` | `Measure Measure`, `AbsoluteFontSize AbsoluteFontSize`, `RelativeFontSize RelativeFontSize` |
| `font-style` | `FontStyleProperty` | `FontStyle Style` |
| `font-variant` | `FontVariantProperty` | `FontVariant Variant` |
| `font-weight` | `FontWeightProperty` | `FontWeightName Name`, `int Amount` |

The `font` shorthand can also parse system font keywords (`caption`, `icon`, `menu`, `message-box`, `small-caption`, `status-bar`) via `SpecialFontProperty`.

### Flexbox

| CSS property | Class | Fields |
|-------------|-------|--------|
| `flex` | `FlexProperty` | `double? Grow`, `double? Shrink`, `Measure Measure`, `bool Auto`, `bool Content`, `bool None` — **shorthand** → grow, shrink, basis |
| `flex-basis` | `FlexBasisProperty` | `Measure Measure`, `bool Auto`, `bool Content` |
| `flex-direction` | `FlexDirectionProperty` | `FlexDirection Direction` |
| `flex-flow` | `FlexFlowProperty` | `FlexDirection Direction`, `FlexWrap Wrap` — **shorthand** → direction, wrap |
| `flex-grow` | `FlexGrowProperty` | `double Grow` |
| `flex-shrink` | `FlexShrinkProperty` | `double Shrink` |
| `flex-wrap` | `FlexWrapProperty` | `FlexWrap Wrap` |
| `align-content` | `AlignContentProperty` | `AlignContentKind AlignContent` |
| `align-items` | `AlignItemsProperty` | `AlignItemsKind AlignItems` |
| `align-self` | `AlignSelfProperty` | `AlignSelfKind AlignSelf` |
| `justify-content` | `JustifyContentProperty` | `JustifyContentKind JustifyContent` |
| `order` | `OrderProperty` | `int Order` |

### List style

| CSS property | Class | Fields |
|-------------|-------|--------|
| `list-style` | `ListStyleProperty` | Sub-property references — **shorthand** → type, position, image |
| `list-style-type` | `ListStyleTypeProperty` | `ListStyleType Style` |
| `list-style-position` | `ListStylePositionProperty` | `ListStylePosition Position` |
| `list-style-image` | `ListStyleImageProperty` | `string? Uri`, `bool None` |

### Table

| CSS property | Class | Fields |
|-------------|-------|--------|
| `table-layout` | `TableLayoutProperty` | `TableLayout TableLayout` |
| `caption-side` | `CaptionSideProperty` | `CaptionSide CaptionSide` |
| `empty-cells` | `EmptyCellsProperty` | `EmptyCellsMode EmptyCells` |

### Generated content

| CSS property | Class | Fields |
|-------------|-------|--------|
| `content` | `ContentProperty` | `IReadOnlyList<ContentPiece> Pieces` |
| `quotes` | `QuotesProperty` | `IReadOnlyList<string> Quotes`, `bool None` |
| `counter-increment` | `CounterIncrementProperty` | `IReadOnlyList<Counter> Counters`, `bool None` |
| `counter-reset` | `CounterResetProperty` | `IReadOnlyList<Counter> Counters`, `bool None` |

### Shadows

| CSS property | Class | Fields |
|-------------|-------|--------|
| `box-shadow` | `BoxShadowProperty` | `IReadOnlyList<Shadow> Shadows` |
| `text-shadow` | `TextShadowProperty` | `IReadOnlyList<Shadow> Shadows` |

### Paged media

| CSS property | Class | Fields |
|-------------|-------|--------|
| `page-break-after` | `PageBreakAfterProperty` | `PageBreakOption Break` |
| `page-break-before` | `PageBreakBeforeProperty` | `PageBreakOption Break` |
| `page-break-inside` | `PageBreakInsideProperty` | `PageBreakInsideOption Break` |
| `widows` | `WidowsProperty` | `int Count` |
| `orphans` | `OrphansProperty` | `int Count` |

### Cursor

| CSS property | Class | Fields |
|-------------|-------|--------|
| `cursor` | `CursorProperty` | `IReadOnlyList<CustomCursor> CustomCursors`, `CursorKind CursorKind` |

### Summary of shorthand decomposition

Fifteen properties are shorthands that decompose into sub-properties:

| Shorthand | Decomposes into |
|-----------|----------------|
| `background` | `background-color`, `background-image`, `background-repeat`, `background-attachment`, `background-position`, `background-origin`, `background-size` |
| `border` | All 12 edge sub-properties (4 edges × width, style, color) |
| `border-top`, `-right`, `-bottom`, `-left` | 3 sub-properties each (width, style, color) |
| `border-width` | 4 edge widths |
| `border-style` | 4 edge styles |
| `border-color` | 4 edge colors |
| `border-radius` | 4 corner radii |
| `outline` | `outline-color`, `outline-style`, `outline-width` |
| `font` | `font-style`, `font-variant`, `font-weight`, `font-size`, `line-height`, `font-family` |
| `flex` | `flex-grow`, `flex-shrink`, `flex-basis` |
| `flex-flow` | `flex-direction`, `flex-wrap` |
| `list-style` | `list-style-type`, `list-style-position`, `list-style-image` |
| `margin` | 4 edge margins |
| `padding` | 4 edge paddings |
| `overflow` | `overflow-x`, `overflow-y` |
