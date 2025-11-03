using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class FlexDirectionProperty : StyleProperty
    {
        public FlexDirection Direction { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFlexDirection(Direction);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFlexDirection(source.FlexDirection);

		public override string ToString()
			=> Direction.ToString().Hyphenize();
	}
}
