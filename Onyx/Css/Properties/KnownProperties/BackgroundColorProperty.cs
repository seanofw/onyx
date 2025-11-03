using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BackgroundColorProperty : StyleProperty
	{
		public Color32 Color { get; init; }

		public static BackgroundColorProperty Default { get; } =
			new BackgroundColorProperty { Kind = KnownPropertyKind.BackgroundColor };

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBackgroundColor(Color);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBackgroundColor(source.BackgroundColor);

		public override string ToString()
			=> Color.ToString();
	}
}
