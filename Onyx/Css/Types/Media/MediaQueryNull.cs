using System.Linq.Expressions;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryNull : MediaQuery
	{
		public static MediaQueryNull Instance { get; } = new MediaQueryNull();

		private MediaQueryNull()
			: base(MediaQueryKind.Null, usesDimensions: false, hasErrors: false)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> null;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(null);

		public override void ToString(StringBuilder dest)
		{
			dest.Append("null");
		}
	}
}
