using System.Runtime.CompilerServices;
using Onyx.Css.Types;

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
	}
}
