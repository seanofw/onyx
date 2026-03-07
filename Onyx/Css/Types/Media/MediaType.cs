namespace Onyx.Css.Types.Media
{
	public enum MediaType : byte
	{
		Unknown = 0,

		All,
		Screen,
		Print,

		// Deprecated media types.
		Tty,
		Tv,
		Projection,
		Handheld,
		Braille,
		Embossed,
		Aural,
		Speech,
	}
}
