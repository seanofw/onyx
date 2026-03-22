using Onyx.Types;

namespace Onyx.Rendering
{
	/// <summary>
	/// An immutable bundle of style parameters passed to every IRenderer draw call.
	/// The typical pattern is to start from DrawStyle.Default and derive working styles
	/// using the With*() fluent methods.
	/// </summary>
	public class DrawStyle
	{
		/// <summary>The active clip region, or null for no clipping.</summary>
		public IClipper? Clip { get; }

		/// <summary>Paint opacity, where 0.0 is fully transparent and 1.0 is fully opaque.</summary>
		public double Opacity { get; }

		/// <summary>The font used by DrawText, or null to let the renderer choose a default.</summary>
		public IFont? Font { get; }

		/// <summary>
		/// The flat paint color, or null if a Brush is active. Color and Brush are mutually
		/// exclusive; setting one via the constructor or a With*() method clears the other.
		/// </summary>
		public Color32? Color { get; }

		/// <summary>
		/// The gradient or pattern paint source, or null if a flat Color is active. Color and
		/// Brush are mutually exclusive; setting one via the constructor or a With*() method
		/// clears the other.
		/// </summary>
		public IBrush? Brush { get; }

		/// <summary>
		/// The bounding rectangle used to anchor the Brush. For a linear gradient this
		/// determines where the gradient starts and ends. Ignored when Color is active.
		/// </summary>
		public Rect2d BrushRect { get; }

		/// <summary>Stroke width in screen pixels, used by line and outline draw calls.</summary>
		public double LineThickness { get; }

		/// <summary>Stroke dash pattern, used by line and outline draw calls.</summary>
		public LineStyle LineStyle { get; }

		/// <summary>Affine transform applied to all coordinates in the draw call.</summary>
		public Matrix3x2d Transform { get; }

		/// <summary>
		/// A sensible starting point: black, fully opaque, 1px solid stroke, identity
		/// transform, no clip, no font. Build working styles from this using the With*() methods.
		/// </summary>
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

		/// <summary>
		/// Construct a new DrawStyle.
		/// </summary>
		/// <param name="clip">The clip region. Null means no clipping.</param>
		/// <param name="opacity">Paint opacity. 0.0 is fully transparent, 1.0 is fully opaque.</param>
		/// <param name="font">The font for DrawText calls. Null lets the renderer choose a default.</param>
		/// <param name="color">
		/// The flat paint color. If set, brush is ignored regardless of the brush parameter.
		/// </param>
		/// <param name="brush">
		/// The gradient or pattern paint source. Ignored if color has a value.
		/// </param>
		/// <param name="brushRect">
		/// The bounding rectangle used to anchor the brush. Default(Rect2d) is a zero rect,
		/// which will produce degenerate output for gradient brushes; DrawStyle.Default uses
		/// an infinite rect as a safe sentinel.
		/// </param>
		/// <param name="lineThickness">Stroke width in screen pixels for line and outline calls.</param>
		/// <param name="lineStyle">
		/// Stroke dash pattern and geometric style. Default(LineStyle) is null; DrawStyle.Default
		/// uses LineStyle.Solid explicitly. A null value is treated as a solid line by renderers.
		/// </param>
		/// <param name="transform">
		/// Affine transform applied to draw call coordinates. Default(Matrix3x2d) is a zero
		/// matrix, not identity; DrawStyle.Default uses Matrix3x2d.Identity.
		/// </param>
		public DrawStyle(
			IClipper? clip = null,
			double opacity = 0.0,
			IFont? font = null,
			Color32? color = default,
			IBrush? brush = null,
			Rect2d brushRect = default,
			double lineThickness = 0.0,
			LineStyle? lineStyle = default,
			Matrix3x2d transform = default)
		{
			Clip = clip;
			Opacity = opacity;
			Font = font;
			Color = color;
			Brush = color.HasValue ? null : brush;
			BrushRect = brushRect;
			LineThickness = lineThickness;
			LineStyle = lineStyle ?? LineStyle.Solid;
			Transform = transform;
		}

		/// <summary>Copy this object, replacing Clip.</summary>
		public DrawStyle WithClip(IClipper? clip)
			=> new DrawStyle(clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing Opacity.</summary>
		public DrawStyle WithOpacity(double opacity)
			=> new DrawStyle(Clip, opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing Font.</summary>
		public DrawStyle WithFont(IFont? font)
			=> new DrawStyle(Clip, Opacity, font, Color, Brush, BrushRect, LineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing Brush. Clears Color.</summary>
		public DrawStyle WithBrush(IBrush? brush)
			=> new DrawStyle(Clip, Opacity, Font, null, brush, BrushRect, LineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing Color. Clears Brush.</summary>
		public DrawStyle WithColor(Color32? color)
			=> new DrawStyle(Clip, Opacity, Font, color, null, BrushRect, LineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing BrushRect.</summary>
		public DrawStyle WithBrushRect(Rect2d brushRect)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, brushRect, LineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing Brush and BrushRect. Clears Color.</summary>
		public DrawStyle WithBrush(IBrush? brush, Rect2d brushRect)
			=> new DrawStyle(Clip, Opacity, Font, null, brush, brushRect, LineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing LineThickness.</summary>
		public DrawStyle WithLineThickness(double lineThickness)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, lineThickness, LineStyle, Transform);
		/// <summary>Copy this object, replacing LineStyle.</summary>
		public DrawStyle WithLineStyle(LineStyle lineStyle)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, lineStyle, Transform);
		/// <summary>Copy this object, replacing Transform.</summary>
		public DrawStyle WithTransform(Matrix3x2d transform)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle, transform);
		/// <summary>Copy this object, replacing Transform, converting from a 3x3 matrix.</summary>
		public DrawStyle WithTransform(Matrix3d transform)
			=> new DrawStyle(Clip, Opacity, Font, Color, Brush, BrushRect, LineThickness, LineStyle,
				new Matrix3x2d(transform));
	}
}
