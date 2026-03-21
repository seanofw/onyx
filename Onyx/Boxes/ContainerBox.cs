using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A container box can hold multiple children.
	/// </summary>
	public abstract class ContainerBox : Box
	{
		protected ContainerBox(BoxKind kind, Element element, ComputedStyle computedStyle,
			IEnumerable<Box> children)
			: base(kind, element, computedStyle, children)
		{
		}
	}
}
