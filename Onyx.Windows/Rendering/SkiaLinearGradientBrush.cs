using Onyx.Css.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	internal class SkiaLinearGradientBrush : SkiaBrush, IDisposable
	{
		public SKShader Shader { get; }

		private static SKColor _white = new SKColor(255, 255, 255, 255);

		public SkiaLinearGradientBrush(LinearGradient linearGradient)
		{
			IReadOnlyList<ColorStop> colorStops = linearGradient.ColorStops;

			SKColor[] colors = new SKColor[colorStops.Count];
			float[] offsets = new float[colorStops.Count];

			for (int i = 0; i < colorStops.Count; i++)
			{
				colors[i] = colorStops[i].Color?.ToSKColor() ?? new SKColor(255, 255, 255);
				offsets[i] = (float)colorStops[i].Measure.Value;
			}

			Shader = SKShader.CreateLinearGradient(
				new SKPoint(0, 0), new SKPoint(1, 1),
				colors, offsets, SKShaderTileMode.Clamp);
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
				Shader.Dispose();
		}

		public override void Apply(SKPaint paint)
		{
			if (paint.Shader != null)
				paint.Shader = Shader;
			if (paint.Color != _white)
				paint.Color = _white;
		}
	}
}
