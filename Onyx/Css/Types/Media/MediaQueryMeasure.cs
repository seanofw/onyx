using System.Globalization;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryMeasure : MediaQuery
	{
		public Measure Measure { get; }

		public MediaQueryMeasure(Measure measure)
			: base(MediaQueryKind.Measure, usesDimensions: false, hasErrors: false)
		{
			Measure = measure;
		}

		public override bool? Eval(MediaQueryContext context)
			=> throw new NotSupportedException();

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(Measure);

		public object? GetValue(MediaQueryContext context)
			=> Measure;

		public override void ToString(StringBuilder dest)
		{
			dest.Append(Measure.ToString());
		}
	}
}
