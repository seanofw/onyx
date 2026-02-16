using Onyx.Types;

namespace Onyx.Rendering
{
	public interface IRenderer
	{
		void Begin();	// Invoked before any drawing on a surface
		void End();     // Invoked after all drawing on a surface is complete

		IClipper? Clip { get; set; }
		double Opacity { get; set; }
		Matrix3d Transform { get; set; }

		void Clear(Color32 color);
		void DrawText(Vector2d topLeftCorner, IFont font, ReadOnlySpan<char> text, IBrush brush);
		void FillRect(Rect2d rect, IBrush brush);
		void DrawLines(ReadOnlySpan<Vector2d> points, bool closePolygon, IBrush brush,
			double thickness, LineStyle lineStyle);
		void FillPolygon(ReadOnlySpan<Vector2d> points, IBrush brush);
		void DrawImage(IImage image, Rect2d sourceRect, Vector2d dest);
	}
}
