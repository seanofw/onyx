using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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
				Left.GetExpression(param),
				Right.GetExpression(param));

		public override string ToString()
			=> $"({Left} and {Right})";
	}
}
