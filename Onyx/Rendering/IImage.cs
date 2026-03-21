using Onyx.Types;

namespace Onyx.Rendering
{
	/// <summary>
	/// A handle to a loaded image, giving the layout engine access to the sizing
	/// information needed to place and constrain image elements. Pixel data remains
	/// inside the backend.
	/// </summary>
	public interface IImage : IDisposable
	{
		/// <summary>
		/// The intrinsic pixel dimensions of the image, or null if the format has no fixed
		/// size (e.g. SVG). When null, the layout engine falls back to AspectRatio.
		/// </summary>
		Size2d? Size { get; }

		/// <summary>
		/// The minimum size at which the image may be rendered without becoming unreadable.
		/// For raster images this is typically (1, 1).
		/// </summary>
		Size2d MinSize { get; }

		/// <summary>
		/// The maximum size at which the image may be rendered without quality loss.
		/// For raster images this is the intrinsic pixel size. For vector images it is
		/// typically unbounded.
		/// </summary>
		Size2d MaxSize { get; }

		/// <summary>
		/// The intrinsic aspect ratio (width / height), or null if the image defines none.
		/// Used when only one dimension is constrained by CSS and the other must be derived.
		/// </summary>
		double? AspectRatio { get; }
	}
}
