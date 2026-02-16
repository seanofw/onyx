using Onyx.Types;

namespace Onyx.Rendering
{
	public readonly struct TextMetrics
	{
		public Size2d Size { get; }
		public Vector2d Advance { get; }
		public Rect2d Bounds { get; }
	}
}
