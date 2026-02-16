using Onyx.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	public static class PointExtensions
	{
		public static SKPoint ToSKPoint(this Vector2d point)
			=> new SKPoint((float)point.X, (float)point.Y);
	}
}
