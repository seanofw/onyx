using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class MinPropertyBase : StyleProperty
	{
		public Measure Offset { get; init; }

		public override string ToString()
			=> Offset.ToString();
	}

	public sealed record class MinWidthProperty : MaxPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithMinWidth(Offset);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithMinWidth(source.MinWidth);
	}

	public sealed record class MinHeightProperty : MaxPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithMinHeight(Offset);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithMinHeight(source.MinHeight);
	}
}
