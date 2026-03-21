namespace Onyx.Rendering
{
	/// <summary>
	/// An opaque handle to a backend-managed paint source such as a gradient or pattern.
	/// The core engine holds and passes brush references through DrawStyle but knows nothing
	/// about how a brush paints; all logic is private to the backend implementation.
	/// Brushes are mutually exclusive with a flat Color in DrawStyle: setting one clears
	/// the other. The layout engine caches brushes for the lifetime of the computed style
	/// that produced them.
	/// </summary>
	public interface IBrush : IDisposable { }
}
