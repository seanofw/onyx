using Onyx.Rendering;
using SkiaSharp;

namespace Onyx.Skia
{
	public class SkiaFont : SkiaDisposable, IFont
	{
		public FontInfo FontInfo { get; }

		public FontMetrics FontMetrics { get; }

		internal SKFont Font { get; }

		private SkiaFont(FontInfo fontInfo, SKFont font)
		{
			FontInfo = fontInfo;
			Font = font;

			FontMetrics = new FontMetrics(
				lineHeight: Math.Abs(font.Metrics.Ascent) + Math.Abs(font.Metrics.Descent) + Math.Abs(font.Metrics.Leading),
				ascent: font.Metrics.Ascent,
				descent: font.Metrics.Descent,

				underlineThickness: font.Metrics.UnderlineThickness ?? 1,
				underlinePosition: font.Metrics.UnderlinePosition ?? -1,
				strikethroughPosition: font.Metrics.StrikeoutPosition ?? font.Metrics.XHeight - 1,
				overlinePosition: font.Metrics.Ascent,

				em: this.MeasureText("M").Size,
				en: this.MeasureText("N").Size,
				ex: this.MeasureText("x").Size,
				space: this.MeasureText(" ").Size.Width
			);
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				Font.Dispose();
			}
		}

		public static IFont? Create(FontInfo fontInfo, bool exactMatchOnly)
		{
			SKFontStyleWeight weight = (SKFontStyleWeight)Math.Min(Math.Max((fontInfo.Weight / 100) * 100, 0), 1000);

			SKFontStyleSlant slant = fontInfo.Style switch
			{
				Css.Types.FontStyle.Normal => SKFontStyleSlant.Upright,
				Css.Types.FontStyle.Italic => SKFontStyleSlant.Italic,
				Css.Types.FontStyle.Oblique => SKFontStyleSlant.Oblique,
				_ => SKFontStyleSlant.Upright,
			};

			SKTypeface? typeface = SKTypeface.FromFamilyName(fontInfo.Name, new SKFontStyle(weight, SKFontStyleWidth.Normal, slant));
			if (typeface == null)
				return null;

			SKFont? font = new SKFont(typeface, (float)fontInfo.Size)
			{
				Edging = SKFontEdging.SubpixelAntialias,
				Hinting = SKFontHinting.Full,
				Subpixel = true,
			};
			if (font == null)
				return null;

			return new SkiaFont(fontInfo, font);
		}

		public IShapedText ShapeText(ReadOnlySpan<char> text)
			=> new SkiaShapedText(Font, text);
	}
}
