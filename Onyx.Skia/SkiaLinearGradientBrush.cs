using Onyx.Css.Types;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public class SkiaLinearGradientBrush : ISkiaBrush, IDisposable
	{
		private SKShader? _shader;

		private Rect2d _lastRect;

		private LinearGradient _linearGradient;

		private static SKColor _white = new SKColor(255, 255, 255, 255);

		public SkiaLinearGradientBrush(LinearGradient linearGradient)
		{
			_linearGradient = linearGradient;

			_lastRect = new Rect2d(double.PositiveInfinity, double.PositiveInfinity,
				double.NegativeInfinity, double.NegativeInfinity);
		}

		~SkiaLinearGradientBrush()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				_shader?.Dispose();
				_shader = null;
			}
		}

		public void Apply(SKPaint paint, Rect2d rect)
		{
			if (rect != _lastRect || _shader == null || paint.Shader != _shader)
			{
				IReadOnlyList<ColorStop> colorStops = _linearGradient.ColorStops;

				if (double.IsInfinity(rect.Width) || double.IsInfinity(rect.Height))
				{
					System.Diagnostics.Debug.WriteLine("Warning: Linear gradient brush has infinite bounding rectangle");
					paint.Shader = null;
					paint.Color = colorStops.Count > 0
						? (colorStops[0].Color ?? Color32.Black).ToSKColor()
						: new SKColor(0);
					return;
				}

				(Vector2d start, Vector2d end) = _linearGradient.GetStartAndEndPoints(rect);
				IReadOnlyList<(Color32 Color, double Offset)> resolvedColorStops = _linearGradient.CalculateColorStops((end - start).Length);

				SKColor[] colors = new SKColor[resolvedColorStops.Count];
				float[] offsets = new float[resolvedColorStops.Count];

				for (int i = 0; i < resolvedColorStops.Count; i++)
				{
					colors[i] = resolvedColorStops[i].Color.ToSKColor();
					offsets[i] = (float)resolvedColorStops[i].Offset;
				}

				_shader?.Dispose();
				_shader = SKShader.CreateLinearGradient(
					start.ToSKPoint(), end.ToSKPoint(), colors, offsets, SKShaderTileMode.Clamp);
				paint.Shader = _shader;
			}

			if (paint.Color != _white)
				paint.Color = _white;
		}
	}
}
