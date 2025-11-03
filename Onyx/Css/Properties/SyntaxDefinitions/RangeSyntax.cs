using System.Globalization;
using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class RangeSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Syntax<TProp> Syntax { get; }
		public int Min { get; }
		public int Max { get; }

		public RangeSyntax(int min, int max, Syntax<TProp> syntax)
		{
			Syntax = syntax;
			Min = min;
			Max = max;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition position = lexer.Here();

			int count;
			for (count = 0; count < Max; count++)
			{
				SkipWhitespace(lexer);

				TProp? result = Syntax.Parse(lexer, property);
				if (result == null)
					break;

				property = result;
			}

			if (count < Min)
			{
				lexer.Rewind(position);
				return null;
			}

			return property;
		}

		public override string ToString()
			=> "[ " + Syntax.ToString() + " ]{"
				+ Min.ToString(CultureInfo.InvariantCulture) + ","
				+ Max.ToString(CultureInfo.InvariantCulture) + "}";
	}
}
