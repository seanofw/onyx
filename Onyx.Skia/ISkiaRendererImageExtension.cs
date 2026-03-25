using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public interface ISkiaRendererImageExtension : ISkiaRendererExtension
	{
		IImage? CreateImage(SkiaRenderer renderer, string url);
		void DrawImage(SkiaRenderer renderer, IImage image, Rect2d sourceRect, Rect2d destRect,
			SKPaint paint, DrawStyle style);
	}
}
