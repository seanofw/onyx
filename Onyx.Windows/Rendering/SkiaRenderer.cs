using Onyx.Css.Types;
using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	public class SkiaRenderer : IRenderer, IRenderables
	{
		public IClipper? Clip { get; set; }

		public double Opacity
		{
			get => _opacity;
			set
			{
				if (_opacity != value)
				{
					_opacity = value;
				}
			}
		}
		private double _opacity = 1.0;

		public Matrix3d Transform
		{
			get => _transform;
			set
			{
				if (_transform != value)
				{
					_transform = value;
					_canvas.SetMatrix(value.ToSKMatrix());
				}
			}
		}
		private Matrix3d _transform = Matrix3d.Identity;

		private readonly SKCanvas _canvas;
		private SKPaint _paint;

		public SkiaRenderer(SKCanvas canvas)
		{
			_canvas = canvas;
			_paint = new SKPaint();
		}

		public void Begin()
		{
			_canvas.Save();
		}

		public IClipper CreateClipper(ReadOnlySpan<Vector2d> convexPolygon)
			=> new SkiaClipper(convexPolygon);

		public IFont? CreateFont(FontInfo fontInfo, bool exactMatchOnly)
			=> throw new NotImplementedException();

		public IImage? CreateImage(string url)
			=> throw new NotImplementedException();

		public IBrush CreateLinearGradientBrush(LinearGradient linearGradient)
			=> new SkiaLinearGradientBrush(linearGradient);

		public IBrush CreateRadialGradientBrush(RadialGradient radialGradient)
			=> throw new NotImplementedException();

		public IBrush CreateSolidBrush(Color32 color)
			=> new SkiaSolidColorBrush(color);

		public void Clear(Color32 color)
		{
			_canvas.Clear(color.ToSKColor());
		}

		public void DrawImage(IImage image, Rect2d sourceRect, Vector2d dest)
		{
			throw new NotImplementedException();
		}

		public void DrawLines(ReadOnlySpan<Vector2d> points, bool closePolygon, IBrush brush,
			double thickness, LineStyle lineStyle)
		{
			if (brush is not SkiaBrush skiaBrush)
				throw new NotSupportedException("This method requires a SkiaBrush.");

			SKPoint[] skPoints = new SKPoint[points.Length];
			for (int i = 0; i < skPoints.Length; i++)
				skPoints[i] = points[i].ToSKPoint();

			skiaBrush.Apply(_paint);
			_paint.StrokeWidth = (float)thickness;
			_paint.Style = SKPaintStyle.Stroke;

			_canvas.DrawPoints(closePolygon ? SKPointMode.Polygon : SKPointMode.Lines, skPoints, _paint);

			_paint.Style = SKPaintStyle.Fill;
		}

		public void DrawText(Vector2d topLeftCorner, IFont font, ReadOnlySpan<char> text, IBrush brush)
		{
			throw new NotImplementedException();
		}

		public void End()
		{
			_canvas.Restore();
		}

		public void FillPolygon(ReadOnlySpan<Vector2d> points, IBrush brush)
		{
			if (brush is not SkiaBrush skiaBrush)
				throw new NotSupportedException("This method requires a SkiaBrush.");

			SKPoint[] skPoints = new SKPoint[points.Length];
			for (int i = 0; i < skPoints.Length; i++)
				skPoints[i] = points[i].ToSKPoint();

			using SKPath skPath = new SKPath();
			skPath.AddPoly(skPoints);

			skiaBrush.Apply(_paint);
			_canvas.DrawPath(skPath, _paint);
		}

		public void FillRect(Rect2d rect, IBrush brush)
		{
			if (brush is not SkiaBrush skiaBrush)
				throw new NotSupportedException("This method requires a SkiaBrush.");

			skiaBrush.Apply(_paint);
			_canvas.DrawRect(new SKRect((float)rect.X, (float)rect.Y,
				(float)(rect.X + rect.Width), (float)(rect.Y + rect.Height)),
				_paint);
		}
	}
}
