using System.Linq.Expressions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryNull : MediaQuery
	{
		public static MediaQueryNull Instance { get; } = new MediaQueryNull();

		private MediaQueryNull()
			: base(MediaQueryKind.Null)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> null;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(null);
	}
}
