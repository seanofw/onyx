namespace Onyx.Css.Types.Media
{
	public enum MediaUpdateMode : byte
	{
		Unknown = 0,

		None,   // Explicitly not the same as Unknown; 'none' has meaning in CSS
		Slow,	// Updates are possible, but animation is generally prohibited
		Fast,	// CSS updates quickly like in a browser on a desktop PC
	}
}
