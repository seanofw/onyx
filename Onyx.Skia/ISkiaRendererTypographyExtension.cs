using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public interface ISkiaRendererTypographyExtension : ISkiaRendererExtension
	{
		IFont? CreateFont(SkiaRenderer renderer, FontInfo fontInfo, bool exactMatchOnly = false);
		void DrawText(SkiaRenderer renderer, Vector2d topLeftCorner, IShapedText shapedText,
			SKPaint paint, DrawStyle style);
	}
}
