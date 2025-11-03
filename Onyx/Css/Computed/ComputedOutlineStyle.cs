using System;
using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedOutlineStyle
	{
		private readonly Units _offsetUnits;
		private readonly Units _widthUnits;
		public readonly BorderStyle Style;
		private readonly byte _invert;

		public readonly Color32 Color;
		private readonly float _offsetValue;
		private readonly float _widthValue;

		public Measure Offset => new Measure(_offsetUnits, _offsetValue);
		public Measure Width => new Measure(_widthUnits, _widthValue);
		public bool Invert => _invert != 0;

		public static ComputedOutlineStyle Default { get; }
			= new ComputedOutlineStyle(Color32.Transparent, false, Measure.Zero, Measure.Zero, BorderStyle.None);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedOutlineStyle(Color32 color, bool invert, Measure offset, Measure width, BorderStyle style)
		{
			Color = color;
			_invert = (byte)(invert ? 1 : 0);
			_offsetUnits = offset.Units;
			_offsetValue = (float)offset.Value;
			_widthUnits = width.Units;
			_widthValue = (float)width.Value;
			Style = style;
		}

		public ComputedOutlineStyle WithColor(Color32 color, bool invert)
			=> new ComputedOutlineStyle(color, invert, Offset, Width, Style);
		public ComputedOutlineStyle WithColor(Color32 color)
			=> new ComputedOutlineStyle(color, Invert, Offset, Width, Style);
		public ComputedOutlineStyle WithInvert(bool invert)
			=> new ComputedOutlineStyle(Color, invert, Offset, Width, Style);
		public ComputedOutlineStyle WithOffset(Measure offset)
			=> new ComputedOutlineStyle(Color, Invert, offset, Width, Style);
		public ComputedOutlineStyle WithWidth(Measure width)
			=> new ComputedOutlineStyle(Color, Invert, Offset, width, Style);
		public ComputedOutlineStyle WithStyle(BorderStyle style)
			=> new ComputedOutlineStyle(Color, Invert, Offset, Width, style);
	}
}
