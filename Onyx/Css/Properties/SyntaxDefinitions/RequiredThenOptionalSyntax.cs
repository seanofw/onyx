using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class RequiredThenOptionalSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Syntax<TProp> Required { get; }
		public Syntax<TProp> Optional { get; }

		public RequiredThenOptionalSyntax(Syntax<TProp> required, Syntax<TProp> optional)
		{
			Required = required;
			Optional = optional;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition position = lexer.Here();

			SkipWhitespace(lexer);

			TProp? result = Required.Parse(lexer, property);
			if (result == null)
				return null;
			property = result;

			SkipWhitespace(lexer);

			result = Optional.Parse(lexer, property);
			if (result != null)
				property = result;

			return property;
		}

		public override string ToString()
			=> Required.ToString() + " " + Optional.ToString() + " ?";
	}
}
