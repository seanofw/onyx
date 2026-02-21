namespace Onyx.Css.Types.Media
{
	public enum MediaHoverKind : byte
	{
		Unknown = 0,

		None,   // Explicitly not the same as Unknown; 'none' has meaning in CSS
		Hover,
	}
}
