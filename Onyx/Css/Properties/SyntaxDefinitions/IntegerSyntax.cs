using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class IntegerSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, int, TProp> Constructor { get; }

		public IntegerSyntax(Func<TProp, int, TProp> constructor)
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

			int value = (int)(Math.Min(Math.Max(token.Number, int.MinValue), int.MaxValue) + 0.5);
			property = Constructor(property, value);
			return property;
		}

		public override string ToString()
			=> "integer";
	}
}
