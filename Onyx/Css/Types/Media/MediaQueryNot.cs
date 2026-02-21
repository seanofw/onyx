using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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
				Child.GetExpression(param));

		public override string ToString()
			=> $"(not {Child})";
	}
}
