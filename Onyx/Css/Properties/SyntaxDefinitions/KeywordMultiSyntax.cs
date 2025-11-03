using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class KeywordMultiSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, TProp> Constructor { get; }

		public IReadOnlySet<string> Keywords { get; }

		public KeywordMultiSyntax(IEnumerable<string> keywords, Func<TProp, string, TProp> constructor)
		{
			Constructor = constructor;
			Keywords = keywords.ToHashSet();
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Ident
				|| !Keywords.Contains(token.Text!))
			{
				lexer.Unget(token);
				return null;
			}
			property = Constructor(property, token.Text!);
			return property;
		}

		public override string ToString()
			=> "[ " + string.Join(" | ", Keywords.OrderBy(k => k)) + " ]";
	}
}
