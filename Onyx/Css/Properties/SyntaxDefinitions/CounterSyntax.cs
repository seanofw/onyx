using Onyx.Css.Parsing;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal static class CounterSyntax
	{
		public static IReadOnlyDictionary<string, ListStyleType> EnumLookup
			=> _enumLookup ??= CreateEnumLookup();
		private static IReadOnlyDictionary<string, ListStyleType>? _enumLookup;

		private static Dictionary<string, ListStyleType> CreateEnumLookup()
		{
			Dictionary<string, ListStyleType> lookup = new Dictionary<string, ListStyleType>();

			foreach (string name in Enum.GetNames(typeof(ListStyleType)))
			{
				ListStyleType value = Enum.Parse<ListStyleType>(name, true);
				string cssName = name.Hyphenize();
				lookup[cssName] = value;
			}

			return lookup;
		}
	}

	internal class CounterSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, string, ListStyleType, TProp> Constructor { get; }

		public CounterSyntax(Func<TProp, string, ListStyleType, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition start = lexer.Here();

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Func
				|| token.Text != "counter")
				goto fail;

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
				goto fail;
			string name = token.Text!;

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

			property = Constructor(property, name, style);
			return property;

		fail:
			lexer.Rewind(start);
			return null;
		}

		public override string ToString()
			=> "<counter>";
	}
}
