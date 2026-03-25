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
		/// Generate "shaped text" for the given string.  "Shaped text" is an opaque
		/// object, renderer-specific, that can be measured and can be drawn on a canvas,
		/// and typically contains a sequence of glyphs.  It is assumed to require Dispose()
		/// to release whatever resources it may hold.
		/// </summary>
		/// <param name="text">The text to be shaped.</param>
		/// <returns>An object representing the shaped text.</returns>
		IShapedText ShapeText(ReadOnlySpan<char> text);

		/// <summary>
		/// Metric data for this font at its loaded size: line height, ascent, descent,
		/// decoration positions, and em/en/ex values. Constant for the lifetime of the font.
		/// </summary>
		FontMetrics FontMetrics { get; }
	}

	public static class IFontExtensions
	{
		/// <summary>
		/// Measure the given text.  This is less efficient than invoking font.ShapeText()
		/// and then keeping the result, but it *is* simpler to use.
		/// </summary>
		/// <param name="font">The font that can transform the text characters into a glyph sequence.</param>
		/// <param name="text">The text string to be shaped.</param>
		/// <returns>TextMetrics that describe the dimensions and advance of the given text.</returns>
		public static TextMetrics MeasureText(this IFont font, ReadOnlySpan<char> text)
		{
			using IShapedText shapedText = font.ShapeText(text);
			return shapedText.TextMetrics;
		}
	}
}
