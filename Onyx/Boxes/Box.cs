using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Html.Dom;
using Onyx.Rendering;
using Onyx.Types;

namespace Onyx.Boxes
{
	/// <summary>
	/// A box is the base class of all rendering:  This has a physical
	/// position and size, an optional reference to an element that generated it,
	/// and an optional parent box that contains it.
	/// </summary>
	public abstract class Box
	{
		public BoxKind Kind { get; }

		/// <summary>
		/// The element that generated this box (and possibly others).
		/// </summary>
		public Element Element { get; }

		/// <summary>
		/// The calculated rendering coordinates of this box, relative to its parent box.
		/// </summary>
		public Rect2d Rect { get; internal set; }

		/// <summary>
		/// The computed style for this box.
		/// </summary>
		public ComputedStyle ComputedStyle { get; }

		/// <summary>
		/// The collection of child boxes of this box, if any.
		/// </summary>
		public IReadOnlyList<Box> Children => _children ?? Array.Empty<Box>();
		protected Box[]? _children;

		/// <summary>
		/// The desired size of this box, resulting from a call to Measure().
		/// If the box's measure is invalid, this will be null.
		/// </summary>
		public Size2d? DesiredSize => _measuredSize.Width >= 0 ? _measuredSize : null;

		/// <summary>
		/// Margin thickness on this box, if any.  Required for many kinds of parent boxes
		/// to lay out their children.  Only valid after Measure() has been invoked.  This is
		/// not the *assigned* margin of the box, but the *computed* margin, and is in pixels.
		/// </summary>
		public virtual Thickness2d Margin => default;

		/// <summary>
		/// Border thickness on this box, if any.  Required for many kinds of parent boxes
		/// to lay out their children.  Only valid after Measure() has been invoked.  This is
		/// not the *assigned* border thickness of the box, but the *computed* border thickness,
		/// and is in pixels.
		/// </summary>
		public virtual Thickness2d Border => default;

		/// <summary>
		/// Padding on this box, if any.  Required for many kinds of parent boxes to lay out
		/// their children.  Only valid after Measure() has been invoked.  This is
		/// not the *assigned* padding of the box, but the *computed* padding, and is in pixels.
		/// </summary>
		public virtual Thickness2d Padding => default;

		private Size2d _lastMeasureAvailableSize;
		private Size2d _measuredSize;
		private bool _arrangeValid;

		private static readonly Size2d _invalidMeasure = new Size2d(-1, -1);

		protected Box(BoxKind kind, Element element, ComputedStyle computedStyle,
			IEnumerable<Box>? children)
		{
			Kind = kind;
			Element = element;
			ComputedStyle = computedStyle;
			_measuredSize = _invalidMeasure;

			if (children != null)
			{
				_children = children.ToArray();

				if (_children.Length == 0)
					_children = null;
			}
			else _children = null;
		}

		/// <summary>
		/// Measure the intrinsic size (or minimum size) of this box's content.
		/// If it has children, this should cause them to measure their sizes as well.
		/// </summary>
		/// <param name="availableSize">The available size for this box, which may be infinite.</param>
		/// <param name="context">Context that describes how to resolve em/ex units and percentages
		/// when converting measurements to pixels.</param>
		/// <returns>The desired size of this box.</returns>
		public Size2d Measure(Size2d availableSize, MeasureContext context)
		{
			if (_measuredSize.Width >= 0 && _measuredSize.Height >= 0
				&& _lastMeasureAvailableSize == availableSize)
				return _measuredSize;

			Size2d measuredSize = MeasureInternal(availableSize, context);
			_measuredSize = new Size2d(Math.Max(measuredSize.Width, 0), Math.Max(measuredSize.Height, 0));

			_lastMeasureAvailableSize = availableSize;
			return _measuredSize;
		}

		/// <summary>
		/// Arrange self and children to fit or fill the given container rectangle,
		/// if arranging is needed.
		/// </summary>
		/// <param name="containerRect">The container rectangle that this box has been assigned.</param>
		public void Arrange(Rect2d containerRect)
		{
			if (Rect == containerRect && _arrangeValid)
				return;

			ArrangeInternal(containerRect);

			Rect = containerRect;
			_arrangeValid = true;
		}

