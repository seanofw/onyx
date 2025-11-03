using Onyx.Css.Parsing;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class LengthSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, Measure, TProp> Constructor { get; }

		public LengthSyntax(Func<TProp, Measure, TProp> constructor)
		{
			Constructor = constructor;
		}

		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			Measure? measure = ReadLength(lexer);
			if (!measure.HasValue)
				return null;

			property = Constructor(property, measure.Value);
			return property;
		}

		public override string ToString()
			=> "length";
	}
}
