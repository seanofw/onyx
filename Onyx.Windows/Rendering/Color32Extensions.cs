using Onyx.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	public static class Color32Extensions
	{
		public static SKColor ToSKColor(this Color32 color)
			=> new SKColor(color.R, color.G, color.B, color.A);
	}
}
