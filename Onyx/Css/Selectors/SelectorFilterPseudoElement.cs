using System.Text;

namespace Onyx.Css.Selectors
{
	public abstract class SelectorFilterPseudoElement : SelectorFilter
	{
		protected abstract string PseudoElementName { get; }

		public override Specificity Specificity => new Specificity(elementCount: 1);

		protected SelectorFilterPseudoElement(SelectorFilterKind kind)
			: base(kind)
		{
		}

		public override bool Equals(SelectorFilter? filter)
			=> ReferenceEquals(this, filter) ? true
				: ReferenceEquals(filter, null) ? false
				: filter is SelectorFilterClass other
					&& Kind == other.Kind;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				return hashCode;
			}
		}

		public override void ToString(StringBuilder dest)
		{
			dest.Append(':');
			dest.Append(PseudoElementName);
		}
	}
}
