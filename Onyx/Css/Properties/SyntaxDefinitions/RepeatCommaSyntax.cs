using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class RepeatCommaSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Syntax<TProp> Syntax { get; }
		public bool IsOptional { get; }

		public RepeatCommaSyntax(Syntax<TProp> syntax, bool isOptional)
		{
			Syntax = syntax;
			IsOptional = isOptional;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition position = lexer.Here();

			bool isFirst = true;

			CssToken token;
			do
			{
				SkipWhitespace(lexer);

				TProp? nextProperty = Syntax.Parse(lexer, property);
				if (nextProperty == null)
				{
					lexer.Rewind(position);
					return isFirst && IsOptional ? property : null;
				}

				SkipWhitespace(lexer);

				isFirst = false;

			} while ((token = lexer.Next()).Kind == CssTokenKind.Comma);

			lexer.Unget(token);

			return property;
		}

		public override string ToString()
			=> "[ " + Syntax.ToString() + (IsOptional ? " ]#" : " ]#?");
	}
}
