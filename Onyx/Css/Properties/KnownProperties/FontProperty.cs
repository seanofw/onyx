using System.Text;
using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FontProperty : StyleProperty
	{
		public FontStyleProperty? FontStyle { get; init; }
		public FontVariantProperty? FontVariant { get; init; }
		public FontWeightProperty? FontWeight { get; init; }
		public FontSizeProperty? FontSize { get; init; }
		public LineHeightProperty? LineHeight { get; init; }
		public FontFamilyProperty? FontFamily { get; init; }
		public SpecialFontProperty? SpecialFont { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
		{
			if (SpecialFont != null)
				return SpecialFont.ToString();

			StringBuilder stringBuilder = new StringBuilder();

			if (FontStyle != null)
				stringBuilder.Append(FontStyle.ToString());
			if (FontVariant != null)
			{
				if (stringBuilder.Length > 0)
					stringBuilder.Append(' ');
				stringBuilder.Append(FontVariant.ToString());
			}

			if (FontWeight != null)
			{
				if (stringBuilder.Length > 0)
					stringBuilder.Append(' ');
				stringBuilder.Append(FontWeight.ToString());
			}

			if (FontSize != null)
			{
				if (stringBuilder.Length > 0)
					stringBuilder.Append(' ');
				stringBuilder.Append(FontSize.ToString());
			}

			if (LineHeight != null)
			{
				stringBuilder.Append("/");
				stringBuilder.Append(LineHeight.ToString());
			}

			if (FontFamily != null)
			{
				if (stringBuilder.Length > 0)
					stringBuilder.Append(' ');
				stringBuilder.Append(FontFamily.ToString());
			}

			return stringBuilder.ToString();
		}

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (SpecialFont != null)
			{
				yield return SpecialFont;
				yield break;
			}

			if (FontStyle != null)
				yield return FontStyle;

			if (FontVariant != null)
				yield return FontVariant;

			if (FontWeight != null)
				yield return FontWeight;

			if (FontSize != null)
				yield return FontSize;

			if (LineHeight != null)
				yield return LineHeight;

			if (FontFamily != null)
				yield return FontFamily;
		}

		public override bool IsDecomposable => true;
	}
}
