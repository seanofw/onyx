namespace Onyx.Rendering
{
	/// <summary>
	/// A live, loaded font handle with metric data and text measurement capabilities.
	/// Obtained from IRenderables.CreateFont(); cached and shared across all elements
	/// that use the same family, size, style, and weight.
	/// </summary>
	public interface IFont : IDisposable
	{
		/// <summary>
		/// The request that produced this font. Used as a cache key by the layout engine
		/// so that fonts are loaded once and reused rather than reloaded per element.
		/// </summary>
		FontInfo FontInfo { get; }

		/// <summary>
		/// Measure the visual extent of a text run in this font.
		/// </summary>
		/// <param name="text">
		/// The characters to measure. An empty span returns a zero-size TextMetrics.
		/// </param>
		/// <returns>
		/// The bounding box of the rendered glyphs, the advance distance (how far the
		/// cursor moves after this run), and the tight ink bounds, which may extend
		/// outside the bounding box for glyphs with unusual metrics.
		/// </returns>
		TextMetrics MeasureText(ReadOnlySpan<char> text);

		/// <summary>
		/// Metric data for this font at its loaded size: line height, ascent, descent,
		/// decoration positions, and em/en/ex values. Constant for the lifetime of the font.
		/// </summary>
		FontMetrics FontMetrics { get; }
	}
}
