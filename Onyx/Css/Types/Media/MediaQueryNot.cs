using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryNot : MediaQueryUnary
	{
		public MediaQueryNot(MediaQuery child)
			: base(MediaQueryKind.Not, child)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static bool? KleeneNot(bool? value)
			=> value.HasValue ? !value.Value : null;

		private static MethodInfo _kleeneNotMethod = typeof(MediaQueryNot).GetMethod(nameof(KleeneNot),
			BindingFlags.NonPublic | BindingFlags.Static)!;

		public override bool? Eval(MediaQueryContext context)
			=> KleeneNot(Child.Eval(context));

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Call(null, _kleeneNotMethod,
				MaybeConvert(Child.GetExpression(param), typeof(bool?)));

		private static Expression MaybeConvert(Expression expr, Type type)
			=> expr.Type != type ? Expression.Convert(expr, type) : expr;

		public override void ToString(StringBuilder dest)
		{
			dest.Append("not ");

			if (Child is MediaQueryAnd || Child is MediaQueryOr)
			{
				dest.Append("(");
				Child.ToString(dest);
				dest.Append(")");
			}
			else
				Child.ToString(dest);
		}
	}
}
