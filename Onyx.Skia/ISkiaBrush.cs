using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public interface ISkiaBrush : IBrush
	{
		void Apply(SKPaint paint, Rect2d rect);
	}
}
