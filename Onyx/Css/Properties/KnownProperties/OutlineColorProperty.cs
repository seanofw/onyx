using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public record class OutlineColorProperty : StyleProperty
	{
		public Color32 Color { get; init; }
		public bool Invert { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOutlineColor(Color, Invert);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOutlineColor(source.OutlineColor, source.OutlineInvert);

		public override string ToString()
			=> Invert ? "invert" : Color.ToString();
	}
}
