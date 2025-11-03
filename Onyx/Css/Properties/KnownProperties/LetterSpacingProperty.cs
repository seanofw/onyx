using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class LetterSpacingProperty : StyleProperty
	{
		public bool Normal { get; init; }
		public Measure Length { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithLetterSpacing(Normal ? Measure.Zero : Length);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithLetterSpacing(source.LetterSpacing);

		public override string ToString()
			=> Normal ? "normal" : Length.ToString();
	}
}
