using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class StringSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, TProp> Constructor { get; }

		public StringSyntax(Func<TProp, string, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.String)
			{
				lexer.Unget(token);
				return null;
			}
			property = Constructor(property, token.Text!);
			return property;
		}

		public override string ToString()
			=> "string";
	}
}