		/// <summary>
		/// Invalidate the measured size of this box so that the next call to Measure()
		/// will always re-measure it.
		/// </summary>
		public void InvalidateMeasure()
		{
			_measuredSize = _invalidMeasure;
		}

		/// <summary>
		/// Invalidate the measured size of this box and of all its descendants so that their
		/// next calls to Measure() will always re-measure them.
		/// </summary>
		public void InvalidateMeasureRecursively()
		{
			InvalidateMeasure();
			foreach (Box child in Children)
				child.InvalidateMeasureRecursively();
		}

		/// <summary>
		/// Invalidate the arranged layout of this box so that the next call to Arrange()
		/// will always re-perform layout on it.
		/// </summary>
		public void InvalidateArrange()
		{
			_arrangeValid = false;
		}

		/// <summary>
		/// Invalidate the arranged layout of this box and of all its descendants so that the
		/// next calls to Arrange() will always re-perform layout on them.
		/// </summary>
		public void InvalidateArrangeRecursively()
		{
			InvalidateArrange();
			foreach (Box child in Children)
				child.InvalidateArrangeRecursively();
		}

		/// <summary>
		/// Measure the intrinsic size (or minimum size) of this box's content.
		/// If it has children, this should cause them to measure their sizes as well.
		/// </summary>
		/// <param name="availableSize">The available size for this box, which may be infinite.</param>
		/// <param name="context">Context that describes how to resolve em/ex units and percentages
		/// when converting measurements to pixels.</param>
		/// <returns>The desired size of this box.</returns>
		protected virtual Size2d MeasureInternal(Size2d availableSize, MeasureContext context)
			=> default;

		/// <summary>
		/// Arrange self and children to fit or fill the given container rectangle.
		/// </summary>
		/// <param name="containerRect">The container rectangle that this box has been assigned.</param>
		protected virtual void ArrangeInternal(Rect2d containerRect)
		{
		}

		/// <summary>
		/// Given a Measure, this calculates the correct number of pixels that it is equivalent to.
		/// This works for all measures:  Anything that's unknown or isn't able to be made equivalent
		/// to a pixel will return 0.
		/// </summary>
		/// <param name="measure">The measure to convert to pixels.</param>
		/// <param name="percentSize">If this measure uses a percentage, which half of the context's
		/// PercentSize to use.</param>
		/// <param name="context">A context that can provide fonts and sizes for relative measures.</param>
		/// <returns>The resulting number of pixels.</returns>
		internal static double ResolveMeasure(Measure measure, PercentMode percentSize, MeasureContext context)
		{
			if (measure.Units == Units.Pixels)
			{
				return measure.Value;
			}
			else if (measure.Units == Units.Centimeters
				|| measure.Units == Units.Millimeters
				|| measure.Units == Units.Picas
				|| measure.Units == Units.Points
				|| measure.Units == Units.Inches)
			{
				return measure.ConvertTo(Units.Pixels).Value;
			}
			else if (measure.Units == Units.Percent)
			{
				if (percentSize == PercentMode.UseHeight)
					return double.IsInfinity(context.PercentSize.Height)
						? 0
						: measure.Value * (1.0 / 100) * context.PercentSize.Height;
				else
					return double.IsInfinity(context.PercentSize.Width)
						? 0
						: measure.Value * (1.0 / 100) * context.PercentSize.Width;
			}
			else if (measure.Units == Units.Em || measure.Units == Units.Ex)
			{
				IFont? font = context.GetCurrentFont();
				if (font == null)
					return 0;
				else if (measure.Units == Units.Em)
					return measure.Value * font.FontInfo.Size;  // sic. This ought to use the real 'M' measurement, but CSS requires us to use the font size.
				else if (measure.Units == Units.Ex)
					return measure.Value * font.FontMetrics.Ex.Height;
				else
					return 0;
			}
			else return 0;
		}
	}
}
