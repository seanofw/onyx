using System;
using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class AttrSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, TProp> Constructor { get; }

		public AttrSyntax(Func<TProp, string, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition start = lexer.Here();

			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Func
				|| token.Text != "attr")
				goto fail;

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
				goto fail;
			string name = token.Text!;

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.RightParen)
				goto fail;

			property = Constructor(property, name);
			return property;

			fail:
			lexer.Rewind(start);
			return null;
		}

		public override string ToString()
			=> "attr()";
	}
}
