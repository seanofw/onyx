using System.Runtime.CompilerServices;
using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A box that has "replaced" contents, like an image or a text input.  These have
	/// no children by definition.
	/// </summary>
	public abstract class ReplacedBox : LeafBox
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected ReplacedBox(BoxKind kind, Element element, ComputedStyle computedStyle)
			: base(kind, element, computedStyle)
		{
		}
	}
}
