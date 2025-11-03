using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OverflowXProperty : StyleProperty
	{
		public OverflowKind OverflowX { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOverflowX(OverflowX);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOverflowX(source.OverflowX);

		public override string ToString()
			=> OverflowX.ToString().Hyphenize();
	}
}
