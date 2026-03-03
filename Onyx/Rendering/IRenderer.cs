using Onyx.Types;

namespace Onyx.Rendering
{
	public interface IRenderer
	{
		void Begin();	// Invoked before any drawing on a surface
		void End();     // Invoked after all drawing on a surface is complete

		void Clear(Color32 color);

		void FillRect(Rect2d rect, DrawStyle style);
		void DrawLines(ReadOnlySpan<Vector2d> points, bool closePolygon, DrawStyle style);
		void FillPolygon(ReadOnlySpan<Vector2d> points, DrawStyle style);
		void DrawText(Vector2d topLeftCorner, ReadOnlySpan<char> text, DrawStyle style);
		void DrawImage(IImage image, Rect2d sourceRect, Vector2d dest, DrawStyle style);
		void FillRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style);
		void DrawRoundRect(Rect2d rect, CornerRadii radii, DrawStyle style);
	}
}
