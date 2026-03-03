using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	internal interface ISkiaBrush : IBrush
	{
		void Apply(SKPaint paint, Rect2d rect);
	}
}
