using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryOr : MediaQueryBinary
	{
		public MediaQueryOr(MediaQuery left, MediaQuery right)
			: base(MediaQueryKind.Or, left, right)
		{
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
				Left.GetExpression(param),
				Right.GetExpression(param));

		public override string ToString()
			=> $"({Left} or {Right})";
	}
}
