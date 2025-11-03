using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class BorderEdgeWidthProperty : StyleProperty
	{
		public Measure Width { get; init; }

		public override string ToString()
			=> Width.ToString();
	}

	public sealed record class BorderTopWidthProperty : BorderEdgeWidthProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderTopWidth(Width);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderTopWidth(source.BorderTopWidth);
	}

	public sealed record class BorderRightWidthProperty : BorderEdgeWidthProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderRightWidth(Width);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderRightWidth(source.BorderTopWidth);
	}

	public sealed record class BorderBottomWidthProperty : BorderEdgeWidthProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderBottomWidth(Width);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderBottomWidth(source.BorderTopWidth);
	}

	public sealed record class BorderLeftWidthProperty : BorderEdgeWidthProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderLeftWidth(Width);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderLeftWidth(source.BorderTopWidth);
	}
}
