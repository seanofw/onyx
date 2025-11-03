using System.Globalization;
using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OpacityProperty : StyleProperty
	{
		public double Opacity { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOpacity(Opacity);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOpacity(source.Opacity);

		public override string ToString()
			=> Opacity.ToString(CultureInfo.InvariantCulture);
	}
}
