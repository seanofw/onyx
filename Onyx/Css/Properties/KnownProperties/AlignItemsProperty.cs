using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class AlignItemsProperty : StyleProperty
	{
		public AlignItemsKind AlignItems { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithAlignItems(AlignItems);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithAlignItems(source.AlignItems);

		public override string ToString()
			=> AlignItems.ToString().Hyphenize();
	}
}
