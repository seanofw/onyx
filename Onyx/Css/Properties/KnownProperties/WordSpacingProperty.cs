using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class WordSpacingProperty : StyleProperty
	{
		public bool Normal { get; init; }
		public Measure Length { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithWordSpacing(Normal ? new Measure(Units.Ex, 0.5) : Length);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithWordSpacing(source.WordSpacing);

		public override string ToString()
			=> Normal ? "normal" : Length.ToString();
	}
}
