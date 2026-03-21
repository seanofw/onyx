namespace Onyx.Rendering
{
	/// <summary>
	/// Describes how corners are rendered where two line segments meet.
	/// Corresponds to the CSS <c>stroke-linejoin</c> property.
	/// </summary>
	public enum LineJoin
	{
		/// <summary>No join style specified; the renderer will use its own default.</summary>
		Unknown = 0,

		/// <summary>
		/// A sharp pointed join that extends to the intersection of the outer stroke edges.
		/// Falls back to a bevel join when the miter length exceeds <see cref="LineStyle.MiterLimit"/>
		/// times the line thickness.
		/// </summary>
		Miter,

		/// <summary>A rounded join centered at the corner vertex.</summary>
		Round,

		/// <summary>A flat join that cuts across the corner, connecting the outer stroke edges directly.</summary>
		Bevel,
	}
}
