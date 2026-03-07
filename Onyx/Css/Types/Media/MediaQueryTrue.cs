using System.Linq.Expressions;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryTrue : MediaQuery
	{
		public static MediaQueryTrue Instance { get; } = new MediaQueryTrue();

		private MediaQueryTrue()
			: base(MediaQueryKind.True, usesDimensions: false, hasErrors: false)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> true;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(true);

		public override void ToString(StringBuilder dest)
		{
			dest.Append("true");
		}
	}
}
