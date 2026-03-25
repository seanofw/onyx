using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	internal class DefaultSkiaImageHandling : ISkiaRendererImageExtension
	{
		private class NoImage : SkiaDisposable, IImage
		{
			public static NoImage Instance { get; } = new NoImage();

			public Size2d? Size => default;
			public Size2d MinSize => default;
			public Size2d MaxSize => default;
			public double? AspectRatio => 1.0;
		}

		public IImage? CreateImage(SkiaRenderer renderer, string url)
			=> NoImage.Instance;

		public void DrawImage(SkiaRenderer renderer, IImage image, Rect2d sourceRect, Rect2d destRect, SKPaint paint, DrawStyle style)
		{
			paint.Color = Color32.Gray.ToSKColor();
			paint.Shader = null;
			paint.IsStroke = false;
			renderer.Canvas.DrawRect(new SKRect((float)destRect.X, (float)destRect.Y, (float)destRect.Width, (float)destRect.Height), paint);

			paint.Color = Color32.Black.ToSKColor();
			paint.IsStroke = true;
			paint.StrokeWidth = 1.0f;
			paint.StrokeCap = SKStrokeCap.Square;
			paint.StrokeJoin = SKStrokeJoin.Miter;
			paint.StrokeMiter = 1.0f;
			renderer.Canvas.DrawRect(new SKRect((float)destRect.X, (float)destRect.Y, (float)destRect.Width, (float)destRect.Height), paint);

			renderer.Canvas.DrawLine(new SKPoint((float)destRect.X, (float)destRect.Y),
				new SKPoint((float)(destRect.X + destRect.Width), (float)(destRect.Y + destRect.Height)), paint);
			renderer.Canvas.DrawLine(new SKPoint((float)(destRect.X + destRect.Width), (float)destRect.Y),
				new SKPoint((float)destRect.X, (float)(destRect.Y + destRect.Height)), paint);
		}
	}
}
