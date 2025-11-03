using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	/// <summary>
	/// A single computed style.  This is designed as a copy-on-write tree of objects.
	/// </summary>
	public class ComputedStyle
	{
		internal readonly ComputedEnumsStyle Enums;
		internal readonly ComputedSizes Sizes;
		internal readonly ComputedBackgroundStyle Background;
		internal readonly ComputedBorderStyle Border;
		internal readonly ComputedInheritedStyle Inherited;
		internal readonly ComputedRareFieldsStyle RareFields;

		internal ComputedFlexStyle Flex => RareFields.Flex;
		internal ComputedPageBreakStyle PageBreak => RareFields.PageBreak;
		internal ComputedOutlineStyle Outline => RareFields.Outline;
		internal ComputedTextStyle Text => Inherited.Text;
		internal ComputedFontStyle Font => Inherited.Font;
		internal ComputedTableStyle Table => Inherited.Table;
		internal ComputedListStyle List => Inherited.List;
		internal ComputedMiscInheritedStyle MiscInherited => Inherited.Misc;
		internal ComputedSuperRareFieldsStyle SuperRare => RareFields.SuperRare;

		// Common enums
		public DisplayKind Display => Enums.Display;
		public PositionKind Position => Enums.Position;
		public TextDecorationLineKind TextDecoration => Enums.TextDecoration;
		public ClearMode Clear => Enums.Clear;
		public FloatMode Float => Enums.Float;
		public ResizeKind Resize => Enums.Resize;
		public BoxSizingMode BoxSizing => Enums.BoxSizing;
		public VerticalAlign VerticalAlign => Enums.VerticalAlign;
		public UnicodeBidi UnicodeBidi => Enums.UnicodeBidi;
		public OverflowKind OverflowX => Enums.OverflowX;
		public OverflowKind OverflowY => Enums.OverflowY;
		public TableLayout TableLayout => Enums.TableLayout;

		// Box offsets
		public Measure Bottom => Sizes.Bottom;
		public Measure Left => Sizes.Left;
		public Measure Right => Sizes.Right;
		public Measure Top => Sizes.Top;

		// Dimensions
		public Measure Width => Sizes.Width;
		public Measure Height => Sizes.Height;
		public Measure MinWidth => Sizes.MinWidth;
		public Measure MinHeight => Sizes.MinHeight;
		public Measure MaxWidth => Sizes.MaxWidth;
		public Measure MaxHeight => Sizes.MaxHeight;

		// Padding
		public Measure PaddingTop => Sizes.PaddingTop;
		public Measure PaddingRight => Sizes.PaddingRight;
		public Measure PaddingBottom => Sizes.PaddingBottom;
		public Measure PaddingLeft => Sizes.PaddingLeft;

		// Margin
		public Measure MarginTop => Sizes.MarginTop;
		public Measure MarginRight => Sizes.MarginRight;
		public Measure MarginBottom => Sizes.MarginBottom;
		public Measure MarginLeft => Sizes.MarginLeft;

		// Flex properties
		public AlignContentKind AlignContent => Flex.AlignContent;
		public AlignItemsKind AlignItems => Flex.AlignItems;
		public AlignSelfKind AlignSelf => Flex.AlignSelf;
		public Measure FlexBasis => Flex.Basis;
		public FlexDirection FlexDirection => Flex.Direction;
		public double FlexGrow => Flex.Grow;
		public double FlexShrink => Flex.Shrink;
		public FlexWrap FlexWrap => Flex.Wrap;
		public JustifyContentKind JustifyContent => Flex.JustifyContent;
		public int Order => Flex.Order;

		// Page-break properties
		public PageBreakOption PageBreakAfter => PageBreak.BreakAfter;
		public PageBreakOption PageBreakBefore => PageBreak.BreakBefore;
		public PageBreakInsideOption PageBreakInside => PageBreak.BreakInside;

		// Text-layout properties
		public TextAlign TextAlign => Text.TextAlign;
		public WhiteSpaceKind WhiteSpace => Text.WhiteSpace;
		public WritingDirection Direction => Text.Direction;
		public TextTransform TextTransform => Text.TextTransform;
		public Measure WordSpacing => Text.WordSpacing;
		public Measure TextIndent => Text.TextIndent;
		public Measure LetterSpacing => Text.LetterSpacing;
		public Measure LineHeight => Text.LineHeight;

		// Borders
		public Color32 BorderTopColor => Border.Top.Color;
		public Color32 BorderRightColor => Border.Right.Color;
		public Color32 BorderBottomColor => Border.Bottom.Color;
		public Color32 BorderLeftColor => Border.Left.Color;

		public BorderStyle BorderTopStyle => Border.Top.Style;
		public BorderStyle BorderRightStyle => Border.Right.Style;
		public BorderStyle BorderBottomStyle => Border.Bottom.Style;
		public BorderStyle BorderLeftStyle => Border.Left.Style;

		public Measure BorderTopWidth => Border.Top.Width;
		public Measure BorderRightWidth => Border.Right.Width;
		public Measure BorderBottomWidth => Border.Bottom.Width;
		public Measure BorderLeftWidth => Border.Left.Width;

		public Measure BorderTopLeftRadius => Border.TopLeftRadius;
		public Measure BorderTopRightRadius => Border.TopRightRadius;
		public Measure BorderBottomLeftRadius => Border.BottomLeftRadius;
		public Measure BorderBottomRightRadius => Border.BottomRightRadius;

		// Outlines
		public Measure OutlineWidth => Outline.Width;
		public Measure OutlineOffset => Outline.Offset;
		public Color32 OutlineColor => Outline.Color;
		public bool OutlineInvert => Outline.Invert;
		public BorderStyle OutlineStyle => Outline.Style;

		// Backgrounds
		public Color32 BackgroundColor => Background.Color;
		public IReadOnlyList<ComputedBackgroundLayer> BackgroundLayers => Background.Layers;
		public IReadOnlyList<Shadow> BoxShadows => Background.BoxShadows;
		public double Opacity => Background.Opacity;

		// Font stuff
		public Color32 Color => Font.Color;
		public IReadOnlyList<FontFamily> FontFamilies => Font.Families;
		public SpecialFontKind SpecialFont => Font.SpecialFont;
		public Measure FontSize => Font.Size;
		public FontStyle FontStyle => Font.Style;
		public int FontWeight => Font.Weight;
		public FontVariant FontVariant => Font.Variant;
		public IReadOnlyList<Shadow> TextShadows => Font.TextShadows;

		// Tables
		public BorderCollapse BorderCollapse => Table.BorderCollapse;
		public EmptyCellsMode EmptyCells => Table.EmptyCells;
		public CaptionSide CaptionSide => Table.CaptionSide;
		public Measure BorderSpacingX => Table.BorderSpacingX;
		public Measure BorderSpacingY => Table.BorderSpacingY;

		// Lists
		public string? ListStyleUri => List.Uri;
		public ListStylePosition ListStylePosition => List.Position;
		public ListStyleType ListStyleType => List.Type;

		// Misc inherited properties
		public int Widows => MiscInherited.Widows;
		public int Orphans => MiscInherited.Orphans;
		public IReadOnlyList<CustomCursor> CustomCursors => MiscInherited.CustomCursors;
		public CursorKind CursorKind => MiscInherited.CursorKind;
		public IReadOnlyList<string> Quotes => MiscInherited.Quotes;
		public VisibilityKind Visibility => Inherited.Visibility;

		// Rare fields
		public int ZIndex => RareFields.ZIndex;

		// Super-rare properties.
		public CssRect? Clip => SuperRare.Clip;
		public IReadOnlyList<ContentPiece> ContentPieces => SuperRare.ContentPieces;
		public Measure VerticalAlignLength => SuperRare.VerticalAlignLength;
		public IReadOnlyList<Counter> CounterIncrements => SuperRare.CounterIncrements;
		public IReadOnlyList<Counter> CounterResets => SuperRare.CounterResets;

		/// <summary>
		/// Default base style.
		/// </summary>
		public static ComputedStyle Default { get; }
			= new ComputedStyle(ComputedEnumsStyle.Default, ComputedSizes.Default,
				ComputedBackgroundStyle.Default, ComputedBorderStyle.Default,
				ComputedInheritedStyle.Default, ComputedRareFieldsStyle.Default);

		// Construction
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle(ComputedEnumsStyle enums, ComputedSizes sizes,
			ComputedBackgroundStyle background, ComputedBorderStyle border,
			ComputedInheritedStyle inherited, ComputedRareFieldsStyle rareFields)
		{
			Enums = enums;
			Sizes = sizes;
			Background = background;
			Border = border;
			Inherited = inherited;
			RareFields = rareFields;
		}

		// Replacing large style chunks
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithEnums(ComputedEnumsStyle enums)
			=> new ComputedStyle(enums, Sizes, Background, Border, Inherited, RareFields);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithSizes(ComputedSizes sizes)
			=> new ComputedStyle(Enums, sizes, Background, Border, Inherited, RareFields);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithBackground(ComputedBackgroundStyle background)
			=> new ComputedStyle(Enums, Sizes, background, Border, Inherited, RareFields);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithBorder(ComputedBorderStyle border)
			=> new ComputedStyle(Enums, Sizes, Background, border, Inherited, RareFields);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithInherited(ComputedInheritedStyle inherited)
			=> new ComputedStyle(Enums, Sizes, Background, Border, inherited, RareFields);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithRareFields(ComputedRareFieldsStyle rareFields)
			=> new ComputedStyle(Enums, Sizes, Background, Border, Inherited, rareFields);

		// Rare-field chunks
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithFlex(ComputedFlexStyle flex)
			=> WithRareFields(RareFields.WithFlex(flex));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithPageBreak(ComputedPageBreakStyle @break)
			=> WithRareFields(RareFields.WithPageBreak(@break));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithOutline(ComputedOutlineStyle outline)
			=> WithRareFields(RareFields.WithOutline(outline));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithSuperRare(ComputedSuperRareFieldsStyle superRare)
			=> WithRareFields(RareFields.WithSuperRare(superRare));

		// Large inherited chunks
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithText(ComputedTextStyle text)
			=> WithInherited(Inherited.WithText(text));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithFont(ComputedFontStyle font)
			=> WithInherited(Inherited.WithFont(font));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithTable(ComputedTableStyle table)
			=> WithInherited(Inherited.WithTable(table));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithList(ComputedListStyle list)
			=> WithInherited(Inherited.WithList(list));
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithMiscInherited(ComputedMiscInheritedStyle miscInherited)
			=> WithInherited(Inherited.WithMisc(miscInherited));

		// Common style
		public ComputedStyle WithDisplay(DisplayKind display)
			=> WithEnums(Enums.WithDisplay(display));
		public ComputedStyle WithPosition(PositionKind position)
			=> WithEnums(Enums.WithPosition(position));

		// Box offsets
		public ComputedStyle WithBottom(Measure bottom)
			=> WithSizes(Sizes.WithBottom(bottom));
		public ComputedStyle WithLeft(Measure left)
			=> WithSizes(Sizes.WithLeft(left));
		public ComputedStyle WithRight(Measure right)
			=> WithSizes(Sizes.WithRight(right));
		public ComputedStyle WithTop(Measure top)
			=> WithSizes(Sizes.WithTop(top));

		// Dimensions
		public ComputedStyle WithWidth(Measure width)
			=> WithSizes(Sizes.WithWidth(width));
		public ComputedStyle WithHeight(Measure height)
			=> WithSizes(Sizes.WithHeight(height));
		public ComputedStyle WithMinWidth(Measure minWidth)
			=> WithSizes(Sizes.WithMinWidth(minWidth));
		public ComputedStyle WithMinHeight(Measure minHeight)
			=> WithSizes(Sizes.WithMinHeight(minHeight));
		public ComputedStyle WithMaxWidth(Measure maxWidth)
			=> WithSizes(Sizes.WithMaxWidth(maxWidth));
		public ComputedStyle WithMaxHeight(Measure maxHeight)
			=> WithSizes(Sizes.WithMaxHeight(maxHeight));

		// Padding
		public ComputedStyle WithPadding(EdgeSizes edgeSizes)
			=> WithSizes(Sizes.WithPadding(edgeSizes));
		public ComputedStyle WithPaddingBottom(Measure paddingBottom)
			=> WithSizes(Sizes.WithPaddingBottom(paddingBottom));
		public ComputedStyle WithPaddingLeft(Measure paddingLeft)
			=> WithSizes(Sizes.WithPaddingLeft(paddingLeft));
		public ComputedStyle WithPaddingRight(Measure paddngRight)
			=> WithSizes(Sizes.WithPaddingRight(paddngRight));
		public ComputedStyle WithPaddingTop(Measure paddingTop)
			=> WithSizes(Sizes.WithPaddingTop(paddingTop));

		// Margin
		public ComputedStyle WithMargin(EdgeSizes edgeSizes)
			=> WithSizes(Sizes.WithMargin(edgeSizes));
		public ComputedStyle WithMarginBottom(Measure marginBottom)
			=> WithSizes(Sizes.WithMarginBottom(marginBottom));
		public ComputedStyle WithMarginLeft(Measure marginLeft)
			=> WithSizes(Sizes.WithMarginLeft(marginLeft));
		public ComputedStyle WithMarginRight(Measure marginRight)
			=> WithSizes(Sizes.WithMarginRight(marginRight));
		public ComputedStyle WithMarginTop(Measure marginTop)
			=> WithSizes(Sizes.WithMarginTop(marginTop));

		// Flex properties
		public ComputedStyle WithAlignContent(AlignContentKind alignContent)
			=> WithFlex(Flex.WithAlignContent(alignContent));
		public ComputedStyle WithAlignItems(AlignItemsKind alignItems)
			=> WithFlex(Flex.WithAlignItems(alignItems));
		public ComputedStyle WithAlignSelf(AlignSelfKind alignSelf)
			=> WithFlex(Flex.WithAlignSelf(alignSelf));
		public ComputedStyle WithFlexBasis(Measure basis)
			=> WithFlex(Flex.WithBasis(basis));
		public ComputedStyle WithFlexDirection(FlexDirection direction)
			=> WithFlex(Flex.WithDirection(direction));
		public ComputedStyle WithFlexGrow(double grow)
			=> WithFlex(Flex.WithGrow(grow));
		public ComputedStyle WithFlexShrink(double shrink)
			=> WithFlex(Flex.WithShrink(shrink));
		public ComputedStyle WithFlexWrap(FlexWrap wrap)
			=> WithFlex(Flex.WithWrap(wrap));
		public ComputedStyle WithJustifyContent(JustifyContentKind justifyContent)
			=> WithFlex(Flex.WithJustifyContent(justifyContent));
		public ComputedStyle WithOrder(int order)
			=> WithFlex(Flex.WithOrder(order));

		// Miscellaneous inherited
		public ComputedStyle WithWidows(int count)
			=> WithMiscInherited(MiscInherited.WithWidows(count));
		public ComputedStyle WithOrphans(int count)
			=> WithMiscInherited(MiscInherited.WithOrphans(count));
		public ComputedStyle WithCursor(CursorKind cursorKind, IEnumerable<CustomCursor>? customCursors = null)
			=> WithMiscInherited(MiscInherited.WithCursor(cursorKind, customCursors));

		// Page-break properties
		public ComputedStyle WithPageBreakAfter(PageBreakOption option)
			=> WithPageBreak(PageBreak.WithBreakAfter(option));
		public ComputedStyle WithPageBreakBefore(PageBreakOption option)
			=> WithPageBreak(PageBreak.WithBreakBefore(option));
		public ComputedStyle WithPageBreakInside(PageBreakInsideOption option)
			=> WithPageBreak(PageBreak.WithBreakInside(option));

		// Text-layout properties
		public ComputedStyle WithTextAlign(TextAlign align)
			=> WithText(Text.WithTextAlign(align));
		public ComputedStyle WithDirection(WritingDirection direction)
			=> WithText(Text.WithDirection(direction));
		public ComputedStyle WithWordSpacing(Measure spacing)
			=> WithText(Text.WithWordSpacing(spacing));
		public ComputedStyle WithTextIndent(Measure indent)
			=> WithText(Text.WithTextIndent(indent));
		public ComputedStyle WithWhiteSpace(WhiteSpaceKind whiteSpace)
			=> WithText(Text.WithWhiteSpace(whiteSpace));
		public ComputedStyle WithLetterSpacing(Measure spacing)
			=> WithText(Text.WithLetterSpacing(spacing));
		public ComputedStyle WithLineHeight(Measure lineHeight)
			=> WithText(Text.WithLineHeight(lineHeight));
		public ComputedStyle WithTextTransform(TextTransform textTransform)
			=> WithText(Text.WithTextTransform(textTransform));

		// Borders
		public ComputedStyle WithBorderColors(Color32 top, Color32 right, Color32 bottom, Color32 left)
			=> WithBorder(Border.WithColors(top, right, bottom, left));
		public ComputedStyle WithBorderTopColor(Color32 color)
			=> WithBorder(Border.WithTopColor(color));
		public ComputedStyle WithBorderRightColor(Color32 color)
			=> WithBorder(Border.WithRightColor(color));
		public ComputedStyle WithBorderBottomColor(Color32 color)
			=> WithBorder(Border.WithBottomColor(color));
		public ComputedStyle WithBorderLeftColor(Color32 color)
			=> WithBorder(Border.WithLeftColor(color));

		public ComputedStyle WithBorderStyles(BorderStyle top, BorderStyle right, BorderStyle bottom, BorderStyle left)
			=> WithBorder(Border.WithStyles(top, right, bottom, left));
		public ComputedStyle WithBorderTopStyle(BorderStyle style)
			=> WithBorder(Border.WithTopStyle(style));
		public ComputedStyle WithBorderRightStyle(BorderStyle style)
			=> WithBorder(Border.WithRightStyle(style));
		public ComputedStyle WithBorderBottomStyle(BorderStyle style)
			=> WithBorder(Border.WithBottomStyle(style));
		public ComputedStyle WithBorderLeftStyle(BorderStyle style)
			=> WithBorder(Border.WithLeftStyle(style));

		public ComputedStyle WithBorderWidths(Measure top, Measure right, Measure bottom, Measure left)
			=> WithBorder(Border.WithWidths(top, right, bottom, left));
		public ComputedStyle WithBorderTopWidth(Measure width)
			=> WithBorder(Border.WithTopWidth(width));
		public ComputedStyle WithBorderRightWidth(Measure width)
			=> WithBorder(Border.WithRightWidth(width));
		public ComputedStyle WithBorderBottomWidth(Measure width)
			=> WithBorder(Border.WithBottomWidth(width));
		public ComputedStyle WithBorderLeftWidth(Measure width)
			=> WithBorder(Border.WithLeftWidth(width));

		public ComputedStyle WithBorderRadii(Measure topLeft, Measure topRight, Measure bottomLeft, Measure bottomRight)
			=> WithBorder(Border.WithRadii(topLeft, topRight, bottomLeft, bottomRight));
		public ComputedStyle WithBorderTopLeftRadius(Measure radius)
			=> WithBorder(Border.WithTopLeftRadius(radius));
		public ComputedStyle WithBorderTopRightRadius(Measure radius)
			=> WithBorder(Border.WithTopRightRadius(radius));
		public ComputedStyle WithBorderBottomLeftRadius(Measure radius)
			=> WithBorder(Border.WithBottomLeftRadius(radius));
		public ComputedStyle WithBorderBottomRightRadius(Measure radius)
			=> WithBorder(Border.WithBottomRightRadius(radius));

		public ComputedStyle WithBorderTop(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithTop(borderEdgeStyle));
		public ComputedStyle WithBorderRight(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithRight(borderEdgeStyle));
		public ComputedStyle WithBorderBottom(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithBottom(borderEdgeStyle));
		public ComputedStyle WithBorderLeft(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithLeft(borderEdgeStyle));

		// Outlines
		public ComputedStyle WithOutlineWidth(Measure width)
			=> WithOutline(Outline.WithWidth(width));
		public ComputedStyle WithOutlineOffset(Measure offset)
			=> WithOutline(Outline.WithOffset(offset));
		public ComputedStyle WithOutlineColor(Color32 color, bool invert)
			=> WithOutline(Outline.WithColor(color, invert));
		public ComputedStyle WithOutlineStyle(BorderStyle style)
			=> WithOutline(Outline.WithStyle(style));

		// Backgrounds
		public ComputedStyle WithBackgroundColor(Color32 color)
			=> WithBackground(Background.WithColor(color));
		public ComputedStyle WithBackgroundLayers(IEnumerable<ComputedBackgroundLayer>? layers)
			=> WithBackground(Background.WithLayers(layers));
		public ComputedStyle WithBoxShadows(IEnumerable<Shadow>? shadows)
			=> WithBackground(Background.WithBoxShadows(shadows));
		public ComputedStyle WithOpacity(double opacity)
			=> WithBackground(Background.WithOpacity(opacity));

		// Font stuff
		public ComputedStyle WithColor(Color32 color)
			=> WithFont(Font.WithColor(color));
		public ComputedStyle WithFontFamilies(IEnumerable<FontFamily>? famililies)
			=> WithFont(Font.WithFamilies(famililies));
		public ComputedStyle WithSpecialFont(SpecialFontKind specialFont)
			=> WithFont(Font.WithSpecialFont(specialFont));
		public ComputedStyle WithFontSize(Measure size)
			=> WithFont(Font.WithSize(size));
		public ComputedStyle WithFontStyle(FontStyle style)
			=> WithFont(Font.WithStyle(style));
		public ComputedStyle WithFontWeight(int weight)
			=> WithFont(Font.WithWeight(weight));
		public ComputedStyle WithFontVariant(FontVariant variant)
			=> WithFont(Font.WithVariant(variant));
		public ComputedStyle WithTextShadows(IEnumerable<Shadow>? shadows)
			=> WithFont(Font.WithTextShadows(shadows));

		// Tables
		public ComputedStyle WithBorderCollapse(BorderCollapse borderCollapse)
			=> WithTable(Table.WithBorderCollapse(borderCollapse));
		public ComputedStyle WithEmptyCells(EmptyCellsMode emptyCells)
			=> WithTable(Table.WithEmptyCells(emptyCells));
		public ComputedStyle WithCaptionSide(CaptionSide captionSide)
			=> WithTable(Table.WithCaptionSide(captionSide));
		public ComputedStyle WithBorderSpacing(Measure spacing)
			=> WithTable(Table.WithBorderSpacing(spacing));
		public ComputedStyle WithBorderSpacing(Measure spacingX, Measure spacingY)
			=> WithTable(Table.WithBorderSpacing(spacingX, spacingY));

		// Lists
		public ComputedStyle WithListStyleUri(string? uri)
			=> WithList(List.WithUri(uri));
		public ComputedStyle WithListStylePosition(ListStylePosition position)
			=> WithList(List.WithPosition(position));
		public ComputedStyle WithListStyleType(ListStyleType type)
			=> WithList(List.WithType(type));

		// Rare-ish properties
		public ComputedStyle WithZIndex(int zIndex)
			=> WithRareFields(RareFields.WithZIndex(zIndex));

		// Super-rare properties
		public ComputedStyle WithClip(CssRect? clip)
			=> WithSuperRare(SuperRare.WithClip(clip));
		public ComputedStyle WithContentPieces(IEnumerable<ContentPiece>? contentPieces)
			=> WithSuperRare(SuperRare.WithContent(contentPieces));
		public ComputedStyle WithCounterIncrements(IEnumerable<Counter>? counters)
			=> WithSuperRare(SuperRare.WithCounterIncrements(counters));
		public ComputedStyle WithCounterResets(IEnumerable<Counter>? counters)
			=> WithSuperRare(SuperRare.WithCounterResets(counters));

		// Enums
		public ComputedStyle WithClear(ClearMode clear)
			=> WithEnums(Enums.WithClear(clear));
		public ComputedStyle WithFloat(FloatMode @float)
			=> WithEnums(Enums.WithFloat(@float));
		public ComputedStyle WithResize(ResizeKind resize)
			=> WithEnums(Enums.WithResize(resize));
		public ComputedStyle WithBoxSizing(BoxSizingMode boxSizing)
			=> WithEnums(Enums.WithBoxSizing(boxSizing));
		public ComputedStyle WithOverflowX(OverflowKind overflowX)
			=> WithEnums(Enums.WithOverflowX(overflowX));
		public ComputedStyle WithOverflowY(OverflowKind overflowY)
			=> WithEnums(Enums.WithOverflowY(overflowY));
		public ComputedStyle WithTableLayout(TableLayout tableLayout)
			=> WithEnums(Enums.WithTableLayout(tableLayout));
		public ComputedStyle WithTextDecoration(TextDecorationLineKind textDecoration)
			=> WithEnums(Enums.WithTextDecoration(textDecoration));
		public ComputedStyle WithVisibility(VisibilityKind visibility)
			=> WithInherited(Inherited.WithVisibility(visibility));
		public ComputedStyle WithQuotes(IEnumerable<string>? quotes)
			=> WithMiscInherited(MiscInherited.WithQuotes(quotes));
		public ComputedStyle WithVerticalAlign(VerticalAlign align, Measure alignLength)
		{
			ComputedStyle style = WithEnums(Enums.WithVerticalAlign(align));
			if (alignLength.Units != default)
				style = style.WithSuperRare(SuperRare.WithVerticalAlignLength(alignLength));
			return style;
		}
		public ComputedStyle WithUnicodeBidi(UnicodeBidi unicodeBidi)
			=> WithEnums(Enums.WithUnicodeBidi(unicodeBidi));

		/// <summary>
		/// Make a "default" style for a child element.  This inherits the default-inheritable
		/// properties from the parent style, and resets the rest to defaults.
		/// </summary>
		/// <param name="parentStyle">The parent style to inherit from.</param>
		/// <returns>The new child style.</returns>
		public ComputedStyle MakeChildStyle()
			=> new ComputedStyle(ComputedEnumsStyle.Default, ComputedSizes.Default,
				ComputedBackgroundStyle.Default, ComputedBorderStyle.Default,
				Inherited,
				ComputedRareFieldsStyle.Default);
	}
}
