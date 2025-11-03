using System.Globalization;
using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FlexGrowProperty : StyleProperty
	{
		public double Grow { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFlexGrow(Grow);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFlexGrow(source.FlexGrow);

		public override string ToString()
			=> Grow.ToString(CultureInfo.InvariantCulture);
	}
}
