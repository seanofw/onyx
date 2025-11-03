using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OverflowYProperty : StyleProperty
	{
		public OverflowKind OverflowY { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOverflowY(OverflowY);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOverflowY(source.OverflowY);

		public override string ToString()
			=> OverflowY.ToString().Hyphenize();
	}
}
