namespace Onyx.Windows
{
	[Flags]
	public enum ModifierKeys : ushort
	{
		None = 0,
		LeftButton = (1 << 0),
		RightButton = (1 << 1),
		Shift = (1 << 2),
		Control = (1 << 3),
		MiddleButton = (1 << 4),
		XButton1 = (1 << 5),
		XButton2 = (1 << 6),
	}
}
