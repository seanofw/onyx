using Onyx.Css.Properties;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
public class ComputedTableStyle : IDefaultInheritedStyle
	{
		public BorderCollapse BorderCollapse { get; }
		public EmptyCellsMode EmptyCells { get; }
		public CaptionSide CaptionSide { get; }
		public Measure BorderSpacingX { get; }
		public Measure BorderSpacingY { get; }

		public static ComputedTableStyle Default { get; } = new ComputedTableStyle(BorderCollapse.Separate,
			new Measure(Units.Pixels, 2), new Measure(Units.Pixels, 2), EmptyCellsMode.Show, CaptionSide.Bottom);

		public ComputedTableStyle(BorderCollapse borderCollapse, Measure borderSpacingX, Measure borderSpacingY,
			EmptyCellsMode emptyCells, CaptionSide captionSide)
		{
			BorderCollapse = borderCollapse;
			BorderSpacingX = borderSpacingX;
			BorderSpacingY = borderSpacingY;
			EmptyCells = emptyCells;
			CaptionSide = captionSide;
		}

		public ComputedTableStyle WithBorderCollapse(BorderCollapse borderCollapse)
			=> new ComputedTableStyle(borderCollapse, BorderSpacingX, BorderSpacingY, EmptyCells, CaptionSide);
		public ComputedTableStyle WithBorderSpacing(Measure borderSpacing)
			=> new ComputedTableStyle(BorderCollapse, borderSpacing, borderSpacing, EmptyCells, CaptionSide);
		public ComputedTableStyle WithBorderSpacing(Measure borderSpacingX, Measure borderSpacingY)
			=> new ComputedTableStyle(BorderCollapse, borderSpacingX, borderSpacingY, EmptyCells, CaptionSide);
		public ComputedTableStyle WithBorderSpacingY(Measure borderSpacingY)
			=> new ComputedTableStyle(BorderCollapse, BorderSpacingX, borderSpacingY, EmptyCells, CaptionSide);
		public ComputedTableStyle WithBorderSpacingX(Measure borderSpacingX)
			=> new ComputedTableStyle(BorderCollapse, borderSpacingX, BorderSpacingY, EmptyCells, CaptionSide);
		public ComputedTableStyle WithEmptyCells(EmptyCellsMode emptyCells)
			=> new ComputedTableStyle(BorderCollapse, BorderSpacingX, BorderSpacingY, emptyCells, CaptionSide);
		public ComputedTableStyle WithCaptionSide(CaptionSide captionSide)
			=> new ComputedTableStyle(BorderCollapse, BorderSpacingX, BorderSpacingY, EmptyCells, captionSide);

		public UInt256 Diff(ComputedTableStyle other)
		{
			if (this == other)
				return default;

			UInt256 bits = default;
			if (BorderCollapse != other.BorderCollapse)
				bits = bits.SetBit((int)KnownPropertyKind.BorderCollapse);
			if (BorderSpacingX != other.BorderSpacingX || BorderSpacingY != BorderSpacingY)
				bits = bits.SetBit((int)KnownPropertyKind.BorderSpacing);
			if (EmptyCells != other.EmptyCells)
				bits = bits.SetBit((int)KnownPropertyKind.EmptyCells);
			if (CaptionSide != other.CaptionSide)
				bits = bits.SetBit((int)KnownPropertyKind.CaptionSide);
			return bits;
		}
	}
}
