using System.Runtime.CompilerServices;
using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A zero-width, zero-height marker indicating that the current LineBox must end here.
	/// </summary>
	public sealed class LineBreakBox : LeafBox
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public LineBreakBox(Element element, ComputedStyle computedStyle)
			: base(BoxKind.LineBreak, element, computedStyle)
		{
		}
	}
}
