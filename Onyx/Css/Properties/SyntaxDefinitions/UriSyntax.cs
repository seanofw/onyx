using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class UriSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, TProp> Constructor { get; }

		public UriSyntax(Func<TProp, string, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Url)
			{
				lexer.Unget(token);
				return null;
			}
			property = Constructor(property, token.Text!);
			return property;
		}

		public override string ToString()
			=> "uri";
	}
}
