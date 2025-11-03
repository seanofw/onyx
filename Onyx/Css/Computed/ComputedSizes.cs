using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedSizes
	{
		private readonly SizeConstraints _sizeConstraints;
		private readonly EdgeSizes _offset;
		private readonly EdgeSizes _padding;
		private readonly EdgeSizes _margin;

		public SizeConstraints SizeConstraints => _sizeConstraints;

		public Measure Width => _sizeConstraints.Width;
		public Measure Height => _sizeConstraints.Height;
		public Measure MinWidth => _sizeConstraints.MinWidth;
		public Measure MinHeight => _sizeConstraints.MinHeight;
		public Measure MaxWidth => _sizeConstraints.MaxWidth;
		public Measure MaxHeight => _sizeConstraints.MaxHeight;

		public EdgeSizes Offset => _offset;

		public Measure Left => _offset.Left;
		public Measure Top => _offset.Top;
		public Measure Right => _offset.Right;
		public Measure Bottom => _offset.Bottom;

		public EdgeSizes Padding => _padding;

		public Measure PaddingLeft => _padding.Left;
		public Measure PaddingTop => _padding.Top;
		public Measure PaddingRight => _padding.Right;
		public Measure PaddingBottom => _padding.Bottom;

		public EdgeSizes Margin => _margin;

		public Measure MarginLeft => _margin.Left;
		public Measure MarginTop => _margin.Top;
		public Measure MarginRight => _margin.Right;
		public Measure MarginBottom => _margin.Bottom;

		public static ComputedSizes Default { get; } = new ComputedSizes(
			SizeConstraints.Default, EdgeSizes.Zero, EdgeSizes.Zero, EdgeSizes.Zero);

		public ComputedSizes(SizeConstraints sizeConstraints,
			EdgeSizes offset, EdgeSizes padding, EdgeSizes margin)
		{
			_sizeConstraints = sizeConstraints;
			_offset = offset;
			_padding = padding;
			_margin = margin;
		}

		public ComputedSizes(Measure width, Measure height,
			Measure minWidth, Measure minHeight, Measure maxWidth, Measure maxHeight,
			Measure top, Measure right, Measure bottom, Measure left,
			Measure paddingTop, Measure paddingRight, Measure paddingBottom, Measure paddingLeft,
			Measure marginTop, Measure marginRight, Measure marginBottom, Measure marginLeft)
		{
			_sizeConstraints = new SizeConstraints(width, height, minWidth, minHeight, maxWidth, maxHeight);
			_offset = new EdgeSizes(top, right, bottom, left);
			_padding = new EdgeSizes(paddingTop, paddingRight, paddingBottom, paddingLeft);
			_margin = new EdgeSizes(marginTop, marginRight, marginBottom, marginLeft);
		}

		public ComputedSizes WithSizeConstraints(SizeConstraints sizeConstraints)
			=> new ComputedSizes(sizeConstraints, _offset, _padding, _margin);

		public ComputedSizes WithWidth(Measure width)
			=> new ComputedSizes(_sizeConstraints.WithWidth(width), _offset, _padding, _margin);
		public ComputedSizes WithHeight(Measure height)
			=> new ComputedSizes(_sizeConstraints.WithHeight(height), _offset, _padding, _margin);
		public ComputedSizes WithMinWidth(Measure minWidth)
			=> new ComputedSizes(_sizeConstraints.WithMinWidth(minWidth), _offset, _padding, _margin);
		public ComputedSizes WithMinHeight(Measure minHeight)
			=> new ComputedSizes(_sizeConstraints.WithMinHeight(minHeight), _offset, _padding, _margin);
		public ComputedSizes WithMaxWidth(Measure maxWidth)
			=> new ComputedSizes(_sizeConstraints.WithMaxWidth(maxWidth), _offset, _padding, _margin);
		public ComputedSizes WithMaxHeight(Measure maxHeight)
			=> new ComputedSizes(_sizeConstraints.WithMaxHeight(maxHeight), _offset, _padding, _margin);

		public ComputedSizes WithOffset(EdgeSizes offset)
			=> new ComputedSizes(_sizeConstraints, offset, _padding, _margin);

		public ComputedSizes WithLeft(Measure left)
			=> new ComputedSizes(_sizeConstraints, _offset.WithLeft(left), _padding, _margin);
		public ComputedSizes WithTop(Measure top)
			=> new ComputedSizes(_sizeConstraints, _offset.WithTop(top), _padding, _margin);
		public ComputedSizes WithBottom(Measure bottom)
			=> new ComputedSizes(_sizeConstraints, _offset.WithBottom(bottom), _padding, _margin);
		public ComputedSizes WithRight(Measure right)
			=> new ComputedSizes(_sizeConstraints, _offset.WithRight(right), _padding, _margin);

		public ComputedSizes WithPadding(EdgeSizes padding)
			=> new ComputedSizes(_sizeConstraints, _offset, padding, _margin);

		public ComputedSizes WithPaddingLeft(Measure paddingLeft)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding.WithLeft(paddingLeft), _margin);
		public ComputedSizes WithPaddingTop(Measure paddingTop)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding.WithTop(paddingTop), _margin);
		public ComputedSizes WithPaddingBottom(Measure paddingBottom)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding.WithBottom(paddingBottom), _margin);
		public ComputedSizes WithPaddingRight(Measure paddingRight)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding.WithRight(paddingRight), _margin);

		public ComputedSizes WithMargin(EdgeSizes margin)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding, margin);

		public ComputedSizes WithMarginLeft(Measure marginLeft)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding, _margin.WithLeft(marginLeft));
		public ComputedSizes WithMarginTop(Measure marginTop)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding, _margin.WithTop(marginTop));
		public ComputedSizes WithMarginBottom(Measure marginBottom)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding, _margin.WithBottom(marginBottom));
		public ComputedSizes WithMarginRight(Measure marginRight)
			=> new ComputedSizes(_sizeConstraints, _offset, _padding, _margin.WithRight(marginRight));
	}
}
