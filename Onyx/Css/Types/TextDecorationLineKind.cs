namespace Onyx.Css.Types
{
	[Flags]
	public enum TextDecorationLineKind : byte
	{
		None = 0,
		Underline = 1 << 0,
		Overline = 1 << 1,
		LineThrough = 1 << 2,
		Blink = 1 << 3,
	}
}
