using Onyx.Types;

namespace Onyx.Rendering
{
	public readonly struct TextMetrics
	{
		public Size2d Size { get; }
		public Vector2d Advance { get; }
		public Rect2d Bounds { get; }

		public TextMetrics(Size2d size, Vector2d advance, Rect2d bounds)
		{
			Size = size;
			Advance = advance;
			Bounds = bounds;
		}
	}
}
