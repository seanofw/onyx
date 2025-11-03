using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class BorderEdgeStyleProperty : StyleProperty
	{
		public BorderStyle Style { get; init; }

		public override string ToString()
			=> Style.ToString().Hyphenize();
	}

	public sealed record class BorderTopStyleProperty : BorderEdgeStyleProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderTopStyle(Style);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderTopStyle(source.BorderTopStyle);
	}

	public sealed record class BorderRightStyleProperty : BorderEdgeStyleProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderRightStyle(Style);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderRightStyle(source.BorderTopStyle);
	}

	public sealed record class BorderBottomStyleProperty : BorderEdgeStyleProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderBottomStyle(Style);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderBottomStyle(source.BorderTopStyle);
	}

	public sealed record class BorderLeftStyleProperty : BorderEdgeStyleProperty
	{
		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBorderLeftStyle(Style);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBorderLeftStyle(source.BorderTopStyle);
	}
}
