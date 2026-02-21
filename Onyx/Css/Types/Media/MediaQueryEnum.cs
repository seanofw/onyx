using System.Linq.Expressions;
using System.Text;
using Onyx.Extensions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryEnum<T> : MediaQuery
		where T : struct
	{
		public T Value { get; }

		public MediaQueryEnum(T value)
			: base(MediaQueryKind.Enum, usesDimensions: false, hasErrors: false)
		{
			Value = value;
		}

		public override bool? Eval(MediaQueryContext context)
			=> throw new NotSupportedException();

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Constant(Value);

		public object? GetValue(MediaQueryContext context)
			=> Value;

		public override void ToString(StringBuilder dest)
		{
			dest.Append(Value.ToString()?.Hyphenize());
		}
	}
}
