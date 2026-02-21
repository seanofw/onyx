namespace Onyx.Css.Types.Media
{
	public enum MediaPointerKind : byte
	{
		Unknown = 0,

		None,   // Explicitly not the same as Unknown; 'none' has meaning in CSS
		Coarse,
		Fine,
	}
}
