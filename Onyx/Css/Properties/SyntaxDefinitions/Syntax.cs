using System.Runtime.CompilerServices;
using Onyx.Css.Parsing;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal abstract class Syntax
	{
		public Type PropertyType { get; }

		protected Syntax(Type propertyType)
		{
			PropertyType = propertyType;
		}

		public abstract object? OuterParse(CssLexer lexer, object? property);

		/// <summary>
		/// Support method:  Skip optional whitespace.
		/// </summary>
		/// <param name="lexer">The lexer to eat whitespace tokens from.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static void SkipWhitespace(CssLexer lexer)
		{
			CssToken token;
			while ((token = lexer.Next()).Kind == CssTokenKind.Space) ;
			lexer.Unget(token);
		}

		protected static Measure? ReadLengthOrPercent(CssLexer lexer)
			=> ReadMeasure(lexer,
				  (1U << (int)Units.Em)
				| (1U << (int)Units.Ex)
				| (1U << (int)Units.Pixels)
				| (1U << (int)Units.Centimeters)
				| (1U << (int)Units.Millimeters)
				| (1U << (int)Units.Inches)
				| (1U << (int)Units.Points)
				| (1U << (int)Units.Picas)
				| (1U << (int)Units.Percent));

		protected static Measure? ReadLength(CssLexer lexer)
			=> ReadMeasure(lexer,
				  (1U << (int)Units.Em)
				| (1U << (int)Units.Ex)
				| (1U << (int)Units.Pixels)
				| (1U << (int)Units.Centimeters)
				| (1U << (int)Units.Millimeters)
				| (1U << (int)Units.Inches)
				| (1U << (int)Units.Points)
				| (1U << (int)Units.Picas));

		protected static Measure? ReadAngle(CssLexer lexer)
			=> ReadMeasure(lexer,
				  (1U << (int)Units.Degrees)
				| (1U << (int)Units.Radians)
				| (1U << (int)Units.Grads));

		protected static Measure? ReadTime(CssLexer lexer)
			=> ReadMeasure(lexer,
				  (1U << (int)Units.Seconds)
				| (1U << (int)Units.Milliseconds));

		protected static Measure? ReadFrequency(CssLexer lexer)
			=> ReadMeasure(lexer,
				  (1U << (int)Units.Hertz)
				| (1U << (int)Units.Kilohertz));

		protected static Measure? ReadMeasure(CssLexer lexer, uint unitMask = uint.MaxValue)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Number)
			{
				lexer.Unget(token);
				return null;
			}

			if (!Measure.SuffixToUnits.TryGetValue(token.Text ?? string.Empty, out Units units))
			{
				lexer.Unget(token);
				return null;
			}

			if ((unitMask & (1U << (int)units)) == 0)
			{
				lexer.Unget(token);
				return null;
			}

			return new Measure(units, token.Number);
		}
	}

	internal class Syntax<TProp> : Syntax
		where TProp : class
	{
		protected Syntax()
			: base(typeof(TProp))
		{
		}

		public override object? OuterParse(CssLexer lexer, object? property)
			=> Parse(lexer, (property as TProp)!);

		public virtual TProp? Parse(CssLexer lexer, TProp property)
			=> null;

		public static implicit operator Syntax<TProp>(Func<SyntaxBuilder<TProp>, Syntax<TProp>> constructor)
		{
			return constructor(new SyntaxBuilder<TProp>());
		}
	}
}
