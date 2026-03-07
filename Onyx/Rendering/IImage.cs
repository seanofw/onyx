using Onyx.Types;

namespace Onyx.Rendering
{
	public interface IImage : IDisposable
	{
		Size2d? Size { get; }
		Size2d MinSize { get; }
		Size2d MaxSize { get; }
		double? AspectRatio { get; }
	}
}
