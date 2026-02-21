namespace Onyx.Css.Types.Media
{
	public enum MediaColorMode : byte
	{
		Unknown = 0,

		Truecolor,		// Uses true color with N bits per color channel.
		Paletted,		// Uses a fixed palette of N entries.
		Monochrome,		// Only renders monochrome with N bits to represent brightness.
	}
}
