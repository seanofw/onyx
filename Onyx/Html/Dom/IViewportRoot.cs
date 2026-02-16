using Onyx.Types;

namespace Onyx.Html.Dom
{
    public interface IViewportRoot
    {
		Rect2d ViewportRect { get; set; }
		Rect2d DocumentRect { get; }
    }
}
