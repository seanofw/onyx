using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class BorderEdgeColorProperty : StyleProperty
	{
		public Color32 Color { get; init; }

		public override string ToString()
			=> Color.ToString();
	}

	public sealed record class BorderTopColorProperty : BorderEdgeColorProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderTopColor(Color);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderTopColor(source.BorderTopColor);
	}

	public sealed record class BorderRightColorProperty : BorderEdgeColorProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderRightColor(Color);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderRightColor(source.BorderRightColor);
	}

	public sealed record class BorderBottomColorProperty : BorderEdgeColorProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderBottomColor(Color);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderBottomColor(source.BorderBottomColor);
	}

	public sealed record class BorderLeftColorProperty : BorderEdgeColorProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderLeftColor(Color);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderLeftColor(source.BorderLeftColor);
	}
}
