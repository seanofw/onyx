using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class OneOfSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public IReadOnlyList<Syntax<TProp>> Syntaxes { get; }
		public bool AllowNone { get; }

		public OneOfSyntax(IEnumerable<Syntax<TProp>>? syntaxes, bool allowNone)
		{
			Syntaxes = syntaxes?.ToArray() ?? Array.Empty<Syntax<TProp>>();
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition position = lexer.Here();

			foreach (Syntax<TProp> syntax in Syntaxes)
			{
				SkipWhitespace(lexer);

				TProp? result = syntax.Parse(lexer, property);
				if (result != null)
					return result;

				lexer.Rewind(position);
			}

			return AllowNone ? property : null;
		}

		public override string ToString()
			=> "[ " + string.Join(" | ", Syntaxes.Select(s => s.ToString())) + (AllowNone ? " ]?" : " ]");
	}
}
