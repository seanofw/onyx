using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FlexBasisProperty : StyleProperty
	{
		public Measure Measure { get; init; }
		public bool Auto { get; init; }
		public bool Content { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFlexBasis(Auto ? PseudoMeasures.Auto
				: Content ? PseudoMeasures.Content
				: Measure);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFlexBasis(source.FlexBasis);

		public override string ToString()
			=> Content ? "content"
				: Auto ? "auto"
				: Measure.ToString();
	}
}
