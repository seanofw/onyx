namespace Onyx.Boxes
{
	public enum BoxFlags : byte
	{
		None = 0,

		/// <summary>
		/// Everything needs to be completely recomputed.
		/// </summary>
		FullyInvalid = (1 << 0),

		/// <summary>
		/// This box needs to have its limits measured.
		/// </summary>
		NeedsMeasure = (1 << 1),

		/// <summary>
		/// This box needs to have its child boxes arranged.
		/// </summary>
		NeedsArrange = (1 << 2),

		/// <summary>
		/// This box needs to be painted.
		/// </summary>
		NeedsPaint = (1 << 3),
	}
}
