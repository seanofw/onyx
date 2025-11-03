using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class PunctSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, CssToken, TProp>? Constructor { get; }
		public IReadOnlySet<CssTokenKind> Kinds => _kinds;
		private readonly HashSet<CssTokenKind> _kinds;

		public PunctSyntax(IEnumerable<CssTokenKind> kinds,
			Func<TProp, CssToken, TProp>? constructor = null)
		{
			_kinds = kinds.ToHashSet();
			Constructor = constructor;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if (!_kinds.Contains((token = lexer.Next()).Kind))
			{
				lexer.Unget(token);
				return null;
			}

			if (Constructor != null)
				property = Constructor.Invoke(property, token);

			return property;
		}

		public override string ToString()
			=> "punct(" + string.Join(", ", Kinds.Select(k => k.ToString())) + ")";
	}
}
