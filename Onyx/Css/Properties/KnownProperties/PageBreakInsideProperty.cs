using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class PageBreakInsideProperty : StyleProperty
	{
		public PageBreakInsideOption Break { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithPageBreakInside(Break);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithPageBreakInside(source.PageBreakInside);

		public override string ToString()
			=> Break.ToString().Hyphenize();
	}
}
