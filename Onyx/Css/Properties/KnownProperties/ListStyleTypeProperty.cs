using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class ListStyleTypeProperty : StyleProperty
    {
        public ListStyleType Style { get; init; }

		public static ListStyleTypeProperty Default { get; } = new ListStyleTypeProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithListStyleType(Style);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithListStyleType(source.ListStyleType);

		public override string ToString()
			=> Style.ToString().Hyphenize();
	}
}
