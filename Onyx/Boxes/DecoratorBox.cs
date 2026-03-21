using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A decorator box holds at most one child box.
	/// </summary>
	public abstract class DecoratorBox : Box
	{
		/// <summary>
		/// The single child.
		/// </summary>
		public Box? Child => _children?.FirstOrDefault();

		protected DecoratorBox(BoxKind kind, Element element, ComputedStyle computedStyle, Box? child)
			: base(kind, element, computedStyle, child != null ? [child] : null)
		{
		}
	}
}
