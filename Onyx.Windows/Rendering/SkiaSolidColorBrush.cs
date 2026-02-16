using Onyx.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	internal class SkiaSolidColorBrush : SkiaBrush
	{
		public SKColor Color { get; }

		public SkiaSolidColorBrush(Color32 color)
		{
			Color = color.ToSKColor();
		}

		public override void Apply(SKPaint paint)
		{
			if (Color != paint.Color)
				paint.Color = Color;
			if (paint.Shader != null)
				paint.Shader = null;
		}
	}
}
