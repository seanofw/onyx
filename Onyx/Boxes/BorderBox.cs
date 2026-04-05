using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A Border box is a decorator that renders a border around its content.
	/// </summary>
	public sealed class BorderBox : DecoratorBox
	{
		public BorderBox(Element element, ComputedStyle computedStyle, Box? child)
			: base(BoxKind.Border, element, computedStyle, child)
		{
		}
	}
}
