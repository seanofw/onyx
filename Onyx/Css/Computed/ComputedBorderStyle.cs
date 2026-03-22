using System.Runtime.CompilerServices;
using Onyx.Css.Properties;
using Onyx.Css.Types;
using Onyx.Types;

namespace Onyx.Css.Computed
{
	public class ComputedBorderStyle
	{
		public readonly ComputedBorderEdgeStyle Top;
		public readonly ComputedBorderEdgeStyle Right;
		public readonly ComputedBorderEdgeStyle Bottom;
		public readonly ComputedBorderEdgeStyle Left;

		private readonly ComputedBorderRadii _corner;

		public Measure TopLeftRadius => _corner.TopLeft;
		public Measure TopRightRadius => _corner.TopRight;
		public Measure BottomLeftRadius => _corner.BottomLeft;
		public Measure BottomRightRadius => _corner.BottomRight;

		public static ComputedBorderStyle Default { get; }
			= new ComputedBorderStyle(ComputedBorderEdgeStyle.Default,
				ComputedBorderEdgeStyle.Default,
				ComputedBorderEdgeStyle.Default,
				ComputedBorderEdgeStyle.Default,
				ComputedBorderRadii.Default);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedBorderStyle(ComputedBorderEdgeStyle top, ComputedBorderEdgeStyle right,
			ComputedBorderEdgeStyle bottom, ComputedBorderEdgeStyle left,
			ComputedBorderRadii corner)
		{
			Top = top;
			Right = right;
			Bottom = bottom;
			Left = left;
			_corner = corner;
		}

		public ComputedBorderStyle WithTop(ComputedBorderEdgeStyle top)
			=> new ComputedBorderStyle(top, Right, Bottom, Left, _corner);
		public ComputedBorderStyle WithRight(ComputedBorderEdgeStyle right)
			=> new ComputedBorderStyle(Top, right, Bottom, Left, _corner);
		public ComputedBorderStyle WithBottom(ComputedBorderEdgeStyle bottom)
			=> new ComputedBorderStyle(Top, Right, bottom, Left, _corner);
		public ComputedBorderStyle WithLeft(ComputedBorderEdgeStyle left)
			=> new ComputedBorderStyle(Top, Right, Bottom, left, _corner);

		public ComputedBorderStyle WithColors(Color32 top, Color32 right, Color32 bottom, Color32 left)
			=> new ComputedBorderStyle(Top.WithColor(top), Right.WithColor(right),
				Bottom.WithColor(bottom), Left.WithColor(left), _corner);
		public ComputedBorderStyle WithTopColor(Color32 color)
			=> WithTop(Top.WithColor(color));
		public ComputedBorderStyle WithRightColor(Color32 color)
			=> WithRight(Right.WithColor(color));
		public ComputedBorderStyle WithBottomColor(Color32 color)
			=> WithBottom(Bottom.WithColor(color));
		public ComputedBorderStyle WithLeftColor(Color32 color)
			=> WithLeft(Left.WithColor(color));

		public ComputedBorderStyle WithStyles(BorderStyle top, BorderStyle right, BorderStyle bottom, BorderStyle left)
			=> new ComputedBorderStyle(Top.WithStyle(top), Right.WithStyle(right),
				Bottom.WithStyle(bottom), Left.WithStyle(left), _corner);
		public ComputedBorderStyle WithTopStyle(BorderStyle style)
			=> WithTop(Top.WithStyle(style));
		public ComputedBorderStyle WithRightStyle(BorderStyle style)
			=> WithRight(Right.WithStyle(style));
		public ComputedBorderStyle WithBottomStyle(BorderStyle style)
			=> WithBottom(Bottom.WithStyle(style));
		public ComputedBorderStyle WithLeftStyle(BorderStyle style)
			=> WithLeft(Left.WithStyle(style));

		public ComputedBorderStyle WithWidths(Measure top, Measure right, Measure bottom, Measure left)
			=> new ComputedBorderStyle(Top.WithWidth(top), Right.WithWidth(right),
				Bottom.WithWidth(bottom), Left.WithWidth(left), _corner);
		public ComputedBorderStyle WithTopWidth(Measure width)
			=> WithTop(Top.WithWidth(width));
		public ComputedBorderStyle WithRightWidth(Measure width)
			=> WithRight(Right.WithWidth(width));
		public ComputedBorderStyle WithBottomWidth(Measure width)
			=> WithBottom(Bottom.WithWidth(width));
		public ComputedBorderStyle WithLeftWidth(Measure width)
			=> WithLeft(Left.WithWidth(width));

		public ComputedBorderStyle WithRadii(Measure topLeft, Measure topRight, Measure bottomLeft, Measure bottomRight)
			=> new ComputedBorderStyle(Top, Right, Bottom, Left,
				new ComputedBorderRadii(topLeft, topRight, bottomLeft, bottomRight));
		public ComputedBorderStyle WithTopLeftRadius(Measure topLeft)
			=> new ComputedBorderStyle(Top, Right, Bottom, Left,
				_corner.WithTopLeft(topLeft));
		public ComputedBorderStyle WithTopRightRadius(Measure topRight)
			=> new ComputedBorderStyle(Top, Right, Bottom, Left,
				_corner.WithTopRight(topRight));
		public ComputedBorderStyle WithBottomLeftRadius(Measure bottomLeft)
			=> new ComputedBorderStyle(Top, Right, Bottom, Left,
				_corner.WithBottomLeft(bottomLeft));
		public ComputedBorderStyle WithBottomRightRadius(Measure bottomRight)
			=> new ComputedBorderStyle(Top, Right, Bottom, Left,
				_corner.WithBottomRight(bottomRight));

		public UInt256 Diff(ComputedBorderStyle other)
		{
			if (this == other)
				return default;

			UInt256 bits = default;
			bits |= Top.Diff(other.Top, KnownPropertyKind.BorderTopStyle, KnownPropertyKind.BorderTopColor, KnownPropertyKind.BorderTopWidth);
			bits |= Right.Diff(other.Right, KnownPropertyKind.BorderRightStyle, KnownPropertyKind.BorderRightColor, KnownPropertyKind.BorderRightWidth);
			bits |= Bottom.Diff(other.Bottom, KnownPropertyKind.BorderBottomStyle, KnownPropertyKind.BorderBottomColor, KnownPropertyKind.BorderBottomWidth);
			bits |= Left.Diff(other.Left, KnownPropertyKind.BorderLeftStyle, KnownPropertyKind.BorderLeftColor, KnownPropertyKind.BorderLeftWidth);
			bits |= _corner.Diff(other._corner);
			return bits;
		}
	}
}
