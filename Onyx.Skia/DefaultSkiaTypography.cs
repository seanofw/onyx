using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	internal class DefaultSkiaTypography : ISkiaRendererTypographyExtension
	{
		public IFont? CreateFont(SkiaRenderer renderer, FontInfo fontInfo, bool exactMatchOnly = false)
			=> SkiaFont.Create(fontInfo, exactMatchOnly);

		public void DrawText(SkiaRenderer renderer, Vector2d topLeftCorner, IShapedText shapedText,
			SKPaint paint, DrawStyle style)
		{
			if (shapedText is not SkiaShapedText skiaShapedText)
				return;

			renderer.Canvas.DrawText(skiaShapedText.TextBlob,
				(float)topLeftCorner.X, (float)topLeftCorner.Y, paint);
		}
	}
}
