namespace Onyx.Css.Types.Media
{
	public enum MediaOverflowMode : byte
	{
		Unknown = 0,

		None,	// Explicitly not the same as Unknown; 'none' has meaning in CSS
		Scroll,
		Paged,
	}
}
