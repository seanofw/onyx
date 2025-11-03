using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FontVariantProperty : StyleProperty
	{
		public FontVariant Variant { get; init; }

		public static FontVariantProperty Default = new FontVariantProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFontVariant(Variant);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFontVariant(source.FontVariant);

		public override string ToString()
			=> Variant.ToString().Hyphenize();
	}
}