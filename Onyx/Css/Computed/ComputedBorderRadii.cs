using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedBorderRadii
	{
		public readonly Measure TopLeft;
		public readonly Measure TopRight;
		public readonly Measure BottomLeft;
		public readonly Measure BottomRight;

		public static ComputedBorderRadii Default { get; }
			= new ComputedBorderRadii(Measure.Zero, Measure.Zero, Measure.Zero, Measure.Zero);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ComputedBorderRadii(Measure topLeft, Measure topRight, Measure bottomLeft, Measure bottomRight)
		{
			TopLeft = topLeft;
			TopRight = topRight;
			BottomLeft = bottomLeft;
			BottomRight = bottomRight;
		}

		public ComputedBorderRadii WithTopLeft(Measure topLeft)
			=> new ComputedBorderRadii(topLeft, TopRight, BottomLeft, BottomRight);
		public ComputedBorderRadii WithTopRight(Measure topRight)
			=> new ComputedBorderRadii(TopLeft, topRight, BottomLeft, BottomRight);
		public ComputedBorderRadii WithBottomLeft(Measure bottomLeft)
			=> new ComputedBorderRadii(TopLeft, TopRight, bottomLeft, BottomRight);
		public ComputedBorderRadii WithBottomRight(Measure bottomRight)
			=> new ComputedBorderRadii(TopLeft, TopRight, BottomLeft, bottomRight);
	}
}
