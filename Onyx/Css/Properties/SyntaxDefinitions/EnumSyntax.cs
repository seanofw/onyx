using Onyx.Css.Parsing;
using Onyx.Extensions;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class EnumSyntax<TProp, TEnum> : Syntax<TProp>
		where TProp : class
		where TEnum : struct
	{
		public Func<TProp, TEnum, TProp> Constructor { get; }

		public IReadOnlyDictionary<string, TEnum> EnumLookup { get; }

		public EnumSyntax(Func<TProp, TEnum, TProp> constructor)
		{
			Constructor = constructor;
			EnumLookup = CreateEnumLookup();
		}

		private static Dictionary<string, TEnum> CreateEnumLookup()
		{
			Dictionary<string, TEnum> lookup = new Dictionary<string, TEnum>();

			foreach (string name in Enum.GetNames(typeof(TEnum)))
			{
				TEnum value = Enum.Parse<TEnum>(name, true);
				string cssName = name.Hyphenize();
				lookup[cssName] = value;
			}

			return lookup;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Ident
				|| !EnumLookup.TryGetValue(token.Text!, out TEnum value))
			{
				lexer.Unget(token);
				return null;
			}
			property = Constructor(property, value);
			return property;
		}

		public override string ToString()
			=> "enum<" + typeof(TEnum).Name + ">";
	}
}
