using System.Runtime.CompilerServices;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public static class RectExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SKRect ToSKRect(this Rect2d rect)
			=> new SKRect((float)rect.X, (float)rect.Y,
					(float)(rect.X + rect.Width), (float)(rect.Y + rect.Height));
	}
}
