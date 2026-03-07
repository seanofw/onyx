using System.Linq.Expressions;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryError : MediaQuery
	{
		public static MediaQueryError Instance { get; } = new MediaQueryError();

		private MediaQueryError()
			: base(MediaQueryKind.Error, usesDimensions: false, hasErrors: true)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> null;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(null, typeof(bool?));

		public override void ToString(StringBuilder dest)
		{
			dest.Append("error");
		}
	}
}
