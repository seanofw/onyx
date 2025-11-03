using Onyx.Css.Parsing;
using Onyx.Css.Properties.KnownProperties;
using Onyx.Css.Types;

namespace Onyx.Css.Properties
{
	internal static partial class PropertySyntaxDefinitions
	{
		static PropertySyntaxDefinitions()
		{
			Syntaxes = new Dictionary<KnownPropertyKind, MiniParser>
			{
				{ KnownPropertyKind.AlignContent,
					// flex-start | flex-end | center | space-between | space-around | stretch
					DefineSyntax<AlignContentProperty>(x =>
						x.Enum<AlignContentKind>((p, e) => p with { AlignContent = e })
					)
				},

				{ KnownPropertyKind.AlignItems,
					// flex-start | flex-end | center | baseline | stretch 
					DefineSyntax<AlignItemsProperty>(x =>
						x.Enum<AlignItemsKind>((p, e) => p with { AlignItems = e })
					)
				},

				{ KnownPropertyKind.AlignSelf,
					// auto | flex-start | flex-end | center | baseline | stretch 
					DefineSyntax<AlignSelfProperty>(x =>
						x.Enum<AlignSelfKind>((p, e) => p with { AlignSelf = e })
					)
				},

				// KnownPropertyKind.Azimuth is deprecated after CSS 2.1 and is therefore omitted here.

				{ KnownPropertyKind.BackgroundColor, _backgroundColor },

				{ KnownPropertyKind.BackgroundAttachment,
					DefineSyntax<BackgroundAttachmentProperty>(x =>
						x.OneOrMoreWithCommas(_backgroundAttachment.Syntax)
					)
				},
				{ KnownPropertyKind.BackgroundImage,
					DefineSyntax<BackgroundImageProperty>(x =>
						x.OneOrMoreWithCommas(_backgroundImage.Syntax)
					)
				},
				{ KnownPropertyKind.BackgroundPosition,
					DefineSyntax<BackgroundPositionProperty>(x =>
						x.OneOrMoreWithCommas(_backgroundPosition.Syntax)
					)
				},
				{ KnownPropertyKind.BackgroundRepeat,
					DefineSyntax<BackgroundRepeatProperty>(x =>
						x.OneOrMoreWithCommas(_backgroundRepeat.Syntax)
					)
				},
				{ KnownPropertyKind.BackgroundOrigin,
					DefineSyntax<BackgroundOriginProperty>(x =>
						x.OneOrMoreWithCommas(_backgroundOrigin.Syntax)
					)
				},
				{ KnownPropertyKind.BackgroundSize,
					DefineSyntax<BackgroundSizeProperty>(x =>
						x.OneOrMoreWithCommas(_backgroundSize.Syntax)
					)
				},

				{ KnownPropertyKind.Background,
					// ['background-color' || 'background-image' || 'background-repeat'
					//		|| 'background-attachment' || 'background-position'] , #?
					//
					// ...with the special rule that "background color" must only appear in
					// the final stacking layer, or we have to unwind the parse entirely.
					DefineSyntax<BackgroundProperty>(x =>
						x.Sequence(
							x.ZeroOrMoreWithCommas(
								x.AnyOrder(
									x.Derive(_backgroundImage.Syntax,
										p => p.BackgroundImage ?? BackgroundImageProperty.Default,
										(p, i) => p with { BackgroundImage = i } ),

									x.Derive(_backgroundRepeat.Syntax,
										p => p.BackgroundRepeat ?? BackgroundRepeatProperty.Default,
										(p, r) => p with { BackgroundRepeat = r } ),

									x.Derive(_backgroundAttachment.Syntax,
										p => p.BackgroundAttachment ?? BackgroundAttachmentProperty.Default,
										(p, a) => p with { BackgroundAttachment = a } ),

									x.Derive(_backgroundPosition.Syntax,
										p => p.BackgroundPosition ?? BackgroundPositionProperty.Default,
										(p, pos) => p with { BackgroundPosition = pos } ),

									x.Derive(_backgroundOrigin.Syntax,
										p => p.BackgroundOrigin ?? BackgroundOriginProperty.Default,
										(p, o) => p with { BackgroundOrigin = o } ),

									x.Derive(_backgroundSize.Syntax,
										p => p.BackgroundSize ?? BackgroundSizeProperty.Default,
										(p, s) => p with { BackgroundSize = s } )
								)
							),
							x.AnyOrder(
								x.Derive(_backgroundColor.Syntax,
									p => p.BackgroundColor ?? BackgroundColorProperty.Default,
									(p, c) => p with { BackgroundColor = c } ),

								x.Derive(_backgroundImage.Syntax,
									p => p.BackgroundImage ?? BackgroundImageProperty.Default,
									(p, i) => p with { BackgroundImage = i } ),

								x.Derive(_backgroundRepeat.Syntax,
									p => p.BackgroundRepeat ?? BackgroundRepeatProperty.Default,
									(p, r) => p with { BackgroundRepeat = r } ),

								x.Derive(_backgroundAttachment.Syntax,
									p => p.BackgroundAttachment ?? BackgroundAttachmentProperty.Default,
									(p, a) => p with { BackgroundAttachment = a } ),

								x.Derive(_backgroundPosition.Syntax,
									p => p.BackgroundPosition ?? BackgroundPositionProperty.Default,
									(p, pos) => p with { BackgroundPosition = pos } ),

								x.Derive(_backgroundOrigin.Syntax,
									p => p.BackgroundOrigin ?? BackgroundOriginProperty.Default,
									(p, o) => p with { BackgroundOrigin = o } ),

								x.Derive(_backgroundSize.Syntax,
									p => p.BackgroundSize ?? BackgroundSizeProperty.Default,
									(p, s) => p with { BackgroundSize = s } )
							)
						)
					)
				},

				{ KnownPropertyKind.BorderCollapse,
					// collapse | separate
					DefineSyntax<BorderCollapseProperty>(x =>
						x.Enum<BorderCollapse>((p, c) => p with { Collapse = c })
					)
				},

				{ KnownPropertyKind.BorderColor,
					// [<color> | transparent]{1,4}
					DefineSyntax<BorderColorProperty>(x =>
						x.Range(1, 4,
							x.Color((p, c) => p.AddColor(c))
						)
					)
				},

				{ KnownPropertyKind.BorderSpacing,
					// <length> <length>?
					DefineSyntax<BorderSpacingProperty>(x =>
						x.RequiredThenOptional(
							x.Length((p, m) => p with { Length = m }),
							x.Length((p, m) => p with { Length2 = m })
						)
					)
				},

				{ KnownPropertyKind.BorderStyle,
					// <border-style>{1,4}
					DefineSyntax<BorderStyleProperty>(x =>
						x.Range(1, 4,
							x.Enum<BorderStyle>((p, s) => p.AddStyle(s)))
					)
				},

				{ KnownPropertyKind.BorderTop, MakeBorderEdgeProperty<BorderTopProperty>() },
				{ KnownPropertyKind.BorderRight, MakeBorderEdgeProperty<BorderRightProperty>() },
				{ KnownPropertyKind.BorderBottom, MakeBorderEdgeProperty<BorderBottomProperty>() },
				{ KnownPropertyKind.BorderLeft, MakeBorderEdgeProperty<BorderLeftProperty>() },

				{ KnownPropertyKind.BorderTopColor, MakeBorderEdgeColorProperty<BorderTopColorProperty>() },
				{ KnownPropertyKind.BorderRightColor, MakeBorderEdgeColorProperty<BorderRightColorProperty>() },
				{ KnownPropertyKind.BorderBottomColor, MakeBorderEdgeColorProperty<BorderBottomColorProperty>() },
				{ KnownPropertyKind.BorderLeftColor, MakeBorderEdgeColorProperty<BorderLeftColorProperty>() },

				{ KnownPropertyKind.BorderTopStyle, MakeBorderEdgeStyleProperty<BorderTopStyleProperty>() },
				{ KnownPropertyKind.BorderRightStyle, MakeBorderEdgeStyleProperty<BorderRightStyleProperty>() },
				{ KnownPropertyKind.BorderBottomStyle, MakeBorderEdgeStyleProperty<BorderBottomStyleProperty>() },
				{ KnownPropertyKind.BorderLeftStyle, MakeBorderEdgeStyleProperty<BorderLeftStyleProperty>() },

				{ KnownPropertyKind.BorderTopWidth, MakeBorderEdgeWidthProperty<BorderTopWidthProperty>() },
				{ KnownPropertyKind.BorderRightWidth, MakeBorderEdgeWidthProperty<BorderRightWidthProperty>() },
				{ KnownPropertyKind.BorderBottomWidth, MakeBorderEdgeWidthProperty<BorderBottomWidthProperty>() },
				{ KnownPropertyKind.BorderLeftWidth, MakeBorderEdgeWidthProperty<BorderLeftWidthProperty>() },

				{ KnownPropertyKind.BorderWidth,
					// <border-width>{1,4}
					DefineSyntax<BorderWidthProperty>(x =>
						x.Range(1, 4,
							x.BorderWidth((p, w) => p.AddWidth(w))
						)
					)
				},

				{ KnownPropertyKind.Border,
					// [ <border-width> || <border-style> || 'border-top-color' ]
					DefineSyntax<BorderProperty>(x =>
						x.AnyOrder(
							x.BorderWidth((p, w) => p with { BorderWidth = w }),
							x.Enum<BorderStyle>((p, s) => p with { BorderStyle = s }),
							x.Color((p, c) => p with { BorderColor = c })
						)
					)
				},

				{ KnownPropertyKind.BorderTopLeftRadius,
					DefineSyntax<BorderTopLeftRadiusProperty>(x =>
						x.Length((p, r) => p with { Radius = r })
					)
				},
				{ KnownPropertyKind.BorderTopRightRadius,
					DefineSyntax<BorderTopRightRadiusProperty>(x =>
						x.Length((p, r) => p with { Radius = r })
					)
				},
				{ KnownPropertyKind.BorderBottomLeftRadius,
					DefineSyntax<BorderBottomLeftRadiusProperty>(x =>
						x.Length((p, r) => p with { Radius = r })
					)
				},
				{ KnownPropertyKind.BorderBottomRightRadius,
					DefineSyntax<BorderBottomRightRadiusProperty>(x =>
						x.Length((p, r) => p with { Radius = r })
					)
				},
				{ KnownPropertyKind.BorderRadius,
					DefineSyntax<BorderRadiusProperty>(x =>
						x.Range(1, 4,
							x.Length((p, r) => p.AddRadius(r))
						)
					)
				},

				{ KnownPropertyKind.Bottom, MakeWidthProperty<BottomProperty>() },

				{ KnownPropertyKind.BoxShadow,
					DefineSyntax<BoxShadowProperty>(x =>
						x.OneOrMoreWithCommas(
							x.Derive(
								_shadow.Syntax,
								p => ShadowClass.Default,
								(p, s) => p.AddShadow(s)
							)
						)
					)
				},

				{ KnownPropertyKind.BoxSizing,
					// border-box | content-box
					DefineSyntax<BoxSizingProperty>(x =>
						x.Enum<BoxSizingMode>((p, s) => p with { Mode = s })
					)
				},

				{ KnownPropertyKind.CaptionSide,
					// top | bottom
					DefineSyntax<CaptionSideProperty>(x =>
						x.Enum<CaptionSide>((p, s) => p with { CaptionSide = s })
					)
				},

				{ KnownPropertyKind.Clear,
					// top | bottom
					DefineSyntax<ClearProperty>(x =>
						x.Enum<ClearMode>((p, c) => p with { ClearMode = c })
					)
				},

				{ KnownPropertyKind.Clip,
					// <shape> | auto
					DefineSyntax<ClipProperty>(x =>
						x.OneOf(
							x.Rect((p, r) => p with { Rect = r }),
							x.Keyword("auto", (p, k) => p with { Auto = true })
						)
					)
				},

				{ KnownPropertyKind.Color,
					// <color>
					DefineSyntax<ColorProperty>(x =>
						x.Color((p, c) => p with { Color = c })
					)
				},

				{ KnownPropertyKind.Content,
					// normal | none | [ <string> | <uri> | <counter> | attr(<identifier>)
					//   | open-quote | close-quote | no-open-quote | no-close-quote ]+
					DefineSyntax<ContentProperty>(x =>
						x.OneOf(
							x.Keyword("none", (p, k) => p.AddNone()),
							x.Keyword("normal", (p, k) => p.AddNormal()),
							x.OneOrMoreOf(
								x.OneOf(
									x.Enum<QuoteKind>((p, q) => p.AddQuoteKind(q)),
									x.String((p, s) => p.AddString(s)),
									x.Uri((p, u) => p.AddUri(u)),
									x.Counter((p, name, style) => p.AddCounter(name, style)),
									x.Counters((p, name, sep, style) => p.AddCounters(name, sep, style)),
									x.Attr((p, a) => p.AddAttr(a))
								)
							)
						)
					)
				},

				{ KnownPropertyKind.CounterIncrement,
					// [ <identifier> <integer>? ]+ | none
					DefineSyntax<CounterIncrementProperty>(x =>
						x.OneOf(
							x.Keyword("none", (p, k) => p with { None = true }),
							x.OneOrMoreOf(
								x.RequiredThenOptional(
									x.Ident((p, i) => p.AddCounter(i, 1)),
									x.Integer((p, i) => p.ApplyValue(i))
								)
							)
						)
					)
				},

				{ KnownPropertyKind.CounterReset,
					// [ <identifier> <integer>? ]+ | none
					DefineSyntax<CounterResetProperty>(x =>
						x.OneOf(
							x.Keyword("none", (p, k) => p with { None = true }),
							x.OneOrMoreOf(
								x.RequiredThenOptional(
									x.Ident((p, i) => p.AddCounter(i, 1)),
									x.Integer((p, i) => p.ApplyValue(i))
								)
							)
						)
					)
				},

				{ KnownPropertyKind.Cursor,
					// [ <uri> [ <number> <number> ]? ] #? <cursor-predefined>
					DefineSyntax<CursorProperty>(x =>
						x.Sequence(
							x.ZeroOrMoreOf(
								x.Sequence(
									x.Derive(_customCursor.Syntax,
										p => CustomCursor.Default,
										(p, c) => p.AddCustomCursor(c)
									),
									x.Punct(CssTokenKind.Comma)
								)
							),
							x.Enum<CursorKind>((p, k) => p with { CursorKind = k })
						)
					)
				},

				// KnownPropertyKind.CueAfter is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.CueBefore is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.Cue is deprecated after CSS 2.1 and is therefore omitted here.

				{ KnownPropertyKind.Direction,
					// ltr | rtl
					DefineSyntax<DirectionProperty>(x =>
						x.Enum<WritingDirection>((p, d) => p with { Direction = d })
					)
				},

				{ KnownPropertyKind.Display,
					// inline | block | list-item | inline-block | table | inline-table | table-row-group
					//   | table-header-group | table-footer-group | table-row | table-column-group
					//   | table-column | table-cell | table-caption | none
					DefineSyntax<DisplayProperty>(x =>
						x.Enum<DisplayKind>((p, d) => p with { Display = d })
					)
				},

				// KnownPropertyKind.Elevation is deprecated after CSS 2.1 and is therefore omitted here.

				{ KnownPropertyKind.EmptyCells,
					// show | hide
					DefineSyntax<EmptyCellsProperty>(x =>
						x.Enum<EmptyCellsMode>((p, m) => p with { EmptyCells = m })
					)
				},

				{ KnownPropertyKind.FlexBasis,
					// content | <width>
					DefineSyntax<FlexBasisProperty>(x =>
						x.OneOf(
							x.Keyword("auto", (p, k) => p with { Auto = true }),
							x.Keyword("content", (p, k) => p with { Content = true }),
							x.LengthOrPercent((p, m) => p with { Measure = m })
						)
					)
				},

				{ KnownPropertyKind.FlexDirection,
					// row | row-reverse | column | column-reverse
					DefineSyntax<FlexDirectionProperty>(x =>
						x.Enum<FlexDirection>((p, d) => p with { Direction = d })
					)
				},

				{ KnownPropertyKind.FlexFlow,
					// <'flex-direction'> || <'flex-wrap'>
					DefineSyntax<FlexFlowProperty>(x =>
						x.AnyOrder(
							x.Enum<FlexDirection>((p, d) => p with { Direction = d }),
							x.Enum<FlexWrap>((p, w) => p with { Wrap = w })
						)
					)
				},

				{ KnownPropertyKind.FlexGrow,
					// <number>
					DefineSyntax<FlexGrowProperty>(x =>
						x.Double((p, n) => p with { Grow = n })
					)
				},

				{ KnownPropertyKind.FlexShrink,
					// <number>
					DefineSyntax<FlexShrinkProperty>(x =>
						x.Double((p, n) => p with { Shrink = n })
					)
				},

				{ KnownPropertyKind.FlexWrap,
					// nowrap | wrap | wrap-reverse 
					DefineSyntax<FlexWrapProperty>(x =>
						x.Enum<FlexWrap>((p, w) => p with { Wrap = w })
					)
				},

				{ KnownPropertyKind.Flex,
					// none | [ <'flex-grow'> <'flex-shrink'>? || <'flex-basis'> ]
					DefineSyntax<FlexProperty>(x =>
						x.OneOf(
							x.Keyword("none", (p, k) => p with { None = true }),
							x.AnyOrder(
								x.OneOf(
									x.Keyword("auto", (p, k) => p with { Auto = true }),
									x.Keyword("content", (p, k) => p with { Content = true }),
									x.LengthOrPercent((p, m) => p with { Measure = m })
								),
								x.RequiredThenOptional(
									x.Double((p, n) => p with { Grow = n }),
									x.Double((p, n) => p with { Shrink = n })
								)
							)
						)
					)
				},

				{ KnownPropertyKind.Float,
					// left | right | none
					DefineSyntax<FloatProperty>(x =>
						x.Enum<FloatMode>((p, m) => p with { Float = m })
					)
				},

				{ KnownPropertyKind.FontFamily, _fontFamily },
				{ KnownPropertyKind.FontSize, _fontSize },
				{ KnownPropertyKind.FontStyle, _fontStyle },
				{ KnownPropertyKind.FontVariant, _fontVariant },
				{ KnownPropertyKind.FontWeight, _fontWeight },

				{ KnownPropertyKind.Font,
					// 	[ [ <'font-style'> || <'font-variant'> || <'font-weight'> ]?
					// 	  <'font-size'> [ / <'line-height'> ]? <'font-family'> ]
					// 	| caption | icon | menu | message-box | small-caption | status-bar
					DefineSyntax<FontProperty>(x =>
						x.OneOf(
							x.Sequence(

								// [ <'font-style'> || <'font-variant'> || <'font-weight'> ]?
								x.Optional(
									x.AnyOrder(
										x.Derive(_fontStyle.Syntax,
											p => p.FontStyle ?? FontStyleProperty.Default,
											(p, s) => p with { FontStyle = s }),
										x.Derive(_fontVariant.Syntax,
											p => p.FontVariant ?? FontVariantProperty.Default,
											(p, v) => p with { FontVariant = v }),
										x.Derive(_fontWeight.Syntax,
											p => p.FontWeight ?? FontWeightProperty.Default,
											(p, w) => p with { FontWeight = w })
									)
								),

								// <'font-size'>
								x.Derive(_fontSize.Syntax,
									p => p.FontSize ?? FontSizeProperty.Default,
									(p, s) => p with { FontSize = s }),

								// [ / <'line-height'> ]?
								x.Optional(
									x.Sequence(
										x.Punct(CssTokenKind.Slash),
										x.Derive(_lineHeight.Syntax,
											p => p.LineHeight ?? LineHeightProperty.Default,
											(p, l) => p with { LineHeight = l })
									)
								),

								// <'font-family'>
								x.Derive(_fontFamily.Syntax,
									p => p.FontFamily ?? FontFamilyProperty.Default,
									(p, f) => p with { FontFamily = f })
							),

							// caption | icon | menu | message-box | small-caption | status-bar
							x.Derive(
								DefineSyntax<SpecialFontProperty>(z =>
									z.Enum<SpecialFontKind>((p, k) => p with { SpecialFontKind = k })
								).Syntax,
								p => p.SpecialFont ?? SpecialFontProperty.Default,
								(p, s) => p with { SpecialFont = s }
							)
						)
					)
				},

				{ KnownPropertyKind.Height, MakeWidthProperty<HeightProperty>() },

				{ KnownPropertyKind.JustifyContent,
					// flex-start | flex-end | center | space-between | space-around
					DefineSyntax<JustifyContentProperty>(x =>
						x.Enum<JustifyContentKind>((p, e) => p with { JustifyContent = e })
					)
				},

				{ KnownPropertyKind.Left, MakeWidthProperty<LeftProperty>() },

				{ KnownPropertyKind.LetterSpacing,
					// normal | <length>
					DefineSyntax<LetterSpacingProperty>(x =>
						x.OneOf(
							x.Length((p, l) => p with { Length = l }),
							x.Keyword("normal", (p, k) => p with { Normal = true })
						)
					)
				},

				{ KnownPropertyKind.LineHeight, _lineHeight },

				{ KnownPropertyKind.ListStyleImage, _listStyleImage },
				{ KnownPropertyKind.ListStylePosition, _listStylePosition },
				{ KnownPropertyKind.ListStyleType, _listStyleType },

				{ KnownPropertyKind.ListStyle,
					DefineSyntax<ListStyleProperty>(x =>
						x.AnyOrder(
							x.Derive(_listStyleType.Syntax,
								p => p.Type ?? ListStyleTypeProperty.Default,
								(p, s) => p with { Type = s }
							),
							x.Derive(_listStylePosition.Syntax,
								p => p.Position ?? ListStylePositionProperty.Default,
								(p, s) => p with { Position = s }
							),
							x.Derive(_listStyleImage.Syntax,
								p => p.Image ?? ListStyleImageProperty.Default,
								(p, s) => p with { Image = s }
							)
						)
					)
				},

				{ KnownPropertyKind.MarginRight, MakeWidthProperty<MarginRightProperty>() },
				{ KnownPropertyKind.MarginLeft, MakeWidthProperty<MarginLeftProperty>() },
				{ KnownPropertyKind.MarginTop, MakeWidthProperty<MarginTopProperty>() },
				{ KnownPropertyKind.MarginBottom, MakeWidthProperty<MarginBottomProperty>() },
				{ KnownPropertyKind.Margin, MakeWidthMultiProperty<MarginProperty>() },

				{ KnownPropertyKind.Orphans,
					// <integer>
					DefineSyntax<OrphansProperty>(x =>
						x.Integer((p, i) => p with { Count = i })
					)
				},

				{ KnownPropertyKind.Order,
					// <integer>
					DefineSyntax<OrderProperty>(x =>
						x.Integer((p, i) => p with { Order = i })
					)
				},

				{ KnownPropertyKind.OutlineColor,
					// <color> | invert
					DefineSyntax<OutlineColorProperty>(x =>
						x.OneOf(
							x.Keyword("invert", (p, k) => p with { Invert = true }),
							x.Color((p, c) => p with { Color = c })
						)
					)
				},

				{ KnownPropertyKind.Outline,
					// [ <'outline-color'> || <'outline-style'> || <'outline-width'> ]
					DefineSyntax<OutlineProperty>(x =>
						x.AnyOrder(
							x.OneOf(
								x.Keyword("invert", (p, k) => p with { Invert = true }),
								x.Color((p, c) => p with { Color = c })
							),
							x.Enum<BorderStyle>((p, s) => p with { Style = s }),
							x.LengthOrPercent((p, m) => p with { Width = m })
						)
					)
				},

				{ KnownPropertyKind.OutlineStyle,
					// <border-width>
					DefineSyntax<OutlineStyleProperty>(x =>
						x.Enum<BorderStyle>((p, s) => p with { Style = s })
					)
				},

				{ KnownPropertyKind.OutlineWidth,
					// <border-width>
					DefineSyntax<OutlineWidthProperty>(x =>
						x.LengthOrPercent((p, m) => p with { Width = m })
					)
				},

				{ KnownPropertyKind.OutlineOffset,
					// <length>
					DefineSyntax<OutlineOffsetProperty>(x =>
						x.Length((p, m) => p with { Offset = m })
					)
				},

				{ KnownPropertyKind.Overflow,
					// visible | hidden | scroll | auto
					DefineSyntax<OverflowProperty>(x =>
						x.Enum<OverflowKind>((p, o) => p with { Overflow = o })
					)
				},
				{ KnownPropertyKind.OverflowX,
					// visible | hidden | scroll | auto
					DefineSyntax<OverflowXProperty>(x =>
						x.Enum<OverflowKind>((p, o) => p with { OverflowX = o })
					)
				},
				{ KnownPropertyKind.OverflowY,
					// visible | hidden | scroll | auto
					DefineSyntax<OverflowYProperty>(x =>
						x.Enum<OverflowKind>((p, o) => p with { OverflowY = o })
					)
				},

				{ KnownPropertyKind.PaddingRight, MakeWidthProperty<PaddingRightProperty>() },
				{ KnownPropertyKind.PaddingLeft, MakeWidthProperty<PaddingLeftProperty>() },
				{ KnownPropertyKind.PaddingTop, MakeWidthProperty<PaddingTopProperty>() },
				{ KnownPropertyKind.PaddingBottom, MakeWidthProperty<PaddingBottomProperty>() },
				{ KnownPropertyKind.Padding, MakeWidthMultiProperty<PaddingProperty>() },

				{ KnownPropertyKind.PageBreakAfter,
					// auto | always | avoid | left | right
					DefineSyntax<PageBreakAfterProperty>(x =>
						x.Enum<PageBreakOption>((p, b) => p with { Break = b})
					)
				},

				{ KnownPropertyKind.PageBreakBefore,
					// auto | always | avoid | left | right
					DefineSyntax<PageBreakBeforeProperty>(x =>
						x.Enum<PageBreakOption>((p, b) => p with { Break = b})
					)
				},

				{ KnownPropertyKind.PageBreakInside,
					// auto | avoid
					DefineSyntax<PageBreakInsideProperty>(x =>
						x.Enum<PageBreakInsideOption>((p, b) => p with { Break = b})
					)
				},

				// KnownPropertyKind.PauseAfter is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.PauseBefore is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.Pause is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.PitchRange is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.Pitch is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.PlayDuring is deprecated after CSS 2.1 and is therefore omitted here.

				{ KnownPropertyKind.Position,
					// static | relative | absolute | fixed | sticky
					DefineSyntax<PositionProperty>(x =>
						x.Enum<PositionKind>((p, k) => p with { Position = k })
					)
				},

				{ KnownPropertyKind.Resize,
					// none | horizontal | vertical | both
					DefineSyntax<ResizeProperty>(x =>
						x.Enum<ResizeKind>((p, k) => p with { Resize = k })
					)
				},

				{ KnownPropertyKind.Quotes,
					// [ <string> <string> ]+ | none
					DefineSyntax<QuotesProperty>(x =>
						x.OneOf(
							x.Keyword("none", (p, k) => p with { None = true }),
							x.OneOrMoreOf(
								x.Sequence(
									x.String((p, s) => p.AddQuote(s)),
									x.String((p, s) => p.AddQuote(s))
								)
							)
						)
					)
				},

				// KnownPropertyKind.Richness is deprecated after CSS 2.1 and is therefore omitted here.

				{ KnownPropertyKind.Right, MakeWidthProperty<RightProperty>() },

				// KnownPropertyKind.SpeakHeader is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.SpeakNumeral is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.SpeakPunctuation is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.Speak is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.SpeechRate is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.Stress is deprecated after CSS 2.1 and is therefore omitted here.

				{ KnownPropertyKind.TableLayout,
					DefineSyntax<TableLayoutProperty>(x =>
						x.Enum<TableLayout>((p, l) => p with { TableLayout = l })
					)
				},

				{ KnownPropertyKind.TextAlign,
					DefineSyntax<TextAlignProperty>(x =>
						x.Enum<TextAlign>((p, a) => p with { TextAlign = a })
					)
				},

				{ KnownPropertyKind.TextDecoration,
					DefineSyntax<TextDecorationProperty>(x =>
						x.OneOf(
							x.Keyword("none", (p, k) => p with { TextDecoration = TextDecorationLineKind.None }),
							x.AnyOrder(
								x.Keyword("underline", (p, k) => p with { TextDecoration =
									p.TextDecoration.GetValueOrDefault() | TextDecorationLineKind.Underline }),
								x.Keyword("overline", (p, k) => p with { TextDecoration =
									p.TextDecoration.GetValueOrDefault() | TextDecorationLineKind.Overline }),
								x.Keyword("line-through", (p, k) => p with { TextDecoration =
									p.TextDecoration.GetValueOrDefault() | TextDecorationLineKind.LineThrough }),
								x.Keyword("blink", (p, k) => p with { TextDecoration =
									p.TextDecoration.GetValueOrDefault() | TextDecorationLineKind.Blink })
							)
						)
					)
				},

				{ KnownPropertyKind.TextIndent, MakeWidthProperty<TextIndentProperty>() },

				{ KnownPropertyKind.TextShadow,
					DefineSyntax<TextShadowProperty>(x =>
						x.OneOrMoreWithCommas(
							x.Derive(
								_shadow.Syntax,
								p => ShadowClass.Default,
								(p, s) => p.AddShadow(s)
							)
						)
					)
				},

				{ KnownPropertyKind.TextTransform,
					DefineSyntax<TextTransformProperty>(x =>
						x.Enum<TextTransform>((p, t) => p with { TextTransform = t })
					)
				},

				{ KnownPropertyKind.Top, MakeWidthProperty<TopProperty>() },

				{ KnownPropertyKind.UnicodeBidi,
					DefineSyntax<UnicodeBidiProperty>(x =>
						x.Enum<UnicodeBidi>((p, u) => p with { UnicodeBidi = u })
					)
				},

				{ KnownPropertyKind.VerticalAlign,
					DefineSyntax<VerticalAlignProperty>(x =>
						x.OneOf(
							x.Enum<VerticalAlign>((p, v) => p with { VerticalAlign = v }),
							x.LengthOrPercent((p, m) => p with { VerticalAlignLength = m })
						)
					)
				},

				{ KnownPropertyKind.Visibility,
					// visible | hidden | collapse
					DefineSyntax<VisibilityProperty>(x =>
						x.Enum<VisibilityKind>((p, v) => p with { Visibility = v })
					)
				},

				// KnownPropertyKind.VoiceFamily is deprecated after CSS 2.1 and is therefore omitted here.
				// KnownPropertyKind.Volume is deprecated after CSS 2.1 and is therefore omitted here.

				{ KnownPropertyKind.WhiteSpace,
					// normal | pre | nowrap | pre-wrap | pre-line | inherit
					DefineSyntax<WhiteSpaceProperty>(x =>
						x.Enum<WhiteSpaceKind>((p, k) => p with { WhiteSpace = k })
					)
				},

				{ KnownPropertyKind.Widows,
					// <integer>
					DefineSyntax<WidowsProperty>(x =>
						x.Integer((p, i) => p with { Count = i })
					)
				},

				{ KnownPropertyKind.Width, MakeWidthProperty<WidthProperty>() },

				{ KnownPropertyKind.WordSpacing,
					// normal | <length>
					DefineSyntax<WordSpacingProperty>(x =>
						x.OneOf(
							x.Length((p, l) => p with { Length = l }),
							x.Keyword("normal", (p, k) => p with { Normal = true })
						)
					)
				},

				{ KnownPropertyKind.ZIndex,
					// auto | <integer>
					DefineSyntax<ZIndexProperty>(x =>
						x.OneOf(
							x.Integer((p, i) => p with { ZIndex = i }),
							x.Keyword("auto", (p, k) => p with { Auto = true })
						)
					)
				},
			};
		}
	}
}
