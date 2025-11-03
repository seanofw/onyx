using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class AnyOrderSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public IReadOnlyList<Syntax<TProp>> Syntaxes { get; }

		public AnyOrderSyntax(IEnumerable<Syntax<TProp>>? syntaxes = null)
		{
			Syntaxes = syntaxes?.ToArray() ?? Array.Empty<Syntax<TProp>>();
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			ulong matches = 0;

			bool matched;
			do
			{
				SkipWhitespace(lexer);

				matched = false;
				for (int i = 0; i < Syntaxes.Count; i++)
				{
					if ((matches & (1UL << i)) != 0)
						continue;   // Already used this syntax.

					CssLexerPosition position = lexer.Here();

					Syntax<TProp> syntax = Syntaxes[i];
					TProp? result = syntax.Parse(lexer, property);
					if (result != null)
					{
						matches |= (1UL << i);	// Mark this syntax as having been used.
						matched = true;
						property = result;
						break;
					}

					lexer.Rewind(position);
				}
			} while (matched);

			if (matches == 0)
				return null;

			return property;
		}

		public override string ToString()
			=> "[ " + string.Join(" || ", Syntaxes.Select(s => s.ToString())) + " ]";
	}
}
