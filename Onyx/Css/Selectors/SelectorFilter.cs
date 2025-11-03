using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public abstract class SelectorFilter : IEquatable<SelectorFilter>
	{
		public SelectorFilterKind Kind { get; }

		public abstract Specificity Specificity { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SelectorFilter(SelectorFilterKind kind)
		{
			Kind = kind;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is SelectorFilter other && Equals(other);

		public virtual bool Equals(SelectorFilter? other)
			=> ReferenceEquals(this, other) ? true
				: ReferenceEquals(other, null) ? false
				: Kind == other.Kind;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				return hashCode;
			}
		}

		public static bool operator ==(SelectorFilter? a, SelectorFilter? b)
			=> ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);

		public static bool operator !=(SelectorFilter? a, SelectorFilter? b)
			=> ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !a.Equals(b);

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		public abstract Expression GetMatchExpression(ParameterExpression element);

		public abstract bool IsMatch(Element element);

		public abstract void ToString(StringBuilder dest);
	}
}
