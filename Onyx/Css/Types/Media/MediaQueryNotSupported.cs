using System.Linq.Expressions;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryNotSupported : MediaQuery
	{
		public static MediaQueryNotSupported Instance { get; } = new MediaQueryNotSupported();

		private MediaQueryNotSupported()
			: base(MediaQueryKind.NotSupported, usesDimensions: false, hasErrors: false)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> false;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(false);

		public override void ToString(StringBuilder dest)
		{
			dest.Append("not-supported");
		}
	}
}
