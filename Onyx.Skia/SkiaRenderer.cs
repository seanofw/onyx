using System.Runtime.CompilerServices;
using Onyx.Css.Types;
using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public sealed class SkiaRenderer : SkiaDisposable, IRenderer, IRenderables
	{
		private readonly ISkiaRendererTypographyExtension _typographyExtension;
		private readonly ISkiaRendererImageExtension _imageExtension;

		private DrawStyle _lastDrawStyle = DrawStyle.Default;

		private SKCanvas _canvas;
		private SKPaint _paint;
		private SKRoundRect _roundRect;

		public SKCanvas Canvas => _canvas;

		public SkiaRenderer(SKCanvas canvas, params ISkiaRendererExtension[] extensions)
		{
			_canvas = canvas;
			_paint = new SKPaint();
			_paint.IsAntialias = true;
			_roundRect = new SKRoundRect();

			_typographyExtension = extensions.OfType<ISkiaRendererTypographyExtension>().FirstOrDefault()
				?? new DefaultSkiaTypography();

			_imageExtension = extensions.OfType<ISkiaRendererImageExtension>().FirstOrDefault()
				?? new DefaultSkiaImageHandling();
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				_roundRect?.Dispose();
				_roundRect = null!;

				_paint?.Dispose();
				_paint = null!;
			}

			_canvas = null!;
		}

		public void Begin()
		{
			_canvas.Save();
		}

		public IClipper CreateClipper(ReadOnlySpan<Vector2d> convexPolygon)
			=> new SkiaClipper(convexPolygon);

		public IBrush CreateLinearGradientBrush(LinearGradient linearGradient)
			=> new SkiaLinearGradientBrush(linearGradient);

		public IBrush CreateRadialGradientBrush(RadialGradient radialGradient)
			=> throw new NotImplementedException();

		public IFont? CreateFont(FontInfo fontInfo, bool exactMatchOnly = false)
			=> _typographyExtension.CreateFont(this, fontInfo, exactMatchOnly);

		public IImage? CreateImage(string url)
			=> _imageExtension.CreateImage(this, url);

		public void Clear(Color32 color)
		{
			_canvas.Clear(color.ToSKColor());
		}

		public void DrawLines(ReadOnlySpan<Vector2d> points, bool closePolygon, DrawStyle style)
		{
			SKPoint[] skPoints = new SKPoint[points.Length];
			for (int i = 0; i < skPoints.Length; i++)
				skPoints[i] = points[i].ToSKPoint();

			ApplyStyle(style);

			_paint.Style = SKPaintStyle.Stroke;
			_canvas.DrawPoints(closePolygon ? SKPointMode.Polygon : SKPointMode.Lines, skPoints, _paint);
			_paint.Style = SKPaintStyle.Fill;
		}

		public void DrawImage(IImage image, Rect2d sourceRect, Rect2d destRect, DrawStyle style)
		{
			ApplyStyle(style);
			_imageExtension.DrawImage(this, image, sourceRect, destRect, _paint, style);
		}

		public void DrawText(Vector2d topLeftCorner, IShapedText shapedText, DrawStyle style)
		{
			ApplyStyle(style);
			_typographyExtension.DrawText(this, topLeftCorner, shapedText, _paint, style);
		}

		public void End()
		{
			_canvas.Restore();
		}

		public void FillPolygon(ReadOnlySpan<Vector2d> points, DrawStyle style)
		{
			ApplyStyle(style);

			SKPoint[] skPoints = new SKPoint[points.Length];
			for (int i = 0; i < skPoints.Length; i++)
				skPoints[i] = points[i].ToSKPoint();

			using SKPath skPath = new SKPath();
			skPath.AddPoly(skPoints);

			_canvas.DrawPath(skPath, _paint);
		}

		public void FillRect(Rect2d rect, DrawStyle style)
		{
			ApplyStyle(style);
			_canvas.DrawRect(rect.ToSKRect(), _paint);
		}

		public void FillRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style)
		{
			ApplyStyle(style);
			DrawRoundRectInternal(rect, radii);
		}

		public void DrawRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style)
		{
			ApplyStyle(style);
			_paint.Style = SKPaintStyle.Stroke;
			DrawRoundRectInternal(rect, radii);
			_paint.Style = SKPaintStyle.Fill;
		}

		private void DrawRoundRectInternal(Rect2d rect, CornerRadii radii)
		{
			const double Epsilon = 0.000001;

			if (rect.Width < Epsilon || rect.Height < Epsilon || radii.IsEffectivelyZero)
			{
				// Degenerate case: It's just a rectangle, not rounded, or too small
				// for rounding to be visible.
				_canvas.DrawRect(rect.ToSKRect(), _paint);
				return;
			}

			// General case: Four elliptical radii, one for each corner.
			_roundRect = new SKRoundRect();

			_roundRect.SetRectRadii(rect.ToSKRect(),
			[
				new SKPoint((float)radii.TopLeft.X,     (float)radii.TopLeft.Y),
				new SKPoint((float)radii.TopRight.X,    (float)radii.TopRight.Y),
				new SKPoint((float)radii.BottomRight.X, (float)radii.BottomRight.Y),
				new SKPoint((float)radii.BottomLeft.X,  (float)radii.BottomLeft.Y),
			]);

			_canvas.DrawRoundRect(_roundRect, _paint);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ApplyStyle(DrawStyle style)
		{
			if (style == _lastDrawStyle)
				return;
			ApplyStyleForReal(style);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void ApplyStyleForReal(DrawStyle style)
		{
			if (style.Color != _lastDrawStyle.Color
				|| style.Brush != _lastDrawStyle.Brush
				|| style.BrushRect != _lastDrawStyle.BrushRect)
			{
				if (style.Color.HasValue)
				{
					_paint.Color = style.Color.Value.ToSKColor();
					_paint.Shader = null;
				}
				else if (style.Brush is not ISkiaBrush skiaBrush)
				{
					System.Diagnostics.Debug.WriteLine("Warning: Drawing without a color or brush; defaulting to black.");
					_paint.Color = Color32.Black.ToSKColor();
					_paint.Shader = null;
				}
				else skiaBrush.Apply(_paint, style.BrushRect);
			}

			if (style.LineThickness != _lastDrawStyle.LineThickness)
			{
				_paint.StrokeWidth = (float)style.LineThickness;
			}

			if (style.Transform != _lastDrawStyle.Transform)
			{
				_canvas.SetMatrix(style.Transform.ToSKMatrix());
			}

			_lastDrawStyle = style;
		}
	}
}
