using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class KeywordSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, TProp> Constructor { get; }

		public string Keyword { get; }

		public KeywordSyntax(string keyword, Func<TProp, string, TProp> constructor)
		{
			Constructor = constructor;
			Keyword = keyword;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Ident || Keyword != token.Text)
			{
				lexer.Unget(token);
				return null;
			}
			property = Constructor(property, token.Text!);
			return property;
		}

		public override string ToString()
			=> Keyword;
	}
}
