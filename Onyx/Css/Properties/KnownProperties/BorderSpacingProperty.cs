using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BorderSpacingProperty : StyleProperty
	{
		public Measure Length { get; init; }
		public Measure Length2 { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> Length2.Units != default
				? style.WithBorderSpacing(Length, Length2)
				: style.WithBorderSpacing(Length);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderSpacing(source.BorderSpacingX, source.BorderSpacingY);

		public override string ToString()
			=> Length2.Units != default ? $"{Length} {Length2}" : Length.ToString();
	}
}
