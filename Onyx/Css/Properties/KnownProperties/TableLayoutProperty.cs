using Onyx.Css.Computed;
using Onyx.Extensions;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class TableLayoutProperty : StyleProperty
	{
		public TableLayout TableLayout { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithTableLayout(TableLayout);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithTableLayout(source.TableLayout);

		public override string ToString()
			=> TableLayout.ToString().Hyphenize();
	}
}
