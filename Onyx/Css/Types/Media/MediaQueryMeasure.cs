using System.Linq.Expressions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryMeasure : MediaQuery
	{
		public Measure Measure { get; }

		public MediaQueryMeasure(Measure measure)
			: base(MediaQueryKind.Measure)
		{
			Measure = measure;
		}

		public override bool? Eval(MediaQueryContext context)
			=> throw new NotSupportedException();

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(Measure);

		public object? GetValue(MediaQueryContext context)
			=> Measure;

		public override string ToString()
			=> Measure.ToString();
	}
}
