using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryOr : MediaQueryBinary
	{
		public bool IsComma { get; }

		public MediaQueryOr(MediaQuery left, MediaQuery right, bool isComma)
			: base(MediaQueryKind.Or, left, right)
		{
			IsComma = isComma;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static bool? KleeneOr(bool? a, bool? b)
			=> a.HasValue && b.HasValue ? a.Value || b.Value
				: a == true || b == true ? true
				: null;

		private static MethodInfo _kleeneOrMethod = typeof(MediaQueryOr).GetMethod(nameof(KleeneOr),
			BindingFlags.NonPublic | BindingFlags.Static)!;

		public override bool? Eval(MediaQueryContext context)
			=> KleeneOr(Left.Eval(context), Right.Eval(context));

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Call(null, _kleeneOrMethod,
				MaybeConvert(Left.GetExpression(param), typeof(bool?)),
				MaybeConvert(Right.GetExpression(param), typeof(bool?)));

		private static Expression MaybeConvert(Expression expr, Type type)
			=> expr.Type != type ? Expression.Convert(expr, type) : expr;

		public override void ToString(StringBuilder dest)
		{
			Left.ToString(dest);

			dest.Append(IsComma ? ", " : " or ");

			Right.ToString(dest);
		}
	}
}
