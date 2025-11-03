using Onyx.Css.Parsing;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class CountersSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, string, ListStyleType, TProp> Constructor { get; }

		public CountersSyntax(Func<TProp, string, string, ListStyleType, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition start = lexer.Here();

			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Func
				|| token.Text != "counters")
				goto fail;

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
				goto fail;
			string name = token.Text!;

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.String)
				goto fail;
			string separator = token.Text!;

			SkipWhitespace(lexer);

			ListStyleType style = default;
			if ((token = lexer.Next()).Kind == CssTokenKind.Comma)
			{
				SkipWhitespace(lexer);

				if ((token = lexer.Next()).Kind != CssTokenKind.Ident
					|| !CounterSyntax.EnumLookup.TryGetValue(token.Text!, out style))
					goto fail;

				SkipWhitespace(lexer);
			}

			if ((token = lexer.Next()).Kind != CssTokenKind.RightParen)
				goto fail;

			property = Constructor(property, name, separator, style);
			return property;

		fail:
			lexer.Rewind(start);
			return null;
		}

		public override string ToString()
			=> "<counters>";
	}
}
