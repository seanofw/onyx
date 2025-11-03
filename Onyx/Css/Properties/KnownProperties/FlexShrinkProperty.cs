using System.Globalization;
using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FlexShrinkProperty : StyleProperty
	{
		public double Shrink { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFlexShrink(Shrink);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFlexShrink(source.FlexShrink);

		public override string ToString()
			=> Shrink.ToString(CultureInfo.InvariantCulture);
	}
}
