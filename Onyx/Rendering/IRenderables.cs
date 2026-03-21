using Onyx.Css.Types;
using Onyx.Types;

namespace Onyx.Rendering
{
	/// <summary>
	/// Contract for allocating backend-owned rendering resources: brushes, images, clippers,
	/// and fonts. Methods are assumed to be expensive and are called upfront, rarely, and
	/// never inside render loops. Results are cached by the layout engine.
	/// </summary>
	public interface IRenderables
	{
		/// <summary>
		/// Create a brush that paints a linear gradient.
		/// </summary>
		/// <param name="linearGradient">
		/// Describes the gradient direction and color stops. The gradient is resolved against
		/// DrawStyle.BrushRect at draw time, so one brush can serve elements of different sizes.
		/// </param>
		/// <returns>A brush handle. The caller is responsible for disposing it.</returns>
		IBrush CreateLinearGradientBrush(LinearGradient linearGradient);

		/// <summary>
		/// Create a brush that paints a radial gradient.
		/// </summary>
		/// <param name="radialGradient">
		/// Describes the gradient shape and color stops. The gradient is resolved against
		/// DrawStyle.BrushRect at draw time, so one brush can serve elements of different sizes.
		/// </param>
		/// <returns>A brush handle. The caller is responsible for disposing it.</returns>
		IBrush CreateRadialGradientBrush(RadialGradient radialGradient);

		/// <summary>
		/// Load an image from a URL and return a handle to it.
		/// </summary>
		/// <param name="url">
		/// A URL the backend can resolve. The format is backend-specific; typically a file
		/// path or a pre-registered key. Null or empty returns null.
		/// </param>
		/// <returns>
		/// An image handle, or null if the image could not be loaded. The caller is
		/// responsible for disposing the handle when it is no longer needed.
		/// </returns>
		IImage? CreateImage(string url);

		/// <summary>
		/// Create a clip region from a convex polygon.
		/// </summary>
		/// <param name="convexPolygon">
		/// The vertices of the polygon, in order. Must be convex; the backend may assume
		/// convexity for efficiency and produce incorrect results if it is violated. At least
		/// three points are required.
		/// </param>
		/// <returns>
		/// A clipper handle that can be combined with others via Union and Intersect.
		/// The caller is responsible for disposing it.
		/// </returns>
		IClipper CreateClipper(ReadOnlySpan<Vector2d> convexPolygon);

		/// <summary>
		/// Load a font and return a live handle to it.
		/// </summary>
		/// <param name="fontInfo">The family name, size, style, and weight to load.</param>
		/// <param name="exactMatchOnly">
		/// If false, the backend may return a substitute when the requested family is not
		/// available. If true, null is returned instead of a substitute, so the caller can
		/// distinguish "not found" from "found a fallback."
		/// </param>
		/// <returns>
		/// A font handle, or null if no matching font was found (or no fallback was allowed).
		/// The caller is responsible for disposing it, and must not dispose it while any
		/// element whose style references it is still active.
		/// </returns>
		IFont? CreateFont(FontInfo fontInfo, bool exactMatchOnly = false);
	}
}
