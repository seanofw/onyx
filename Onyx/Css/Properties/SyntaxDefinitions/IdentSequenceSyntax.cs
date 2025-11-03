using System.Text;
using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class IdentSequenceSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, TProp> Constructor { get; }

		public IdentSequenceSyntax(Func<TProp, string, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			StringBuilder stringBuilder = new StringBuilder();

			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
			{
				lexer.Unget(token);
				return null;
			}
			stringBuilder.Append(token.Text!);

			while (true)
			{
				SkipWhitespace(lexer);

				if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
				{
					lexer.Unget(token);
					break;
				}

				if (stringBuilder.Length > 0)
					stringBuilder.Append(" ");

				stringBuilder.Append(token.Text!);
			}

			property = Constructor(property, stringBuilder.ToString());
			return property;
		}

		public override string ToString()
			=> "<idents>";
	}
}
