using Onyx.Types;

namespace Onyx.Rendering
{
	public class DrawStyle
	{
		public IClipper? Clip { get; }
		public double Opacity { get; }
		public IFont? Font { get; }
		public Color32? Color { get; }
		public IBrush? Brush { get; }
		public Rect2d BrushRect { get; }
		public double LineThickness { get; }
		public LineStyle LineStyle { get; }

		// Final, optional transform to apply to the rendered content.
		public Matrix3x2d Transform { get; }

		public static DrawStyle Default { get; } = new DrawStyle(
			clip: null,
			opacity: 1.0,
			font: null,
			color: Color32.Black,
			brush: null,
			brushRect: new Rect2d(
				double.MinValue, double.MinValue,
				double.PositiveInfinity, double.PositiveInfinity),
			lineThickness: 1.0,
			lineStyle: LineStyle.Solid,
			transform: Matrix3x2d.Identity
		);

		public DrawStyle(
			IClipper? clip = null,
			double opacity = 0.0,
			IFont? font = null,
			Color32? color = default,
			IBrush? brush = null,
			Rect2d brushRect = default,
			double lineThickness = 0.0,
			LineStyle lineStyle = default,
			Matrix3x2d transform = default)
		{
			Clip = clip;
			Opacity = opacity;
			Font = font;
			Color = color;
			Brush = color.HasValue ? null : brush;
			BrushRect = brushRect;
			LineThickness = lineThickness;
			LineStyle = lineStyle;
			Transform = transform;
		}

		public DrawStyle WithClip(IClipper? clip)
			=> new DrawStyle(clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle, Transform);
		public DrawStyle WithOpacity(double opacity)
			=> new DrawStyle(Clip, opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle, Transform);
		public DrawStyle WithFont(IFont? font)
			=> new DrawStyle(Clip, Opacity, font, Color, Brush, BrushRect, LineThickness, LineStyle, Transform);
		public DrawStyle WithBrush(IBrush? brush)
			=> new DrawStyle(Clip, Opacity, Font, null, brush, BrushRect, LineThickness, LineStyle, Transform);
		public DrawStyle WithColor(Color32? color)
			=> new DrawStyle(Clip, Opacity, Font, color, null, BrushRect, LineThickness, LineStyle, Transform);
		public DrawStyle WithBrushRect(Rect2d brushRect)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, brushRect, LineThickness, LineStyle, Transform);
		public DrawStyle WithBrush(IBrush? brush, Rect2d brushRect)
			=> new DrawStyle(Clip, Opacity, Font, null, brush, brushRect, LineThickness, LineStyle, Transform);
		public DrawStyle WithLineThickness(double lineThickness)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, lineThickness, LineStyle, Transform);
		public DrawStyle WithLineStyle(LineStyle lineStyle)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, lineStyle, Transform);
		public DrawStyle WithTransform(Matrix3x2d transform)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle, transform);
		public DrawStyle WithTransform(Matrix3d transform)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle,
				new Matrix3x2d(transform));
	}
}
