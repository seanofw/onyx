using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryAnd : MediaQueryBinary
	{
		public MediaQueryAnd(MediaQuery left, MediaQuery right)
			: base(MediaQueryKind.And, left, right)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static bool? KleeneAnd(bool? a, bool? b)
			=> a.HasValue && b.HasValue ? a.Value && b.Value
				: a == false || b == false ? false
				: null;

		private static MethodInfo _kleeneAndMethod = typeof(MediaQueryAnd).GetMethod(nameof(KleeneAnd),
			BindingFlags.NonPublic | BindingFlags.Static)!;

		public override bool? Eval(MediaQueryContext context)
			=> KleeneAnd(Left.Eval(context), Right.Eval(context));

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Call(null, _kleeneAndMethod,
				MaybeConvert(Left.GetExpression(param), typeof(bool?)),
				MaybeConvert(Right.GetExpression(param), typeof(bool?)));

		private static Expression MaybeConvert(Expression expr, Type type)
			=> expr.Type != type ? Expression.Convert(expr, type) : expr;

		public override void ToString(StringBuilder dest)
		{
			if (Left is MediaQueryOr)
			{
				dest.Append("(");
				Left.ToString(dest);
				dest.Append(")");
			}
			else
			{
				Left.ToString(dest);
			}

			dest.Append(" and ");

			if (Right is MediaQueryOr)
			{
				dest.Append("(");
				Right.ToString(dest);
				dest.Append(")");
			}
			else
			{
				Right.ToString(dest);
			}
		}
	}
}
