using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class BoxSizingProperty : StyleProperty
    {
        public BoxSizingMode Mode { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBoxSizing(Mode);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBoxSizing(source.BoxSizing);

		public override string ToString()
			=> Mode.ToString().Hyphenize();
	}
}
