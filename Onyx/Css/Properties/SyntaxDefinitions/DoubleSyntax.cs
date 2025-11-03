using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class DoubleSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, double, TProp> Constructor { get; }

		public DoubleSyntax(Func<TProp, double, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Number
				|| !string.IsNullOrEmpty(token.Text))
			{
				lexer.Unget(token);
				return null;
			}
			property = Constructor(property, token.Number);
			return property;
		}

		public override string ToString()
			=> "number";
	}
}
