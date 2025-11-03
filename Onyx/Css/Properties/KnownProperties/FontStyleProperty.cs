using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FontStyleProperty : StyleProperty
	{
		public FontStyle Style { get; init; }

		public static FontStyleProperty Default = new FontStyleProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFontStyle(Style);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFontStyle(source.FontStyle);

		public override string ToString()
			=> Style.ToString().Hyphenize();
	}
}