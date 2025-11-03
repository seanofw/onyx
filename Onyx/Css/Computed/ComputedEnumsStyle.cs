using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	/// <summary>
	/// Miscellaneous non-inherited enums.  These pack tightly, so they're all kept inside
	/// a single ulong, which means they fit in a single 64-bit register.
	/// </summary>
	public readonly struct ComputedEnumsStyle
	{
		private const int DisplayOffset = 0 * 4;
		private const int PositionOffset = 2 * 4;
		private const int ClearOffset = 3 * 4;
		private const int FloatOffset = 4 * 4;
		private const int ResizeOffset = 5 * 4;
		private const int BoxSizingOffset = 6 * 4;
		private const int OverflowXOffset = 7 * 4;
		private const int OverflowYOffset = 8 * 4;
		private const int TableLayoutOffset = 9 * 4;
		private const int VerticalAlignOffset = 10 * 4;
		private const int UnicodeBidiOffset = 11 * 4;
		private const int TextDecorationOffset = 12 * 4;

		public DisplayKind Display => (DisplayKind)((_value >> DisplayOffset) & 0xFF);
		public PositionKind Position => (PositionKind)((_value >> PositionOffset) & 0xF);
		public ClearMode Clear => (ClearMode)((_value >> ClearOffset) & 0xF);
		public FloatMode Float => (FloatMode)((_value >> FloatOffset) & 0xF);
		public ResizeKind Resize => (ResizeKind)((_value >> ResizeOffset) & 0xF);
		public BoxSizingMode BoxSizing => (BoxSizingMode)((_value >> BoxSizingOffset) & 0xF);
		public OverflowKind OverflowX => (OverflowKind)((_value >> OverflowXOffset) & 0xF);
		public OverflowKind OverflowY => (OverflowKind)((_value >> OverflowYOffset) & 0xF);
		public TableLayout TableLayout => (TableLayout)((_value >> TableLayoutOffset) & 0xF);
		public VerticalAlign VerticalAlign => (VerticalAlign)((_value >> VerticalAlignOffset) & 0xF);
		public UnicodeBidi UnicodeBidi => (UnicodeBidi)((_value >> UnicodeBidiOffset) & 0xF);
		public TextDecorationLineKind TextDecoration => (TextDecorationLineKind)((_value >> TextDecorationOffset) & 0xF);

		private readonly ulong _value;

		public static ComputedEnumsStyle Default { get; } = new ComputedEnumsStyle(
			DisplayKind.Block, PositionKind.Static,
			ClearMode.None, FloatMode.None, BoxSizingMode.BorderBox, ResizeKind.None,
			OverflowKind.Auto, OverflowKind.Auto,
			VerticalAlign.Baseline, UnicodeBidi.Normal,
			TableLayout.Auto, TextDecorationLineKind.None);

		public ComputedEnumsStyle(DisplayKind displayKind, PositionKind positionKind,
			ClearMode clear, FloatMode @float, BoxSizingMode boxSizing,
			ResizeKind resizeKind, OverflowKind overflowX, OverflowKind overflowY,
			VerticalAlign verticalAlign, UnicodeBidi unicodeBidi,
			TableLayout tableLayout, TextDecorationLineKind textDecoration)
		{
			_value =
				  ((ulong)displayKind << DisplayOffset)
				| ((ulong)positionKind << PositionOffset)
				| ((ulong)clear << ClearOffset)
				| ((ulong)@float << FloatOffset)
				| ((ulong)boxSizing << BoxSizingOffset)
				| ((ulong)resizeKind << ResizeOffset)
				| ((ulong)overflowX << OverflowXOffset)
				| ((ulong)overflowY << OverflowYOffset)
				| ((ulong)verticalAlign << VerticalAlignOffset)
				| ((ulong)unicodeBidi << UnicodeBidiOffset)
				| ((ulong)textDecoration << TextDecorationOffset)
				| ((ulong)tableLayout << TableLayoutOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedEnumsStyle(ulong value)
			=> _value = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithDisplay(DisplayKind display)
			=> new ComputedEnumsStyle(((ulong)display << DisplayOffset) | (_value & ~(0xFFUL << DisplayOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithPosition(PositionKind position)
			=> new ComputedEnumsStyle(((ulong)position << PositionOffset) | (_value & ~(0xFUL << PositionOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithClear(ClearMode clear)
			=> new ComputedEnumsStyle(((ulong)clear << ClearOffset) | (_value & ~(0xFUL << ClearOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithFloat(FloatMode @float)
			=> new ComputedEnumsStyle(((ulong)@float << FloatOffset) | (_value & ~(0xFUL << FloatOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithBoxSizing(BoxSizingMode boxSizing)
			=> new ComputedEnumsStyle(((ulong)boxSizing << BoxSizingOffset) | (_value & ~(0xFUL << BoxSizingOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithResize(ResizeKind resizeKind)
			=> new ComputedEnumsStyle(((ulong)resizeKind << ResizeOffset) | (_value & ~(0xFUL << ResizeOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithOverflowX(OverflowKind overflowX)
			=> new ComputedEnumsStyle(((ulong)overflowX << OverflowXOffset) | (_value & ~(0xFUL << OverflowXOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithOverflowY(OverflowKind overflowY)
			=> new ComputedEnumsStyle(((ulong)overflowY << OverflowYOffset) | (_value & ~(0xFUL << OverflowYOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithVerticalAlign(VerticalAlign verticalAlign)
			=> new ComputedEnumsStyle(((ulong)verticalAlign << VerticalAlignOffset) | (_value & ~(0xFUL << VerticalAlignOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithUnicodeBidi(UnicodeBidi unicodeBidi)
			=> new ComputedEnumsStyle(((ulong)unicodeBidi << UnicodeBidiOffset) | (_value & ~(0xFUL << UnicodeBidiOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithTableLayout(TableLayout tableLayout)
			=> new ComputedEnumsStyle(((ulong)tableLayout << TableLayoutOffset) | (_value & ~(0xFUL << TableLayoutOffset)));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedEnumsStyle WithTextDecoration(TextDecorationLineKind textDecoration)
			=> new ComputedEnumsStyle(((ulong)textDecoration << TextDecorationOffset) | (_value & ~(0xFUL << TextDecorationOffset)));
	}
}