using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class PageBreakPropertyBase : StyleProperty
	{
		public PageBreakOption Break { get; init; }

		public override string ToString()
			=> Break.ToString().Hyphenize();
	}

	public sealed record class PageBreakAfterProperty : PageBreakPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithPageBreakAfter(Break);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithPageBreakAfter(source.PageBreakAfter);
}

	public sealed record class PageBreakBeforeProperty : PageBreakPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithPageBreakBefore(Break);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithPageBreakBefore(source.PageBreakBefore);
	}
}
