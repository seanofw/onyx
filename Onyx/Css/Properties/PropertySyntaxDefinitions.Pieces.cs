using System.Runtime.CompilerServices;
using Onyx.Css.Properties.KnownProperties;
using Onyx.Css.Properties.SyntaxDefinitions;
using Onyx.Css.Types;

namespace Onyx.Css.Properties
{
	internal static partial class PropertySyntaxDefinitions
	{
		public static IReadOnlyDictionary<KnownPropertyKind, MiniParser> Syntaxes { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static MiniParser<TProp> DefineSyntax<TProp>(
			Func<SyntaxBuilder<TProp>, Syntax<TProp>> constructor)
			where TProp : class, new()
			=> new MiniParser<TProp>(constructor(new SyntaxBuilder<TProp>()), () => new TProp());

		// scroll | fixed
		private static readonly MiniParser<BackgroundAttachmentProperty> _backgroundAttachment =
			DefineSyntax<BackgroundAttachmentProperty>(x =>
				x.Enum<BackgroundAttachment>((p, a) => p.AddAttachment(a))
			);

		// <color> | transparent
		private static readonly MiniParser<BackgroundColorProperty> _backgroundColor =
			DefineSyntax<BackgroundColorProperty>(x =>
				x.Color((p, c) => p with { Color = c })
			);

		// <uri> | none
		private static readonly MiniParser<BackgroundImageProperty> _backgroundImage =
			DefineSyntax<BackgroundImageProperty>(x =>
				x.OneOf(
					x.Uri((p, u) => p.Add(u)),
					x.Keyword("none", (p, n) => p.AddNone())
				)
			);

		// This one is so hideously complicated to get right that it requires a custom parser.
		//
		// [ left | center | right | top | bottom | <length-percentage> ]
		// | [ left | center | right | <length-percentage> ] [ top | center | bottom | <length-percentage> ]
		// | [ center | [ left | right ] <length-percentage>? ] && [ center | [ top | bottom ] <length-percentage>? ]
		private static readonly MiniParser<BackgroundPositionProperty> _backgroundPosition =
			DefineSyntax<BackgroundPositionProperty>(x =>
				x.BackgroundPosition((p, n) => p.AddPosition(n))
			);

		// repeat | repeat-x | repeat-y | no-repeat
		private static readonly MiniParser<BackgroundRepeatProperty> _backgroundRepeat =
			DefineSyntax<BackgroundRepeatProperty>(x =>
				x.Enum<BackgroundRepeat>((p, r) => p.AddRepeat(r))
			);

		// border-box | padding-box | content-box
		private static readonly MiniParser<BackgroundOriginProperty> _backgroundOrigin =
			DefineSyntax<BackgroundOriginProperty>(x =>
				x.Enum<BackgroundOrigin>((p, o) => p.AddOrigin(o))
			);

		// cover | contain | [ [<length-percentage> | auto]{1,2} ]
		private static readonly MiniParser<BackgroundSizeProperty> _backgroundSize =
			DefineSyntax<BackgroundSizeProperty>(x =>
				x.Derive(
					DefineSyntax<BackgroundSizeClass>(x =>
						x.OneOf(
							x.Enum<BackgroundSizeKind>((p, k) => p with { Kind = k }),
							x.RequiredThenOptional(
								x.OneOf(
									x.Keyword("auto", (p, k) => p with { AutoX = true }),
									x.LengthOrPercent((p, m) => p with { X = m })
								),
								x.OneOf(
									x.Keyword("auto", (p, k) => p with { AutoY = true }),
									x.LengthOrPercent((p, m) => p with { Y = m })
								)
							)
						)
					).Syntax,
					p => new BackgroundSizeClass(),
					(p, c) => p.AddSize(c)
				)
			);

		// [ <border-width> || <border-style> || 'border-color' ]
		private static MiniParser<TProp> MakeBorderEdgeProperty<TProp>()
			where TProp : BorderEdgeProperty, new()
			=> DefineSyntax<TProp>(x =>
				x.AnyOrder(
					x.BorderWidth((p, w) => p with { Width = w }),
					x.Enum<BorderStyle>((p, s) => p with { Style = s }),
					x.Color((p, c) => p with { Color = c })
				)
			);

		// <color> | transparent
		private static MiniParser<TProp> MakeBorderEdgeColorProperty<TProp>()
			where TProp : BorderEdgeColorProperty, new()
			=> DefineSyntax<TProp>(x =>
				x.Color((p, c) => p with { Color = c }));

		// <border-style>
		private static MiniParser<TProp> MakeBorderEdgeStyleProperty<TProp>()
			where TProp : BorderEdgeStyleProperty, new()
			=> DefineSyntax<TProp>(x =>
				x.Enum<BorderStyle>((p, s) => p with { Style = s }));

		// <border-width>
		private static MiniParser<TProp> MakeBorderEdgeWidthProperty<TProp>()
			where TProp : BorderEdgeWidthProperty, new()
			=> DefineSyntax<TProp>(x =>
				x.BorderWidth((p, w) => p with { Width = w }));

		// <length> | <percentage> | auto
		private static MiniParser<TProp> MakeWidthProperty<TProp>()
			where TProp : WidthPropertyBase, new()
			=> DefineSyntax<TProp>(x =>
				x.OneOf(
					x.LengthOrPercent((p, m) => p with { Width = new Width(m) }),
					x.Keyword("auto", (p, k) => p with { Width = new Width(true) })
				)
			);

		// [ <length> | <percentage> | auto ]{1,4}
		private static MiniParser<TProp> MakeWidthMultiProperty<TProp>()
			where TProp : WidthMultiPropertyBase, new()
			=> DefineSyntax<TProp>(x =>
				x.Range(1, 4,
					x.OneOf(
						x.LengthOrPercent((p, m) => (TProp)p.AddWidth(new Width(m))),
						x.Keyword("auto", (p, k) => (TProp)p.AddWidth(new Width(true)))
					)
				)
			);

		// [ <family-name> | generic-family ], [ <family-name> | generic-family ]*
		private static readonly MiniParser<FontFamilyProperty> _fontFamily =
			DefineSyntax<FontFamilyProperty>(x =>
				x.OneOrMoreOf(
					x.OneOf(
						x.Enum<GenericFontFamily>((p, k) => p.AddGenericFontFamily(k)),
						x.IdentSequence((p, n) => p.AddName(n)),
						x.String((p, n) => p.AddName(n))
					)
				)
			);

		// <absolute-size> | <relative-size> | <length> | <percentage>
		private static readonly MiniParser<FontSizeProperty> _fontSize =
			DefineSyntax<FontSizeProperty>(x =>
				x.OneOf(
					x.LengthOrPercent((p, m) => p with { Measure = m }),
					x.Enum<AbsoluteFontSize>((p, s) => p with { AbsoluteFontSize = s }),
					x.Enum<RelativeFontSize>((p, s) => p with { RelativeFontSize = s })
				)
			);

		// normal | italic | oblique
		private static readonly MiniParser<FontStyleProperty> _fontStyle =
			DefineSyntax<FontStyleProperty>(x =>
				x.Enum<FontStyle>((p, s) => p with { Style = s })
			);

		// 	normal | small-caps
		private static readonly MiniParser<FontVariantProperty> _fontVariant =
			DefineSyntax<FontVariantProperty>(x =>
				x.Enum<FontVariant>((p, v) => p with { Variant = v })
			);

		// 	normal | bold | bolder | lighter | 100 | 200 | 300 | 400 | 500 | 600 | 700 | 800 | 900
		private static readonly MiniParser<FontWeightProperty> _fontWeight =
			DefineSyntax<FontWeightProperty>(x =>
				x.OneOf(
					x.Enum<FontWeightName>((p, n) => p with { Name = n }),
					x.Integer((p, i) => p with { Amount = i })
				)
			);

		// normal | <number> | <length> | <percentage>
		private static readonly MiniParser<LineHeightProperty> _lineHeight =
			DefineSyntax<LineHeightProperty>(x =>
				x.OneOf(
					x.Keyword("normal", (p, s) => p with { Normal = true }),
					x.Double((p, n) => p with { Number = n }),
					x.LengthOrPercent((p, m) => p with { Measure = m })
				)
			);

		// <uri> | none
		private static readonly MiniParser<ListStyleImageProperty> _listStyleImage =
			DefineSyntax<ListStyleImageProperty>(x =>
				x.OneOf(
					x.Uri((p, s) => p with { Uri = s }),
					x.Keyword("none", (p, k) => p with { None = true })
				)
			);

		// inside | outside
		private static readonly MiniParser<ListStylePositionProperty> _listStylePosition =
			DefineSyntax<ListStylePositionProperty>(x =>
				x.Enum<ListStylePosition>((p, s) => p with { Position = s })
			);

		// disc | circle | square | decimal | decimal-leading-zero
		//   | lower-roman | upper-roman | lower-greek | lower-latin | upper-latin
		//   | armenian | georgian | lower-alpha | upper-alpha | none
		private static readonly MiniParser<ListStyleTypeProperty> _listStyleType =
			DefineSyntax<ListStyleTypeProperty>(x =>
				x.Enum<ListStyleType>((p, s) => p with { Style = s })
			);

		// <color>? && [ <length>{2} [ <length> <length>? ]? ] && inset?
		private static readonly MiniParser<ShadowClass> _shadow =
			DefineSyntax<ShadowClass>(x =>
				x.AnyOrder(
					x.Color((s, c) => s.WithColor(c)),
					x.Sequence(
						x.Length((s, m) => s.WithOffsetX(m)),
						x.Length((s, m) => s.WithOffsetY(m)),
						x.RequiredThenOptional(
							x.Length((s, m) => s.WithBlur(m)),
							x.Optional(
								x.Length((s, m) => s.WithSpread(m))
							)
						)
					),
					x.Keyword("inset", (s, k) => s.WithInset(true))
				)
			);

		// <uri> [ <number> <number> ]?
		private static readonly MiniParser<CustomCursor> _customCursor =
			DefineSyntax<CustomCursor>(x =>
				x.RequiredThenOptional(
					x.Uri((c, u) => c.WithUri(u)),
					x.Optional(
						x.Sequence(
							x.Number((c, x) => c.WithHotspotX(x)),
							x.Number((c, y) => c.WithHotspotX(y))
						)
					)
				)
			);
	}
}