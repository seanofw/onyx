using Onyx.Css.Parsing;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class RectSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, CssRect, TProp> Constructor { get; }

		public RectSyntax(Func<TProp, CssRect, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition start = lexer.Here();

			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Func
				|| token.Text != "rect")
				goto fail;

			SkipWhitespace(lexer);

			Measure? top = ReadLength(lexer);
			if (top == null)
				goto fail;

			SkipWhitespace(lexer);

			Measure? right = ReadLength(lexer);
			if (right == null)
				goto fail;

			SkipWhitespace(lexer);

			Measure? bottom = ReadLength(lexer);
			if (bottom == null)
				goto fail;

			SkipWhitespace(lexer);

			Measure? left = ReadLength(lexer);
			if (left == null)
				goto fail;

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.RightParen)
				goto fail;

			property = Constructor(property, new CssRect(top.Value, right.Value, bottom.Value, left.Value));
			return property;

		fail:
			lexer.Rewind(start);
			return null;
		}

		public override string ToString()
			=> "<rect>";
	}
}
