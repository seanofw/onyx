using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A line box positions its children in a horizontal stack, and can apply the
	/// vertical-align style to align them correctly relative to each other.
	/// </summary>
	public sealed class LineBox : ContainerBox
	{
		public LineBox(Element element, ComputedStyle computedStyle, IEnumerable<Box> children)
			: base(BoxKind.Line, element, computedStyle, children)
		{
		}
	}
}
