using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OutlineWidthProperty : StyleProperty
	{
		public Measure Width { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOutlineWidth(Width);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOutlineWidth(source.OutlineWidth);

		public override string ToString()
			=> Width.ToString();
	}
}
