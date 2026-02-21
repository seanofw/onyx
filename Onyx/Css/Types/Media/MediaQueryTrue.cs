using System.Linq.Expressions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryTrue : MediaQuery
	{
		public static MediaQueryTrue Instance { get; } = new MediaQueryTrue();

		private MediaQueryTrue()
			: base(MediaQueryKind.True)
		{
		}

		public override bool? Eval(MediaQueryContext context)
			=> true;

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(true);
	}
}
