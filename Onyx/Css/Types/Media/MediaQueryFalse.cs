using System.Linq.Expressions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryFalse : MediaQuery
	{
		public static MediaQueryFalse Instance { get; } = new MediaQueryFalse();

		private MediaQueryFalse()
			: base(MediaQueryKind.False)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> false;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(false);
	}
}
