
using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
    public abstract record class WidowAndOrphanPropertyBase : StyleProperty
    {
        public int Count { get; init; }
    }

    public sealed record class WidowsProperty : WidowAndOrphanPropertyBase
    {
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithWidows(Count);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithWidows(source.Widows);
	}

	public sealed record class OrphansProperty : WidowAndOrphanPropertyBase
    {
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOrphans(Count);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOrphans(source.Orphans);
	}
}
