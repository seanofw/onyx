using Onyx.Types;

namespace Onyx.Rendering
{
	/// <summary>
	/// Contract for a rendering backend. Draw methods are called inside tight rendering
	/// loops and must be as fast as possible.
	/// </summary>
	public interface IRenderer
	{
		/// <summary>
		/// Save renderer state. Must be called before any drawing in a render pass,
		/// and must be paired with a call to End().
		/// </summary>
		void Begin();

		/// <summary>
		/// Restore renderer state. Must be called after all drawing in a render pass is
		/// complete, even if no draw calls were made.
		/// </summary>
		void End();

		/// <summary>
		/// Fill the entire rendering surface with a solid color, discarding all prior content.
		/// </summary>
		/// <param name="color">The fill color. The alpha channel is honored.</param>
		void Clear(Color32 color);

		/// <summary>
		/// Fill an axis-aligned rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to fill, in screen coordinates.</param>
		/// <param name="style">The fill style. Color or Brush determines the paint source.</param>
		void FillRect(Rect2d rect, DrawStyle style);

		/// <summary>
		/// Draw the outline of a polyline or closed polygon.
		/// </summary>
		/// <param name="points">
		/// The vertices, in order. At least two points are required; passing fewer
		/// produces no output.
		/// </param>
		/// <param name="closePolygon">
		/// If true, an additional segment connects the last point back to the first.
		/// If false, the path is left open.
		/// </param>
		/// <param name="style">
		/// The stroke style. LineThickness and LineStyle control the stroke appearance.
		/// </param>
		void DrawLines(ReadOnlySpan<Vector2d> points, bool closePolygon, DrawStyle style);

		/// <summary>
		/// Fill a polygon. The polygon need not be convex.
		/// </summary>
		/// <param name="points">
		/// The vertices, in order. At least three points are required; passing fewer
		/// produces no output.
		/// </param>
		/// <param name="style">The fill style. Color or Brush determines the paint source.</param>
		void FillPolygon(ReadOnlySpan<Vector2d> points, DrawStyle style);

		/// <summary>
		/// Draw a run of text.
		/// </summary>
		/// <param name="topLeftCorner">
		/// The top-left corner of the text bounding box in screen coordinates. This is not
		/// the baseline position; use FontMetrics.Ascent to find the baseline from here.
		/// </param>
		/// <param name="shapedText">
		/// The text to render, which must be shaped text from a known font.
		/// </param>
		/// <param name="style">
		/// The draw style. Font must be set; if it is null the renderer may substitute a
		/// default font or produce no output.
		/// </param>
		void DrawText(Vector2d topLeftCorner, IShapedText shapedText, DrawStyle style);

		/// <summary>
		/// Draw a region of an image at a position on screen.
		/// </summary>
		/// <param name="image">The image to draw. Must not be disposed.</param>
		/// <param name="sourceRect">
		/// The region of the image to sample, in image-local coordinates. Values outside
		/// the image bounds are clamped or wrapped depending on the backend.
		/// </param>
		/// <param name="destRect">The destination rectangle in screen coordinates.
		/// The image will be stretched or squashed as necessary to fit this rectangle.</param>
		/// <param name="style">The draw style. Transform, Opacity, and Clip are applied.</param>
		void DrawImage(IImage image, Rect2d sourceRect, Rect2d destRect, DrawStyle style);

		/// <summary>
		/// Fill a rectangle with rounded corners.
		/// </summary>
		/// <param name="rect">The bounding rectangle, in screen coordinates.</param>
		/// <param name="radii">
		/// The elliptical radius at each corner. If all radii are effectively zero,
		/// the call degenerates to a plain filled rectangle.
		/// </param>
		/// <param name="style">The fill style. Color or Brush determines the paint source.</param>
		void FillRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style);

		/// <summary>
		/// Draw the outline of a rectangle with rounded corners.
		/// </summary>
		/// <param name="rect">The bounding rectangle, in screen coordinates.</param>
		/// <param name="radii">
		/// The elliptical radius at each corner. If all radii are effectively zero,
		/// the call degenerates to a plain rectangle outline.
		/// </param>
		/// <param name="style">
		/// The stroke style. LineThickness and LineStyle control the stroke appearance.
		/// </param>
		void DrawRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style);
	}
}
