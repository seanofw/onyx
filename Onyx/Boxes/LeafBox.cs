using System.Runtime.CompilerServices;
using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A box with no children.
	/// </summary>
	public abstract class LeafBox : Box
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected LeafBox(BoxKind kind, Element element, ComputedStyle computedStyle)
			: base(kind, element, computedStyle, null)
		{
		}
	}
}
