using System.Linq.Expressions;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryFalse : MediaQuery
	{
		public static MediaQueryFalse Instance { get; } = new MediaQueryFalse();

		private MediaQueryFalse()
			: base(MediaQueryKind.False, usesDimensions: false, hasErrors: false)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> false;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(false);

		public override void ToString(StringBuilder dest)
		{
			dest.Append("false");
		}
	}
}
