using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class SequenceSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public IReadOnlyList<Syntax<TProp>> Syntaxes { get; }

		public SequenceSyntax(IEnumerable<Syntax<TProp>>? syntaxes)
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
				if (result == null)
					return null;

				property = result;
			}

			return property;
		}

		public override string ToString()
			=> "[ " + string.Join(" ", Syntaxes.Select(s => s.ToString())) + " ]";
	}
}
