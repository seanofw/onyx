using Onyx.Css.Types;
using Onyx.Types;

namespace Onyx.Rendering
{
	public interface IRenderables
	{
		IBrush CreateLinearGradientBrush(LinearGradient linearGradient);
		IBrush CreateRadialGradientBrush(RadialGradient radialGradient);
		IImage? CreateImage(string url);
		IClipper CreateClipper(ReadOnlySpan<Vector2d> convexPolygon);
		IFont? CreateFont(FontInfo fontInfo, bool exactMatchOnly = false);
	}
}
