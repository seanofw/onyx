using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class ColorProperty : StyleProperty
	{
		public Color32 Color { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithColor(Color);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithColor(source.Color);

		public override string ToString()
			=> Color.ToString();
	}
}
