namespace Onyx.Css.Selectors
{
	/// <summary>
	/// The supported set of CSS combinators.
	/// </summary>
	public enum Combinator
	{
		/// <summary>
		/// None (reserved).
		/// </summary>
		None = 0,

		/// <summary>
		/// Descendant relationship (A B).
		/// </summary>
		Descendant,

		/// <summary>
		/// Adjacent-sibling relationship (A + B).
		/// </summary>
		AdjacentSibling,

		/// <summary>
		/// General-sibling relationship (A ~ B).
		/// </summary>
		GeneralSibling,

		/// <summary>
		/// Immediate child relationship (A > B).
		/// </summary>
		Child,

		/// <summary>
		/// Self relationship (A) (used to represent an element or the special '*' universal selector).
		/// </summary>
		Self,
	}
}
