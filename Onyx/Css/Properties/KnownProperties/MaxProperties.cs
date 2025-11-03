using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class MaxPropertyBase : StyleProperty
	{
		public Measure Offset { get; init; }
		public bool None { get; init; }

		public override string ToString()
			=> None ? "none" : Offset.ToString();
	}

	public sealed record class MaxWidthProperty : MaxPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithMaxWidth(None ? PseudoMeasures.None : Offset);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithMaxWidth(source.MaxWidth);
	}

	public sealed record class MaxHeightProperty : MaxPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithMaxHeight(None ? PseudoMeasures.None : Offset);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithMaxHeight(source.MaxHeight);
	}
}
