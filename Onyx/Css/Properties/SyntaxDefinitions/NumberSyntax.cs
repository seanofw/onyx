using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class NumberSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, double, TProp> Constructor { get; }

		public NumberSyntax(Func<TProp, double, TProp> constructor)
		{
			Constructor = constructor;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssToken token = lexer.Next();
			if (token.Kind != CssTokenKind.Number || !string.IsNullOrEmpty(token.Text))
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
