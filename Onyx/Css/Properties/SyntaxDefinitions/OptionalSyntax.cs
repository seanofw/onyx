using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class OptionalSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Syntax<TProp> Syntax { get; }

		public OptionalSyntax(Syntax<TProp> syntax)
		{
			Syntax = syntax;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition position = lexer.Here();

			SkipWhitespace(lexer);

			TProp? result = Syntax.Parse(lexer, property);
			if (result != null)
				return result;

			lexer.Rewind(position);
			return property;
		}

		public override string ToString()
			=> Syntax.ToString() + " ?";
	}
}
