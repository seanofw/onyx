namespace Onyx.Html.Dom
{
	/// <summary>
	/// Miscellaneous flags describing this node.
	/// </summary>
	public enum NodeFlags : uint
	{
		None = 0,

		/// <summary>
		/// The high bits store the unique ID.
		/// </summary>
		UniqueIdMask = 0xFFFFU << 16,
	}
}
