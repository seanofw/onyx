using System.Runtime.CompilerServices;
using Onyx.Css.Properties;
using Onyx.Css.Types;
using Onyx.Types;

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

		public readonly UInt256 AssignedPropertyBits;
		private KnownPropertyKind[]? _assignedProperties;

		internal ComputedFlexStyle Flex => RareFields.Flex;
		internal ComputedPageBreakStyle PageBreak => RareFields.PageBreak;
		internal ComputedOutlineStyle Outline => RareFields.Outline;
		internal ComputedTextStyle Text => Inherited.Text;
		internal ComputedFontStyle Font => Inherited.Font;
		internal ComputedTableStyle Table => Inherited.Table;
		internal ComputedListStyle List => Inherited.List;
		internal ComputedMiscInheritedStyle MiscInherited => Inherited.Misc;
		internal ComputedSuperRareFieldsStyle SuperRare => RareFields.SuperRare;

		/// <summary>
		/// The list of KnownPropertyKinds that were assigned as part of this computed style.
		/// This list is available for reference, but prefer IsPropertyAssigned() and
		/// AnyPropertiesAssigned() for testing, as those work without allocating and
		/// constructing an actual list: IsPropertyAssigned() executes in O(1) time with
		/// only a few machine instructions and no allocations.
		/// </summary>
		public IReadOnlyList<KnownPropertyKind> AssignedProperties
			=> _assignedProperties ??= ConvertBitsToKnownProperties(AssignedPropertyBits);

		/// <summary>
		/// Test to see if a specific property was assigned as part of this computed style.
		/// </summary>
		/// <param name="kind">The type of property to test.</param>
		/// <returns>True if that property was assigned in this computed style, false if it was not.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsPropertyAssigned(KnownPropertyKind kind)
			=> AssignedPropertyBits.IsBitSet((int)kind);

		/// <summary>
		/// Test to see if any of the given properties were assigned as part of this computed style.
		/// </summary>
		/// <param name="kind">The type of property to test.</param>
		/// <returns>True if any of the given properties were assigned in this computed style, false if none were.</returns>
		public bool AnyPropertiesAssigned(params KnownPropertyKind[] kinds)
		{
			foreach (KnownPropertyKind kind in kinds)
				if (AssignedPropertyBits.IsBitSet((int)kind))
					return true;
			return false;
		}

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
				ComputedInheritedStyle.Default, ComputedRareFieldsStyle.Default,
				UInt256.Zero);

		// Construction
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle(ComputedEnumsStyle enums, ComputedSizes sizes,
			ComputedBackgroundStyle background, ComputedBorderStyle border,
			ComputedInheritedStyle inherited, ComputedRareFieldsStyle rareFields,
			UInt256 assignedPropertyBits)
		{
			Enums = enums;
			Sizes = sizes;
			Background = background;
			Border = border;
			Inherited = inherited;
			RareFields = rareFields;
			AssignedPropertyBits = assignedPropertyBits;
		}

		// Reconstruction
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle(ComputedEnumsStyle enums, ComputedSizes sizes,
			ComputedBackgroundStyle background, ComputedBorderStyle border,
			ComputedInheritedStyle inherited, ComputedRareFieldsStyle rareFields,
			UInt256 assignedPropertyBits, KnownPropertyKind knownPropertyKind)
		{
			Enums = enums;
			Sizes = sizes;
			Background = background;
			Border = border;
			Inherited = inherited;
			RareFields = rareFields;
			AssignedPropertyBits = assignedPropertyBits.SetBit((int)knownPropertyKind);
		}

		// Replacing large style chunks
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithEnums(ComputedEnumsStyle enums, KnownPropertyKind knownPropertyKind)
			=> new ComputedStyle(enums, Sizes, Background, Border, Inherited, RareFields, AssignedPropertyBits, knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithSizes(ComputedSizes sizes, KnownPropertyKind knownPropertyKind)
			=> new ComputedStyle(Enums, sizes, Background, Border, Inherited, RareFields, AssignedPropertyBits, knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithSizes(ComputedSizes sizes, UInt256 newBits)
			=> new ComputedStyle(Enums, sizes, Background, Border, Inherited, RareFields, AssignedPropertyBits | newBits);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithBackground(ComputedBackgroundStyle background, KnownPropertyKind knownPropertyKind)
			=> new ComputedStyle(Enums, Sizes, background, Border, Inherited, RareFields, AssignedPropertyBits, knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithBackground(ComputedBackgroundStyle background, UInt256 newBits)
			=> new ComputedStyle(Enums, Sizes, background, Border, Inherited, RareFields, AssignedPropertyBits | newBits);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithBorder(ComputedBorderStyle border, KnownPropertyKind knownPropertyKind)
			=> new ComputedStyle(Enums, Sizes, Background, border, Inherited, RareFields, AssignedPropertyBits, knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithBorder(ComputedBorderStyle border, UInt256 newBits)
			=> new ComputedStyle(Enums, Sizes, Background, border, Inherited, RareFields, AssignedPropertyBits | newBits);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithInherited(ComputedInheritedStyle inherited, KnownPropertyKind knownPropertyKind)
			=> new ComputedStyle(Enums, Sizes, Background, Border, inherited, RareFields, AssignedPropertyBits, knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithRareFields(ComputedRareFieldsStyle rareFields, KnownPropertyKind knownPropertyKind)
			=> new ComputedStyle(Enums, Sizes, Background, Border, Inherited, rareFields, AssignedPropertyBits, knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedStyle WithRareFields(ComputedRareFieldsStyle rareFields, UInt256 newBits)
			=> new ComputedStyle(Enums, Sizes, Background, Border, Inherited, rareFields, AssignedPropertyBits | newBits);

		// Rare-field chunks
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithFlex(ComputedFlexStyle flex, KnownPropertyKind knownPropertyKind)
			=> WithRareFields(RareFields.WithFlex(flex), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithPageBreak(ComputedPageBreakStyle @break, KnownPropertyKind knownPropertyKind)
			=> WithRareFields(RareFields.WithPageBreak(@break), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithOutline(ComputedOutlineStyle outline, KnownPropertyKind knownPropertyKind)
			=> WithRareFields(RareFields.WithOutline(outline), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithSuperRare(ComputedSuperRareFieldsStyle superRare, KnownPropertyKind knownPropertyKind)
			=> WithRareFields(RareFields.WithSuperRare(superRare), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithSuperRare(ComputedSuperRareFieldsStyle superRare, UInt256 newBits)
			=> WithRareFields(RareFields.WithSuperRare(superRare), newBits);

		// Large inherited chunks
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithText(ComputedTextStyle text, KnownPropertyKind knownPropertyKind)
			=> WithInherited(Inherited.WithText(text), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithFont(ComputedFontStyle font, KnownPropertyKind knownPropertyKind)
			=> WithInherited(Inherited.WithFont(font), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithTable(ComputedTableStyle table, KnownPropertyKind knownPropertyKind)
			=> WithInherited(Inherited.WithTable(table), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithList(ComputedListStyle list, KnownPropertyKind knownPropertyKind)
			=> WithInherited(Inherited.WithList(list), knownPropertyKind);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedStyle WithMiscInherited(ComputedMiscInheritedStyle miscInherited, KnownPropertyKind knownPropertyKind)
			=> WithInherited(Inherited.WithMisc(miscInherited), knownPropertyKind);

		// Common style
		public ComputedStyle WithDisplay(DisplayKind display)
			=> WithEnums(Enums.WithDisplay(display), KnownPropertyKind.Display);
		public ComputedStyle WithPosition(PositionKind position)
			=> WithEnums(Enums.WithPosition(position), KnownPropertyKind.Position);

		// Box offsets
		public ComputedStyle WithBottom(Measure bottom)
			=> WithSizes(Sizes.WithBottom(bottom), KnownPropertyKind.Bottom);
		public ComputedStyle WithLeft(Measure left)
			=> WithSizes(Sizes.WithLeft(left), KnownPropertyKind.Left);
		public ComputedStyle WithRight(Measure right)
			=> WithSizes(Sizes.WithRight(right), KnownPropertyKind.Right);
		public ComputedStyle WithTop(Measure top)
			=> WithSizes(Sizes.WithTop(top), KnownPropertyKind.Top);

		// Dimensions
		public ComputedStyle WithWidth(Measure width)
			=> WithSizes(Sizes.WithWidth(width), KnownPropertyKind.Width);
		public ComputedStyle WithHeight(Measure height)
			=> WithSizes(Sizes.WithHeight(height), KnownPropertyKind.Height);
		public ComputedStyle WithMinWidth(Measure minWidth)
			=> WithSizes(Sizes.WithMinWidth(minWidth), KnownPropertyKind.MinWidth);
		public ComputedStyle WithMinHeight(Measure minHeight)
			=> WithSizes(Sizes.WithMinHeight(minHeight), KnownPropertyKind.MinHeight);
		public ComputedStyle WithMaxWidth(Measure maxWidth)
			=> WithSizes(Sizes.WithMaxWidth(maxWidth), KnownPropertyKind.MaxWidth);
		public ComputedStyle WithMaxHeight(Measure maxHeight)
			=> WithSizes(Sizes.WithMaxHeight(maxHeight), KnownPropertyKind.MaxHeight);

		// Padding
		private static readonly UInt256 _allPadding = UInt256.Zero
			.SetBit((int)KnownPropertyKind.PaddingTop)
			.SetBit((int)KnownPropertyKind.PaddingRight)
			.SetBit((int)KnownPropertyKind.PaddingBottom)
			.SetBit((int)KnownPropertyKind.PaddingLeft);
		public ComputedStyle WithPadding(EdgeSizes edgeSizes)
			=> WithSizes(Sizes.WithPadding(edgeSizes), _allPadding);
		public ComputedStyle WithPaddingBottom(Measure paddingBottom)
			=> WithSizes(Sizes.WithPaddingBottom(paddingBottom), KnownPropertyKind.PaddingBottom);
		public ComputedStyle WithPaddingLeft(Measure paddingLeft)
			=> WithSizes(Sizes.WithPaddingLeft(paddingLeft), KnownPropertyKind.PaddingLeft);
		public ComputedStyle WithPaddingRight(Measure paddngRight)
			=> WithSizes(Sizes.WithPaddingRight(paddngRight), KnownPropertyKind.PaddingRight);
		public ComputedStyle WithPaddingTop(Measure paddingTop)
			=> WithSizes(Sizes.WithPaddingTop(paddingTop), KnownPropertyKind.PaddingTop);

		// Margin
		private static readonly UInt256 _allMargins = UInt256.Zero
			.SetBit((int)KnownPropertyKind.MarginTop)
			.SetBit((int)KnownPropertyKind.MarginRight)
			.SetBit((int)KnownPropertyKind.MarginBottom)
			.SetBit((int)KnownPropertyKind.MarginLeft);
		public ComputedStyle WithMargin(EdgeSizes edgeSizes)
			=> WithSizes(Sizes.WithMargin(edgeSizes), _allMargins);
		public ComputedStyle WithMarginBottom(Measure marginBottom)
			=> WithSizes(Sizes.WithMarginBottom(marginBottom), KnownPropertyKind.MarginBottom);
		public ComputedStyle WithMarginLeft(Measure marginLeft)
			=> WithSizes(Sizes.WithMarginLeft(marginLeft), KnownPropertyKind.MarginLeft);
		public ComputedStyle WithMarginRight(Measure marginRight)
			=> WithSizes(Sizes.WithMarginRight(marginRight), KnownPropertyKind.MarginRight);
		public ComputedStyle WithMarginTop(Measure marginTop)
			=> WithSizes(Sizes.WithMarginTop(marginTop), KnownPropertyKind.MarginTop);

		// Flex properties
		public ComputedStyle WithAlignContent(AlignContentKind alignContent)
			=> WithFlex(Flex.WithAlignContent(alignContent), KnownPropertyKind.AlignContent);
		public ComputedStyle WithAlignItems(AlignItemsKind alignItems)
			=> WithFlex(Flex.WithAlignItems(alignItems), KnownPropertyKind.AlignItems);
		public ComputedStyle WithAlignSelf(AlignSelfKind alignSelf)
			=> WithFlex(Flex.WithAlignSelf(alignSelf), KnownPropertyKind.AlignSelf);
		public ComputedStyle WithFlexBasis(Measure basis)
			=> WithFlex(Flex.WithBasis(basis), KnownPropertyKind.FlexBasis);
		public ComputedStyle WithFlexDirection(FlexDirection direction)
			=> WithFlex(Flex.WithDirection(direction), KnownPropertyKind.FlexDirection);
		public ComputedStyle WithFlexGrow(double grow)
			=> WithFlex(Flex.WithGrow(grow), KnownPropertyKind.FlexGrow);
		public ComputedStyle WithFlexShrink(double shrink)
			=> WithFlex(Flex.WithShrink(shrink), KnownPropertyKind.FlexShrink);
		public ComputedStyle WithFlexWrap(FlexWrap wrap)
			=> WithFlex(Flex.WithWrap(wrap), KnownPropertyKind.FlexWrap);
		public ComputedStyle WithJustifyContent(JustifyContentKind justifyContent)
			=> WithFlex(Flex.WithJustifyContent(justifyContent), KnownPropertyKind.JustifyContent);
		public ComputedStyle WithOrder(int order)
			=> WithFlex(Flex.WithOrder(order), KnownPropertyKind.Order);

		// Miscellaneous inherited
		public ComputedStyle WithWidows(int count)
			=> WithMiscInherited(MiscInherited.WithWidows(count), KnownPropertyKind.Widows);
		public ComputedStyle WithOrphans(int count)
			=> WithMiscInherited(MiscInherited.WithOrphans(count), KnownPropertyKind.Orphans);
		public ComputedStyle WithCursor(CursorKind cursorKind, IEnumerable<CustomCursor>? customCursors = null)
			=> WithMiscInherited(MiscInherited.WithCursor(cursorKind, customCursors), KnownPropertyKind.Cursor);

		// Page-break properties
		public ComputedStyle WithPageBreakAfter(PageBreakOption option)
			=> WithPageBreak(PageBreak.WithBreakAfter(option), KnownPropertyKind.PageBreakAfter);
		public ComputedStyle WithPageBreakBefore(PageBreakOption option)
			=> WithPageBreak(PageBreak.WithBreakBefore(option), KnownPropertyKind.PageBreakBefore);
		public ComputedStyle WithPageBreakInside(PageBreakInsideOption option)
			=> WithPageBreak(PageBreak.WithBreakInside(option), KnownPropertyKind.PageBreakInside);

		// Text-layout properties
		public ComputedStyle WithTextAlign(TextAlign align)
			=> WithText(Text.WithTextAlign(align), KnownPropertyKind.TextAlign);
		public ComputedStyle WithDirection(WritingDirection direction)
			=> WithText(Text.WithDirection(direction), KnownPropertyKind.Direction);
		public ComputedStyle WithWordSpacing(Measure spacing)
			=> WithText(Text.WithWordSpacing(spacing), KnownPropertyKind.WordSpacing);
		public ComputedStyle WithTextIndent(Measure indent)
			=> WithText(Text.WithTextIndent(indent), KnownPropertyKind.TextIndent);
		public ComputedStyle WithWhiteSpace(WhiteSpaceKind whiteSpace)
			=> WithText(Text.WithWhiteSpace(whiteSpace), KnownPropertyKind.WhiteSpace);
		public ComputedStyle WithLetterSpacing(Measure spacing)
			=> WithText(Text.WithLetterSpacing(spacing), KnownPropertyKind.LetterSpacing);
		public ComputedStyle WithLineHeight(Measure lineHeight)
			=> WithText(Text.WithLineHeight(lineHeight), KnownPropertyKind.LineHeight);
		public ComputedStyle WithTextTransform(TextTransform textTransform)
			=> WithText(Text.WithTextTransform(textTransform), KnownPropertyKind.TextTransform);

		// Borders
		private static readonly UInt256 _allBorderColors = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderTopColor)
			.SetBit((int)KnownPropertyKind.BorderRightColor)
			.SetBit((int)KnownPropertyKind.BorderBottomColor)
			.SetBit((int)KnownPropertyKind.BorderLeftColor);
		public ComputedStyle WithBorderColors(Color32 top, Color32 right, Color32 bottom, Color32 left)
			=> WithBorder(Border.WithColors(top, right, bottom, left), _allBorderColors);
		public ComputedStyle WithBorderTopColor(Color32 color)
			=> WithBorder(Border.WithTopColor(color), KnownPropertyKind.BorderTopColor);
		public ComputedStyle WithBorderRightColor(Color32 color)
			=> WithBorder(Border.WithRightColor(color), KnownPropertyKind.BorderRightColor);
		public ComputedStyle WithBorderBottomColor(Color32 color)
			=> WithBorder(Border.WithBottomColor(color), KnownPropertyKind.BorderBottomColor);
		public ComputedStyle WithBorderLeftColor(Color32 color)
			=> WithBorder(Border.WithLeftColor(color), KnownPropertyKind.BorderLeftColor);

		private static readonly UInt256 _allBorderStyles = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderTopStyle)
			.SetBit((int)KnownPropertyKind.BorderRightStyle)
			.SetBit((int)KnownPropertyKind.BorderBottomStyle)
			.SetBit((int)KnownPropertyKind.BorderLeftStyle);
		public ComputedStyle WithBorderStyles(BorderStyle top, BorderStyle right, BorderStyle bottom, BorderStyle left)
			=> WithBorder(Border.WithStyles(top, right, bottom, left), _allBorderStyles);
		public ComputedStyle WithBorderTopStyle(BorderStyle style)
			=> WithBorder(Border.WithTopStyle(style), KnownPropertyKind.BorderTopStyle);
		public ComputedStyle WithBorderRightStyle(BorderStyle style)
			=> WithBorder(Border.WithRightStyle(style), KnownPropertyKind.BorderRightStyle);
		public ComputedStyle WithBorderBottomStyle(BorderStyle style)
			=> WithBorder(Border.WithBottomStyle(style), KnownPropertyKind.BorderBottomStyle);
		public ComputedStyle WithBorderLeftStyle(BorderStyle style)
			=> WithBorder(Border.WithLeftStyle(style), KnownPropertyKind.BorderLeftStyle);

		private static readonly UInt256 _allBorderWidths = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderTopWidth)
			.SetBit((int)KnownPropertyKind.BorderRightWidth)
			.SetBit((int)KnownPropertyKind.BorderBottomWidth)
			.SetBit((int)KnownPropertyKind.BorderLeftWidth);
		public ComputedStyle WithBorderWidths(Measure top, Measure right, Measure bottom, Measure left)
			=> WithBorder(Border.WithWidths(top, right, bottom, left), _allBorderWidths);
		public ComputedStyle WithBorderTopWidth(Measure width)
			=> WithBorder(Border.WithTopWidth(width), KnownPropertyKind.BorderTopWidth);
		public ComputedStyle WithBorderRightWidth(Measure width)
			=> WithBorder(Border.WithRightWidth(width), KnownPropertyKind.BorderRightWidth);
		public ComputedStyle WithBorderBottomWidth(Measure width)
			=> WithBorder(Border.WithBottomWidth(width), KnownPropertyKind.BorderBottomWidth);
		public ComputedStyle WithBorderLeftWidth(Measure width)
			=> WithBorder(Border.WithLeftWidth(width), KnownPropertyKind.BorderLeftWidth);

		private static readonly UInt256 _allBorderRadii = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderTopLeftRadius)
			.SetBit((int)KnownPropertyKind.BorderTopRightRadius)
			.SetBit((int)KnownPropertyKind.BorderBottomRightRadius)
			.SetBit((int)KnownPropertyKind.BorderBottomLeftRadius);
		public ComputedStyle WithBorderRadii(Measure topLeft, Measure topRight, Measure bottomLeft, Measure bottomRight)
			=> WithBorder(Border.WithRadii(topLeft, topRight, bottomLeft, bottomRight), _allBorderRadii);
		public ComputedStyle WithBorderTopLeftRadius(Measure radius)
			=> WithBorder(Border.WithTopLeftRadius(radius), KnownPropertyKind.BorderTopLeftRadius);
		public ComputedStyle WithBorderTopRightRadius(Measure radius)
			=> WithBorder(Border.WithTopRightRadius(radius), KnownPropertyKind.BorderTopRightRadius);
		public ComputedStyle WithBorderBottomLeftRadius(Measure radius)
			=> WithBorder(Border.WithBottomLeftRadius(radius), KnownPropertyKind.BorderBottomLeftRadius);
		public ComputedStyle WithBorderBottomRightRadius(Measure radius)
			=> WithBorder(Border.WithBottomRightRadius(radius), KnownPropertyKind.BorderBottomRightRadius);

		private static readonly UInt256 _allBorderTop = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderTopColor)
			.SetBit((int)KnownPropertyKind.BorderTopStyle)
			.SetBit((int)KnownPropertyKind.BorderTopWidth);
		private static readonly UInt256 _allBorderRight = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderRightColor)
			.SetBit((int)KnownPropertyKind.BorderRightStyle)
			.SetBit((int)KnownPropertyKind.BorderRightWidth);
		private static readonly UInt256 _allBorderBottom = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderBottomColor)
			.SetBit((int)KnownPropertyKind.BorderBottomStyle)
			.SetBit((int)KnownPropertyKind.BorderBottomWidth);
		private static readonly UInt256 _allBorderLeft = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BorderLeftColor)
			.SetBit((int)KnownPropertyKind.BorderLeftStyle)
			.SetBit((int)KnownPropertyKind.BorderLeftWidth);
		public ComputedStyle WithBorderTop(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithTop(borderEdgeStyle), _allBorderTop);
		public ComputedStyle WithBorderRight(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithRight(borderEdgeStyle), _allBorderRight);
		public ComputedStyle WithBorderBottom(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithBottom(borderEdgeStyle), _allBorderBottom);
		public ComputedStyle WithBorderLeft(ComputedBorderEdgeStyle borderEdgeStyle)
			=> WithBorder(Border.WithLeft(borderEdgeStyle), _allBorderLeft);

		// Outlines
		public ComputedStyle WithOutlineWidth(Measure width)
			=> WithOutline(Outline.WithWidth(width), KnownPropertyKind.OutlineWidth);
		public ComputedStyle WithOutlineOffset(Measure offset)
			=> WithOutline(Outline.WithOffset(offset), KnownPropertyKind.OutlineOffset);
		public ComputedStyle WithOutlineColor(Color32 color, bool invert)
			=> WithOutline(Outline.WithColor(color, invert), KnownPropertyKind.OutlineColor);
		public ComputedStyle WithOutlineStyle(BorderStyle style)
			=> WithOutline(Outline.WithStyle(style), KnownPropertyKind.OutlineStyle);

		// Backgrounds
		public ComputedStyle WithBackgroundColor(Color32 color)
			=> WithBackground(Background.WithColor(color), KnownPropertyKind.BackgroundColor);
		private static readonly UInt256 _allBackgroundLayers = UInt256.Zero
			.SetBit((int)KnownPropertyKind.BackgroundAttachment)
			.SetBit((int)KnownPropertyKind.BackgroundImage)
			.SetBit((int)KnownPropertyKind.BackgroundOrigin)
			.SetBit((int)KnownPropertyKind.BackgroundPosition)
			.SetBit((int)KnownPropertyKind.BackgroundRepeat)
			.SetBit((int)KnownPropertyKind.BackgroundSize);
		public ComputedStyle WithBackgroundLayers(IEnumerable<ComputedBackgroundLayer>? layers)
			=> WithBackground(Background.WithLayers(layers), _allBackgroundLayers);
		public ComputedStyle WithBoxShadows(IEnumerable<Shadow>? shadows)
			=> WithBackground(Background.WithBoxShadows(shadows), KnownPropertyKind.BoxShadow);
		public ComputedStyle WithOpacity(double opacity)
			=> WithBackground(Background.WithOpacity(opacity), KnownPropertyKind.Opacity);

		// Font stuff
		public ComputedStyle WithColor(Color32 color)
			=> WithFont(Font.WithColor(color), KnownPropertyKind.Color);
		public ComputedStyle WithFontFamilies(IEnumerable<FontFamily>? famililies)
			=> WithFont(Font.WithFamilies(famililies), KnownPropertyKind.FontFamily);
		public ComputedStyle WithSpecialFont(SpecialFontKind specialFont)
			=> WithFont(Font.WithSpecialFont(specialFont), KnownPropertyKind.SpecialFont);
		public ComputedStyle WithFontSize(Measure size)
			=> WithFont(Font.WithSize(size), KnownPropertyKind.FontSize);
		public ComputedStyle WithFontStyle(FontStyle style)
			=> WithFont(Font.WithStyle(style), KnownPropertyKind.FontStyle);
		public ComputedStyle WithFontWeight(int weight)
			=> WithFont(Font.WithWeight(weight), KnownPropertyKind.FontWeight);
		public ComputedStyle WithFontVariant(FontVariant variant)
			=> WithFont(Font.WithVariant(variant), KnownPropertyKind.FontVariant);
		public ComputedStyle WithTextShadows(IEnumerable<Shadow>? shadows)
			=> WithFont(Font.WithTextShadows(shadows), KnownPropertyKind.TextShadow);

		// Tables
		public ComputedStyle WithBorderCollapse(BorderCollapse borderCollapse)
			=> WithTable(Table.WithBorderCollapse(borderCollapse), KnownPropertyKind.BorderCollapse);
		public ComputedStyle WithEmptyCells(EmptyCellsMode emptyCells)
			=> WithTable(Table.WithEmptyCells(emptyCells), KnownPropertyKind.EmptyCells);
		public ComputedStyle WithCaptionSide(CaptionSide captionSide)
			=> WithTable(Table.WithCaptionSide(captionSide), KnownPropertyKind.CaptionSide);
		public ComputedStyle WithBorderSpacing(Measure spacing)
			=> WithTable(Table.WithBorderSpacing(spacing), KnownPropertyKind.BorderSpacing);
		public ComputedStyle WithBorderSpacing(Measure spacingX, Measure spacingY)
			=> WithTable(Table.WithBorderSpacing(spacingX, spacingY), KnownPropertyKind.BorderSpacing);

		// Lists
		public ComputedStyle WithListStyleUri(string? uri)
			=> WithList(List.WithUri(uri), KnownPropertyKind.ListStyleImage);
		public ComputedStyle WithListStylePosition(ListStylePosition position)
			=> WithList(List.WithPosition(position), KnownPropertyKind.ListStylePosition);
		public ComputedStyle WithListStyleType(ListStyleType type)
			=> WithList(List.WithType(type), KnownPropertyKind.ListStyleType);

		// Rare-ish properties
		public ComputedStyle WithZIndex(int zIndex)
			=> WithRareFields(RareFields.WithZIndex(zIndex), KnownPropertyKind.ZIndex);

		// Super-rare properties
		public ComputedStyle WithClip(CssRect? clip)
			=> WithSuperRare(SuperRare.WithClip(clip), KnownPropertyKind.Clip);
		public ComputedStyle WithContentPieces(IEnumerable<ContentPiece>? contentPieces)
			=> WithSuperRare(SuperRare.WithContent(contentPieces), KnownPropertyKind.Content);
		public ComputedStyle WithCounterIncrements(IEnumerable<Counter>? counters)
			=> WithSuperRare(SuperRare.WithCounterIncrements(counters), KnownPropertyKind.CounterIncrement);
		public ComputedStyle WithCounterResets(IEnumerable<Counter>? counters)
			=> WithSuperRare(SuperRare.WithCounterResets(counters), KnownPropertyKind.CounterReset);

		// Enums
		public ComputedStyle WithClear(ClearMode clear)
			=> WithEnums(Enums.WithClear(clear), KnownPropertyKind.Clear);
		public ComputedStyle WithFloat(FloatMode @float)
			=> WithEnums(Enums.WithFloat(@float), KnownPropertyKind.Float);
		public ComputedStyle WithResize(ResizeKind resize)
			=> WithEnums(Enums.WithResize(resize), KnownPropertyKind.Resize);
		public ComputedStyle WithBoxSizing(BoxSizingMode boxSizing)
			=> WithEnums(Enums.WithBoxSizing(boxSizing), KnownPropertyKind.BoxSizing);
		public ComputedStyle WithOverflowX(OverflowKind overflowX)
			=> WithEnums(Enums.WithOverflowX(overflowX), KnownPropertyKind.OverflowX);
		public ComputedStyle WithOverflowY(OverflowKind overflowY)
			=> WithEnums(Enums.WithOverflowY(overflowY), KnownPropertyKind.OverflowY);
		public ComputedStyle WithTableLayout(TableLayout tableLayout)
			=> WithEnums(Enums.WithTableLayout(tableLayout), KnownPropertyKind.TableLayout);
		public ComputedStyle WithTextDecoration(TextDecorationLineKind textDecoration)
			=> WithEnums(Enums.WithTextDecoration(textDecoration), KnownPropertyKind.TextDecoration);
		public ComputedStyle WithVisibility(VisibilityKind visibility)
			=> WithInherited(Inherited.WithVisibility(visibility), KnownPropertyKind.Visibility);
		public ComputedStyle WithQuotes(IEnumerable<string>? quotes)
			=> WithMiscInherited(MiscInherited.WithQuotes(quotes), KnownPropertyKind.Quotes);
		public ComputedStyle WithVerticalAlign(VerticalAlign align, Measure alignLength)
		{
			ComputedStyle style = WithEnums(Enums.WithVerticalAlign(align), KnownPropertyKind.VerticalAlign);
			if (alignLength.Units != default)
				style = style.WithSuperRare(SuperRare.WithVerticalAlignLength(alignLength), UInt256.Zero);
			return style;
		}
		public ComputedStyle WithUnicodeBidi(UnicodeBidi unicodeBidi)
			=> WithEnums(Enums.WithUnicodeBidi(unicodeBidi), KnownPropertyKind.UnicodeBidi);

		/// <summary>
		/// Convert a set of bits where each bit represents a KnownPropertyKind enum
		/// value into an actual array of KnownPropertyKind values.
		/// </summary>
		/// <param name="bits">The set of bits describing which properties are included and which are not.</param>
		/// <returns>An array of KnownPropertyKind enum values.</returns>
		public static KnownPropertyKind[] ConvertBitsToKnownProperties(UInt256 bits)
		{
			if (bits.IsZero)
				return Array.Empty<KnownPropertyKind>();

			KnownPropertyKind[] kinds = new KnownPropertyKind[bits.PopCount()];

			int start = bits.TrailingZeroCount();
			int end = 256 - bits.LeadingZeroCount();

			int dest = 0;
			for (int i = start; i < end; i++)
			{
				if (bits.IsBitSet(i))
					kinds[dest++] = (KnownPropertyKind)i;
			}

			return kinds;
		}

		/// <summary>
		/// Convert a collection of KnownPropertyKind enum values to a compact bit set
		/// representing those same enum values.
		/// </summary>
		/// <param name="kinds">The set of property kinds.</param>
		/// <returns>A bit set that represents that same set.</returns>
		public static UInt256 ConvertKnownPropertiesToBits(IEnumerable<KnownPropertyKind> kinds)
		{
			UInt256 bits = default;
			foreach (KnownPropertyKind kind in kinds)
				bits = bits.SetBit((int)kind);
			return bits;
		}

		/// <summary>
		/// Compare this style to another, and indicate which properties, if any, are
		/// different by adding their KnownPropertyKind(s) to the provided changes collection.
		/// This does a proper deep comparison:  Every property value is tested (but subtrees
		/// are compared by pointer where possible to make it as efficient as possible).
		/// </summary>
		/// <param name="other">The other style to compare to.</param>
		/// <returns>A bit collection that describes which properties have changed, where
		/// each bit position represents an entry in the KnownPropertyKind enum.</param>
		public UInt256 Diff(ComputedStyle other)
		{
			if (this == other)
				return default;

			UInt256 bits = default;
			bits |= Enums.Diff(other.Enums);
			bits |= Sizes.Diff(other.Sizes);
			bits |= Background.Diff(other.Background);
			bits |= Border.Diff(other.Border);
			bits |= Inherited.Diff(other.Inherited);
			bits |= RareFields.Diff(other.RareFields);
			return bits;
		}

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
				ComputedRareFieldsStyle.Default,
				UInt256.Zero);
	}
}
