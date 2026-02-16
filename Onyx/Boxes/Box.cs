using Onyx.Css.Computed;
using Onyx.Html.Dom;
using Onyx.Types;

namespace Onyx.Boxes
{
	/// <summary>
	/// A box is the base class of all rendering:  This has a physical
	/// position and size, an optional reference to an element that generated it,
	/// and an optional parent box that contains it.
	/// </summary>
	public abstract class Box
	{
		/// <summary>
		/// Flags describing the box's state.
		/// </summary>
		public BoxFlags Flags { get; internal set; }

		/// <summary>
		/// The parent box of this box, if any.
		/// </summary>
		public Box? Parent { get; internal set; }

		/// <summary>
		/// The document that owns this box.
		/// </summary>
		public Document? Document { get; internal set; }

		/// <summary>
		/// The element that generated this box (and possibly others).
		/// </summary>
		public Element? Element { get; internal set; }

		/// <summary>
		/// The style applied to this box.
		/// </summary>
		public ComputedStyle? ComputedStyle { get; internal set; }

		/// <summary>
		/// The calculated rendering coordinates of this box.
		/// </summary>
		public Rect2d Rect { get; internal set; }
	}
}
