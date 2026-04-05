using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A Background box is a decorator that renders a background behind its content.
	/// </summary>
	public sealed class BackgroundBox : DecoratorBox
	{
		public BackgroundBox(Element element, ComputedStyle computedStyle, Box? child)
			: base(BoxKind.Background, element, computedStyle, child)
		{
		}
	}
}
