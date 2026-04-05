using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A float box pulls its content out of the inline flow to the left or right side,
	/// and creates margins within the inline flow around its content.
	/// </summary>
	public sealed class FloatBox : DecoratorBox
	{
		public FloatBox(Element element, ComputedStyle computedStyle, Box? child)
			: base(BoxKind.Float, element, computedStyle, child)
		{
		}
	}
}
