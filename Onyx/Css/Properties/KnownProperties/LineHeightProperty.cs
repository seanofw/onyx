using System.Globalization;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class LineHeightProperty : StyleProperty
    {
        public bool Normal { get; init; }
        public double? Number { get; init; }
        public Measure Measure { get; init; }

		public static LineHeightProperty Default { get; } = new LineHeightProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithLineHeight(Normal ? new Measure(Units.Percent, 120)
				: Number.HasValue ? new Measure(Units.Percent, Number.Value * 100)
				: Measure);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithLineHeight(source.LineHeight);

		public override string ToString()
			=> Normal ? "normal"
				: Number.HasValue ? Number.Value.ToString(CultureInfo.InvariantCulture)
				: Measure.ToString();
	}
}
