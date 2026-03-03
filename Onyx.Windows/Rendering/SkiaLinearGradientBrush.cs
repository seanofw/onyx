using Onyx.Css.Types;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	internal class SkiaLinearGradientBrush : ISkiaBrush, IDisposable
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

				(Vector2d start, Vector2d end) = CalculateStartAndEndPoints(rect, _linearGradient);
				double lineLength = Math.Max((end - start).Length, 0.000001);

				SKColor[] colors = new SKColor[colorStops.Count];
				float[] offsets = new float[colorStops.Count];

				for (int i = 0; i < colorStops.Count; i++)
				{
					colors[i] = colorStops[i].Color?.ToSKColor() ?? new SKColor(255, 255, 255);
					Measure measure = colorStops[i].Measure;
					offsets[i] = CalculateColorStopOffset(measure, lineLength);
				}

				_shader?.Dispose();
				_shader = SKShader.CreateLinearGradient(
					start.ToSKPoint(), end.ToSKPoint(), colors, offsets, SKShaderTileMode.Clamp);
				paint.Shader = _shader;
			}

			if (paint.Color != _white)
				paint.Color = _white;
		}

		private static float CalculateColorStopOffset(Measure measure, double lineLength)
			=> measure.Units switch
			{
				Units.Pixels => (float)(measure.Value / lineLength),
				Units.Percent => (float)(measure.Value / 100.0),
				_ => 0,
			};

		private static (Vector2d Start, Vector2d End) CalculateStartAndEndPoints(Rect2d rect, LinearGradient linearGradient)
		{
			double angle = linearGradient.AngleInRadians;

			// Convert CSS angle to math angle.  Because CSS is weird, that's why.  And then
			// flip it upside-down because Y points downward because we're on a screen.
			double theta = angle - (Math.PI * 0.5);

			Vector2d direction = new Vector2d(Math.Cos(theta), Math.Sin(theta));

			Vector2d center = rect.Center;

			double min = double.PositiveInfinity;
			double max = double.NegativeInfinity;

			foreach (Vector2d corner in new[]
				{ rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft })
			{
				Vector2d relative = corner - center;
				double projection = relative.Dot(direction);

				min = Math.Min(min, projection);
				max = Math.Max(max, projection);
			}

			Vector2d start = center + direction * min;
			Vector2d end   = center + direction * max;

			return (start, end);
		}
	}
}
