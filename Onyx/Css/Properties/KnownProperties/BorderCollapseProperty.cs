using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BorderCollapseProperty : StyleProperty
	{
		public BorderCollapse Collapse { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderCollapse(Collapse);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderCollapse(source.BorderCollapse);

		public override string ToString()
			=> $"{Collapse.ToString().Hyphenize()}";
	}
}
