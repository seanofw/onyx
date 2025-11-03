# Onyx

Onyx is the C# UI anti-framework.

## Overview

There's no shortage of UI frameworks for C#.  [WinForms](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/), [WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/overview/), and [Avalonia](https://avaloniaui.net/), [MAUI](https://dotnet.microsoft.com/en-us/apps/maui) and [WinUI 3](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/), and there are ports of [Qt](https://www.qt.io/) and [GTK](https://www.gtk.org/) and [Dear IMGUI](https://github.com/ocornut/imgui) too.  What these all have in common is that at some level, they're _procedural_:  Even if you write `<Button>` in XAML, you're still really just writing `new Button()` and hoping that it runs at the right time.

Onyx is different.

Why do developers love the web?  It's not because they love distributed programming — distributed programming is _hard_.  What developers love is that after thirty years of testing and refinement by everyone on the entire planet, HTML and CSS have become _great_ languages to work in.  Modern web standards coding on evergreen browsers is predictable, reliable, straightforward, and generally, it _just works_.  HTML markup describes semantic relationships well, the HTML DOM is well-understood and fairly easy to manipulate, and CSS is like _magic_ for describing appearances.

But to get all of those lovely things, you have to have a browser.  (Or something like [Electron](https://www.electronjs.org/), which is just a browser in a box.)  You can't do HTML and CSS without a browser.

Until now.

Onyx is _half_ of a browser.  (Or at least a major _part_ of a browser.)  Onyx is a pure-.NET, managed-code library that eats web-standards HTML and CSS, constructs all the same data structures that a browser constructs, manages them using all the same algorithms that a browser manages, and renders them the same way a browser does.

Onyx is:

* HTML parsing
* DOM construction and management
* CSS parsing
* Selector querying
* Style evaluation
* Flow and layout
* Rendering

Onyx is not:

* An HTTP client.
* A JavaScript runtime.
* An application framework.

It's not a browser.  It's just the part of the browser that you always wished wasn't part of the browser.

Importantly, each of its pieces can be used independently.  Want an advanced, standards-compliant HTML parser for C#?  You can use just that part of Onyx and ignore the rest.  Onyx has *no opinions*, other than that you should be using HTML and CSS for your UI.

In fact, you can even use Onyx *with* other major UI frameworks.  It doesn't care what it's rendering to — on a window of its own, on an area of an existing UI, or even just onto a bitmap.

Intrigued?  Read on.

## Project Status

Onyx is currently in an "alpha" status.  It is *not* yet production-ready!  Many of its intended features exist, and much of its intended functionality is implemented, but not all of it is there, and not all of it is well-tested yet.

Current implementation status:

* HTML parser: 100%
* DOM: 100%
* DOM Linq APIs: 100%
* CSS parser: 100%
* Optimized selector IsMatch(): 100%
* Optimized selector Find(): 100%
* Optimized style locator: 100%
* Computed-style applicator: 100%
* Basic layout: 0%
* Float layouts: 0%
* Flexbox: 0%
* Tables: 0%
* Render pipeline: 0%
* SkiaSharp backend: 0%
* Event management: 0%

## Installation

Eventually this is Nuget packages.  For now, build it yourself.  (See [Building Onyx](#building-onyx) below.)

Onyx itself has no third-party dependencies on anything other than .NET 8+.  (Backports to older .NET versions are intended, but not yet implemented.)

The Onyx core DLL contains all of the algorithms and data structures.  These require nothing and expect nothing.

Future DLLs will be added for rendering and/or integration with other UI frameworks.  The SkiaSharp backend will be the preferred backend, but SkiaSharp will not be a mandatory dependency of Onyx itself.

## Targeted Standards

The baseline measure for Onyx 1.0 is fully standards-compliant [HTML 5](https://www.w3.org/TR/2012/CR-html5-20121217/) and [CSS 2.1](https://www.w3.org/TR/CSS2/), with several useful [CSS 3](https://www.w3.org/Style/CSS/Overview.en.html) extensions:

* Common selector extensions, like `:checked`
* `display: flex` and the rest of flexbox
* `border-radius`
* `box-shadow` and `text-shadow`
* `box-sizing`
* `linear-gradient` and `radial-gradient`
* Multiple background images
* `background-size`

In short, it's all the things you'd need to make a modern UI:  The goal of Onyx is to be able to replace many of the use cases of other UI frameworks with ordinary HTML and CSS, and ideally be so close to standards-compliance that code from major CSS libraries can be used in Onyx verbatim.

Long-term, full CSS 3 compatibility would be desirable, but for now, the goal is CSS 2.1, plus a few of the most desirable CSS 3 enhancements.

## HTML Elements

Onyx supports the full set of HTML 5 elements that comprise the *body* of a document.  Just imagine that `<body>...</body>` surrounds your content, and write whatever else you want.

(Elements that belong to the head are effectively metadata, and Onyx will simply ignore them if you use them.  You don't need a `<title>` tag if you're already writing `new Window("My Application")` anyway.)

Special parsing is provided, as expected for standards compliance, for certain elements:

* `<style>`, `<xmp>`, and `<script>` are consumed verbatim
* `<img>`, `<link>`, `<br>`, `<hr>`, `<input>`, and `<meta>` automatically close

Complex recovery rules are provided for mismatched tags, and for missing tags, per HTML 5 parsing rules.

Errors and mismatches are ignored, per HTML 5 parsing rules, but they *are* recorded and emitted as warnings, so that if you make mistakes in your markup, you can fix them.

## CSS

Onyx has no strong opinion on the *presentation* of most elements — it provides a default stylesheet that matches that of most browsers, and it invites you to load your own CSS to style whatever you need.

Selectors follow standard CSS specificity rules.  All standard CSS 2.1 selectors are supported, including the `>` and `+` and `~` operators, and attribute selectors, and starts-with and ends-with and contains attribute selectors too.  Selectors may be used to style any element.

Note that the default stylesheet provides default styles for a `<row>` element and a `<column>` element — more on this below.

(**NOTE**:  `:before` and `:after` pseudo-element selectors don't work yet.  Sorry.  It's on the list!)

### CSS properties supported

Each of these should work as defined by web standards.  If they do not, please report an issue.

* `align-content`, `align-items`, `align-self`
* `background`, `background-attachment`, `background-color`, `background-image`, `background-origin`, `background-position`, `background-repeat`, `background-size`
* `border`, `border-color`, `border-*-color`, `border-style`, `border-*-style`, `border-width`, `border-*-width`, `border-radius`, `border-*-radius`
* `box-shadow`, `box-sizing`
* `border-collapse`, `border-spacing`
* `caption-side`, `clear`, `clip`, `color`, `content`, `counter-increment`, `counter-reset`, `cursor`
* `direction`, `display`
* `empty-cells`
* `flex`, `flex-basis`, `flex-direction`, `flex-flow`, `flex-grow`, `flex-shrink`, `flex-wrap`
* `font`, `font-family`, `font-size`, `font-style`, `font-variant`, `font-weight`
* `float`
* `justify-content`
* `letter-spacing`, `line-height`, `list-style`, `list-style-image`, `list-style-position`, `list-style-type`
* `margin`, `margin-*`
* `min-width`, `width`, `max-width`
* `min-height`, `height`, `max-height`
* `opacity`, `order`, `orphans`, `overflow`, `overflow-x`, `overflow-y`
* `outline`, `outline-color`, `outline-offset`, `outline-style`, `outline-width`
* `padding`, `padding-*`
* `position`, `page-break-before`, `page-break-after`, `page-break-inside`
* `quotes`
* `resize`
* `table-layout`, `text-align`, `text-decoration`, `text-shadow`, `text-transform`
* `unicode-bidi`
* `vertical-align`, `visibility`
* `white-space`, `widows`, `word-spacing`
* `z-index`

## The DOM

Onyx targets compatibility with HTML and CSS, but it explicitly does *not* target compatibility with the traditional DOM.  Onyx's DOM is similar, but is more streamlined.  Some legacy APIs have been removed, and new APIs have been added, and Onyx goes out of its way to provide Linq compatibility for easy element analysis and traversal.

Major differences include:

* Everything uses `TitleCase`, per .NET naming conventions.
* `GetElementById()` and `ByClassName()` and `ByType()` do not return "live" element lists, but simply fixed collections.
* `QuerySelectorAll()` has been replaced with the far shorter name `Find()`.  There is no `QuerySelector()` equivalent.
* Elements have no `GetAttributeBy()`/`SetAttribute()` methods; they just have an `Attributes` property that implements `IDictionary<string, Attribute>`.
* Any `IEnumerable<Element>` has many useful query methods, like `Find(selector)` and `Where(selector)` and `Closest(selector)` and `Descendants()` and `Ancestors()`, often obviating the need for using `ChildNodes`/`ParentNode`/`NextSibling`/`PreviousSibling` directly.
* The `Classname` property is still whitespace-separated, but there is also a `Classnames` property that is an `IReadOnlySet<string>`, and `AddClass()` and `RemoveClass()` and `HasClass()` are first-class methods on individual `Elements` — and on collections.
* `Document` is intentionally a simple container — unlike in the normal DOM, it's not very special or very complex or even necessarily the root of the element tree.
* Methods that return elements rarely return them in document order, for performance reasons:  If you actually need document order for any collection of elements, there is an `.OrderByPosition()` extension method on `IEnumerable<Element>`.

All of these differences are designed to simplify working with the DOM in modern C# by stripping away old cruft, adding methods inspired by jQuery, and ensuring deep Linq compatibility — in many cases, these changes allow complicated JavaScript DOM manipulations to be reduced to simple Linq one-liners.

Full compatibility with the traditional JS DOM is not in scope, nor is it expected to ever be in scope.  Please do not report issues about missing DOM APIs.

## `<row>` and `<column>`

Because Onyx is intended to be used for building UIs, we provide one very small tweak to HTML — still web-compatible, but designed to be useful for Onyx's target use case.  The Onyx default stylesheet includes these simple rules:

```css
row {
	display: flex;
	flex-flow: row nowrap;
}

column {
	display: flex;
	flex-flow: column nowrap;
}
```

This effectively "creates" new `<row>` and `<column>` elements that don't exist in "normal" HTML by simply assigning them default flexbox styles and behaviors.  You can reproduce the same effect in "normal" HTML by including the eight lines above on any web page, so we do not consider this to fundamentally be a *breaking change* with HTML and CSS.

## Building Onyx

You will need Visual Studio 2022 or later.  Open the `.sln` file.  Click "Build" > "Rebuild Solution".  There should be neither warnings nor errors.  The build will produce a single `Onyx.dll` as output.

## License

Onyx is covered under the [MIT Open-Source License](LICENSE.txt).  Share and enjoy.
