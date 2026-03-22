using System.Runtime.CompilerServices;
using Onyx.Css.Properties;
using Onyx.Css.Types;
using Onyx.Types;

namespace Onyx.Css.Computed
{
	public readonly struct ComputedBorderEdgeStyle
	{
		public readonly Color32 Color;
		private readonly float _value;
		private readonly Units _units;
		public readonly BorderStyle Style;

		public Measure Width => new Measure(_units, _value);

		public static ComputedBorderEdgeStyle Default { get; }
			= new ComputedBorderEdgeStyle(BorderStyle.Solid, Color32.Transparent, new Measure(Units.Pixels, 3));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedBorderEdgeStyle(BorderStyle style, Color32 color, Measure width)
		{
			Style = style;
			Color = color;

			_units = width.Units;
			_value = (float)width.Value;
		}

		public ComputedBorderEdgeStyle WithStyle(BorderStyle style)
			=> new ComputedBorderEdgeStyle(style, Color, Width);
		public ComputedBorderEdgeStyle WithColor(Color32 color)
			=> new ComputedBorderEdgeStyle(Style, color, Width);
		public ComputedBorderEdgeStyle WithWidth(Measure width)
			=> new ComputedBorderEdgeStyle(Style, Color, width);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256 Diff(ComputedBorderEdgeStyle other,
			KnownPropertyKind style, KnownPropertyKind color, KnownPropertyKind width)
		{
			UInt256 bits = default;
			if (Style != other.Style)
				bits = bits.SetBit((int)style);
			if (Color != other.Color)
				bits = bits.SetBit((int)color);
			if (Width != other.Width)
				bits = bits.SetBit((int)width);
			return bits;
		}
	}
}
