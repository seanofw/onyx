using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public static class PointExtensions
	{
		public static SKPoint ToSKPoint(this Vector2d point)
			=> new SKPoint((float)point.X, (float)point.Y);
	}
}
