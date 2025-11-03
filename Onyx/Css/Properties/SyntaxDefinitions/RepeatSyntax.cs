using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class RepeatSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Syntax<TProp> Syntax { get; }
		public bool IsOptional { get; }

		public RepeatSyntax(Syntax<TProp> syntax, bool isOptional)
		{
			Syntax = syntax;
			IsOptional = isOptional;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition position = lexer.Here();

			SkipWhitespace(lexer);

			bool succeeded = false;
			TProp? nextProperty;
			while ((nextProperty = Syntax.Parse(lexer, property)) != null)
			{
				property = nextProperty;
				succeeded = true;

				SkipWhitespace(lexer);
			}

			if (!succeeded)
			{
				lexer.Rewind(position);
				return IsOptional ? property : null;
			}

			return property;
		}

		public override string ToString()
			=> "[ " + Syntax.ToString() + (IsOptional ? " ]" : " ]?");
	}
}
