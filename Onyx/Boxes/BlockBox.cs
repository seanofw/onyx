using Onyx.Css.Computed;
using Onyx.Html.Dom;
using Onyx.Types;

namespace Onyx.Boxes
{
	/// <summary>
	/// A Block box performs block top-to-bottom layouts on its children.
	/// </summary>
	public class BlockBox : ContainerBox
	{
		public override Thickness2d Margin => _margin;
		private Thickness2d _margin;
		private Thickness2d _assignedMargin;

		public override Thickness2d Padding => _padding;
		private Thickness2d _padding;

		public BlockBox(Element element, ComputedStyle computedStyle, IEnumerable<Box>? children)
			: base(BoxKind.Block, element, computedStyle, children ?? Array.Empty<Box>())
		{
		}

		protected override Size2d MeasureInternal(Size2d availableSize, MeasureContext context)
		{
			_padding = new Thickness2d(
				ResolveMeasure(ComputedStyle.PaddingTop, PercentMode.UseWidth, context),
				ResolveMeasure(ComputedStyle.PaddingRight, PercentMode.UseWidth, context),
				ResolveMeasure(ComputedStyle.PaddingBottom, PercentMode.UseWidth, context),
				ResolveMeasure(ComputedStyle.PaddingLeft, PercentMode.UseWidth, context)
			);

			availableSize = new Size2d(
				availableSize.Width - _padding.Left - _padding.Right,
				availableSize.Height - _padding.Top - _padding.Bottom);

			double    topMargin = ResolveMeasure(ComputedStyle.MarginTop, PercentMode.UseWidth, context);
			double  rightMargin = ResolveMeasure(ComputedStyle.MarginRight, PercentMode.UseWidth, context);
			double bottomMargin = ResolveMeasure(ComputedStyle.MarginBottom, PercentMode.UseWidth, context);
			double   leftMargin = ResolveMeasure(ComputedStyle.MarginLeft, PercentMode.UseWidth, context);
			_assignedMargin = new Thickness2d(topMargin, rightMargin, bottomMargin, leftMargin);

			double measuredWidth = 0;
			double measuredHeight = 0;

			double previousMargin = 0;
			bool exposeTopMargin = _padding.Top <= 0;
			bool exposeBottomMargin = _padding.Bottom <= 0;

			foreach (Box child in Children)
			{
				Size2d childSize = child.Measure(availableSize, context);

				Thickness2d childMargin = child.Margin;

				if (double.IsInfinity(childSize.Width) || double.IsInfinity(childSize.Height))
					throw new InvalidOperationException("Child box has an infinite desired size.");
				if (childSize.Width < 0 || childSize.Height < 0)
					throw new InvalidOperationException("Child box has a negative desired size.");

				double usedHeight;
				if (exposeTopMargin)
				{
					topMargin = CollapseMargins(topMargin, childMargin.Top);
					usedHeight = childSize.Height;
					exposeTopMargin = false;
				}
				else usedHeight = CollapseMargins(previousMargin, childMargin.Top) + childSize.Height;

				measuredWidth = Math.Max(measuredWidth, childMargin.Left + childSize.Width + childMargin.Right);
				measuredHeight += usedHeight;

				previousMargin = childMargin.Bottom;
			}

			if (exposeBottomMargin)
				bottomMargin = CollapseMargins(bottomMargin, previousMargin);
			else
				measuredHeight += previousMargin;

			_margin = new Thickness2d(topMargin, rightMargin, bottomMargin, leftMargin);

			return new Size2d(
				_padding.Left + measuredWidth + _padding.Right,
				_padding.Top + measuredHeight + _padding.Bottom);
		}

		protected override void ArrangeInternal(Rect2d containerRect)
		{
			double x = containerRect.X;
			double y = containerRect.Y;

			double previousMargin = 0;
			bool exposeTopMargin = _padding.Top <= 0;
			bool exposeBottomMargin = _padding.Bottom <= 0;

			foreach (Box child in Children)
			{
				Thickness2d childMargin = child.Margin;
				if (exposeTopMargin)
					exposeTopMargin = false;
				else
					y += CollapseMargins(childMargin.Top, previousMargin);

				double childHeight = child.DesiredSize!.Value.Height;
				child.Arrange(new Rect2d(x + childMargin.Left, y, containerRect.Width, childHeight));
				y += childHeight;

				previousMargin = childMargin.Bottom;
			}
		}

		private static double CollapseMargins(double a, double b)
		{
			if (a >= 0 && b >= 0)
				return Math.Max(a, b);
			else if (a <= 0 && b <= 0)
				return Math.Min(a, b);
			else
				return a + b;
		}
	}
}
