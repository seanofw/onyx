using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class PercentSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, double, TProp> Constructor { get; }

		public PercentSyntax(Func<TProp, double, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Percentage)
			{
				lexer.Unget(token);
				return null;
			}
			property = Constructor(property, token.Number);
			return property;
		}

		public override string ToString()
			=> "percent";
	}
}
