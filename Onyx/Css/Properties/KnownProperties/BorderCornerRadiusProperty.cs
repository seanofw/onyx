using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class BorderCornerRadiusProperty : StyleProperty
	{
		public Measure Radius { get; init; }

		public override string ToString()
			=> Radius.ToString();
	}

	public sealed record class BorderTopLeftRadiusProperty : BorderCornerRadiusProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderTopLeftRadius(Radius);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderTopLeftRadius(source.BorderTopLeftRadius);
	}

	public sealed record class BorderTopRightRadiusProperty : BorderCornerRadiusProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderTopRightRadius(Radius);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderTopRightRadius(source.BorderTopRightRadius);
	}

	public sealed record class BorderBottomLeftRadiusProperty : BorderCornerRadiusProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderBottomLeftRadius(Radius);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderBottomLeftRadius(source.BorderBottomLeftRadius);
	}

	public sealed record class BorderBottomRightRadiusProperty : BorderCornerRadiusProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderBottomRightRadius(Radius);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderBottomRightRadius(source.BorderBottomRightRadius);
	}
}
