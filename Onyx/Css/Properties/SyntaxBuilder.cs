using Onyx.Css.Properties.SyntaxDefinitions;
using System.Runtime.CompilerServices;
using Onyx.Css.Parsing;
using System;
using Onyx.Css.Types;

namespace Onyx.Css.Properties
{
	internal readonly struct SyntaxBuilder<TProp>
		where TProp : class
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Color(Func<TProp, Color32, TProp> constructor)
			=> new ColorSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Uri(Func<TProp, string, TProp> constructor)
			=> new UriSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Integer(Func<TProp, int, TProp> constructor)
			=> new IntegerSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Double(Func<TProp, double, TProp> constructor)
			=> new DoubleSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Punct(CssTokenKind kind, Func<TProp, CssToken, TProp>? constructor = null)
			=> new PunctSyntax<TProp>([kind], constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Punct(IEnumerable<CssTokenKind> kinds, Func<TProp, CssToken, TProp>? constructor = null)
			=> new PunctSyntax<TProp>(kinds, constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> String(Func<TProp, string, TProp> constructor)
			=> new StringSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Ident(Func<TProp, string, TProp> constructor)
			=> new IdentSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> IdentSequence(Func<TProp, string, TProp> constructor)
			=> new IdentSequenceSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Keyword(string keyword, Func<TProp, string, TProp> constructor)
			=> new KeywordSyntax<TProp>(keyword, constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> KeywordMulti(IEnumerable<string> keywords, Func<TProp, string, TProp> constructor)
			=> new KeywordMultiSyntax<TProp>(keywords, constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Enum<TEnum>(Func<TProp, TEnum, TProp> constructor)
			where TEnum : struct
			=> new EnumSyntax<TProp, TEnum>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Number(Func<TProp, double, TProp> constructor)
			=> new NumberSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Length(Func<TProp, Measure, TProp> constructor)
			=> new LengthSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> LengthOrPercent(Func<TProp, Measure, TProp> constructor)
			=> new LengthOrPercentSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Angle(Func<TProp, Measure, TProp> constructor)
			=> new AngleSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Time(Func<TProp, Measure, TProp> constructor)
			=> new TimeSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Frequency(Func<TProp, Measure, TProp> constructor)
			=> new FrequencySyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Rect(Func<TProp, CssRect, TProp> constructor)
			=> new RectSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Counter(Func<TProp, string, ListStyleType, TProp> constructor)
			=> new CounterSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Counters(Func<TProp, string, string, ListStyleType, TProp> constructor)
			=> new CountersSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Attr(Func<TProp, string, TProp> constructor)
			=> new AttrSyntax<TProp>(constructor);

		// thin | medium | thick | <length>
		public Syntax<TProp> BorderWidth(Func<TProp, Measure, TProp> constructor)
			=> OneOf(
				Length((p, m) => constructor(p, m)),
				KeywordMulti(["thin", "medium", "thick"], (p, keyword) => keyword switch
				{
					"thin"   => constructor(p, new Measure(Units.Pixels, 1.0)),
					"medium" => constructor(p, new Measure(Units.Pixels, 3.0)),
					"thick"  => constructor(p, new Measure(Units.Pixels, 5.0)),
					_ => throw new InvalidOperationException("Unknown border width keyword"),
				})
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> BackgroundPosition(Func<TProp, BackgroundPosition, TProp> constructor)
			=> new BackgroundPositionSyntax<TProp>(constructor);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Sequence(params Syntax<TProp>[] syntaxes)
			=> new SequenceSyntax<TProp>(syntaxes);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> RequiredThenOptional(Syntax<TProp> required, Syntax<TProp> optional)
			=> new RequiredThenOptionalSyntax<TProp>(required, optional);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> OneOf(params Syntax<TProp>[] syntaxes)
			=> new OneOfSyntax<TProp>(syntaxes, true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> OptionalOneOf(params Syntax<TProp>[] syntaxes)
			=> new OneOfSyntax<TProp>(syntaxes, false);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> AnyOrder(params Syntax<TProp>[] syntaxes)
			=> new AnyOrderSyntax<TProp>(syntaxes);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Optional(Syntax<TProp> syntax)
			=> new OptionalSyntax<TProp>(syntax);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Range(int min, int max, Syntax<TProp> syntax)
			=> new RangeSyntax<TProp>(min, max, syntax);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> OneOrMoreOf(Syntax<TProp> syntax)
			=> new RepeatSyntax<TProp>(syntax, isOptional: false);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> ZeroOrMoreOf(Syntax<TProp> syntax)
			=> new RepeatSyntax<TProp>(syntax, isOptional: true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> OneOrMoreWithCommas(Syntax<TProp> syntax)
			=> new RepeatCommaSyntax<TProp>(syntax, isOptional: false);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> ZeroOrMoreWithCommas(Syntax<TProp> syntax)
			=> new RepeatCommaSyntax<TProp>(syntax, isOptional: true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Syntax<TProp> Derive<TChildProp>(Syntax<TChildProp> syntax,
			Func<TProp, TChildProp> extract, Func<TProp, TChildProp, TProp> apply)
			where TChildProp : class
			=> new DerivedSyntax<TProp, TChildProp>(syntax, extract, apply);
	}
}
