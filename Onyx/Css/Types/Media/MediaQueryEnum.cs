using System.Linq.Expressions;
using Onyx.Extensions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryEnum<T> : MediaQuery
		where T : struct
	{
		public T Value { get; }

		public MediaQueryEnum(T value)
			: base(MediaQueryKind.Enum)
		{
			Value = value;
		}

		public override bool? Eval(MediaQueryContext context)
			=> throw new NotSupportedException();

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(Value);

		public object? GetValue(MediaQueryContext context)
			=> Value;

		public override string ToString()
			=> Value.ToString()!.Hyphenize();
	}
}
