using Onyx.Css.Parsing;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class ColorSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, Color32, TProp> Constructor { get; }

		public ColorSyntax(Func<TProp, Color32, TProp> constructor)
			=> Constructor = constructor;

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			SkipWhitespace(lexer);

			if (!Color32.TryParse(lexer, out Color32 color, out SourceLocation location))
				return null;
			property = Constructor(property, color);
			return property;
		}

		public override string ToString()
			=> "color";
	}
}
