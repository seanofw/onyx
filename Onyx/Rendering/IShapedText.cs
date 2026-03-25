namespace Onyx.Rendering
{
	/// <summary>
	/// A chunk of text whose glyphs have been shaped for display.
	/// </summary>
	public interface IShapedText : IDisposable
	{
		/// <summary>
		/// The bounding box of the rendered glyphs, the advance distance (how far the
		/// cursor moves after this run), and the tight ink bounds, which may extend
		/// outside the bounding box for glyphs with unusual metrics.
		/// </summary>
		TextMetrics TextMetrics { get; }
	}
}
