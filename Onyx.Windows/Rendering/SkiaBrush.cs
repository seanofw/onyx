using Onyx.Rendering;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	internal abstract class SkiaBrush : IBrush
	{
		public abstract void Apply(SKPaint paint);
	}
}
