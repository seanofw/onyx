namespace Onyx.Css.Types
{
	[Flags]
	public enum SideOrCorner : byte
	{
		Left = 1 << 0,
		Right = 1 << 1,
		Top = 1 << 2,
		Bottom = 1 << 3,

		TopLeft = Top | Left,
		BottomLeft = Bottom | Left,
		TopRight = Top | Right,
		BottomRight = Bottom | Right,
	}
}
