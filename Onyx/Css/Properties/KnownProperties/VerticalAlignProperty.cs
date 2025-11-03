using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class VerticalAlignProperty : StyleProperty
	{
		public VerticalAlign VerticalAlign { get; init; }
		public Measure VerticalAlignLength { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithVerticalAlign(VerticalAlign, VerticalAlignLength);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithVerticalAlign(source.VerticalAlign, source.VerticalAlignLength);

		public override string ToString()
			=> VerticalAlign != default
					? VerticalAlign.ToString().Hyphenize()
				: VerticalAlignLength.ToString();
	}
}
