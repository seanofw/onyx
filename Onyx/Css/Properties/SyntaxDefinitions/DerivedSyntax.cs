using Onyx.Css.Parsing;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class DerivedSyntax<TProp, TChildProp> : Syntax<TProp>
		where TProp : class
		where TChildProp : class
	{
		public Syntax<TChildProp> Syntax { get; }
		public Func<TProp, TChildProp> Extract { get; }
		public Func<TProp, TChildProp, TProp> Apply { get; }

		public DerivedSyntax(Syntax<TChildProp> syntax,
			Func<TProp, TChildProp> extract,
			Func<TProp, TChildProp, TProp> apply)
		{
			Syntax = syntax;
			Extract = extract;
			Apply = apply;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition position = lexer.Here();

			TChildProp? childProp = Extract(property);
			TChildProp? result = Syntax.Parse(lexer, childProp);
			if (result != null)
				return Apply(property, result);

			lexer.Rewind(position);
			return null;
		}

		public override string ToString()
			=> Syntax.ToString()!;
	}
}
