using System.Globalization;
using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
	public record class ZIndexProperty : StyleProperty
	{
		public int ZIndex { get; init; }
		public bool Auto { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithZIndex(Auto ? 0 : ZIndex);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithZIndex(source.ZIndex);

		public override string ToString()
			=> Auto ? "auto" : ZIndex.ToString(CultureInfo.InvariantCulture);
	}
}
