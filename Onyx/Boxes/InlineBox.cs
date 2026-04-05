using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Html.Dom;
using Onyx.Types;

namespace Onyx.Boxes
{
	/// <summary>
	/// An Inline box performs left-to-right top-to-bottom paragraph-style layout
	/// on its children, optionally with line wrapping, and obeys the provided
	/// alignment and wrapping modes.
	/// </summary>
	public sealed class InlineBox : ContainerBox
	{
		public bool Wrap { get; }
		public TextAlign Align { get; }

		public InlineBox(Element element, IEnumerable<Box>? children, ComputedStyle computedStyle,
			bool wrap, TextAlign textAlign)
			: base(BoxKind.Inline, element, computedStyle, children ?? Array.Empty<Box>())
		{
			Wrap = wrap;
			Align = textAlign;
		}

		//protected override Size2d MeasureInternal(Size2d availableSize)
		//{
		//	Size2d measuredSize = new Size2d(0, 0);

		//	foreach (Box child in Children)
		//	{
		//		Size2d childSize = child.Measure(availableSize);
		//		measuredSize = new Size2d(measuredSize.Width + childSize.Width,
		//			Math.Max(measuredSize.Height, childSize.Height));
		//	}

		//	return measuredSize;
		//}

		protected override void ArrangeInternal(Rect2d containerRect)
		{
			Vector2d topLeft = containerRect.TopLeft;
			foreach (Box child in Children)
			{
				double childWidth = child.DesiredSize.GetValueOrDefault().Width;
				child.Arrange(new Rect2d(topLeft, new Size2d(childWidth, containerRect.Height)));
			}
		}
	}
}
