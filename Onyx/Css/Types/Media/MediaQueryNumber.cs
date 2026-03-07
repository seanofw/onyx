using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryNumber : MediaQuery
	{
		public double Number { get; }

		public MediaQueryNumber(double number)
			: base(MediaQueryKind.Number, usesDimensions: false, hasErrors: false)
		{
			Number = number;
		}

		public override bool? Eval(MediaQueryContext context)
			=> throw new NotSupportedException();

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(Number);

		public object? GetValue(MediaQueryContext context)
			=> Number;

		public override void ToString(StringBuilder dest)
		{
			dest.Append(Number.ToString(CultureInfo.InvariantCulture));
		}
	}
}
