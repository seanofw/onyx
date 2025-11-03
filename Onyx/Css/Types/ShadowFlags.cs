namespace Onyx.Css.Types
{
	[Flags]
	public enum ShadowFlags
	{
		None = 0,
		HasColor = 1 << 0,
		Inset = 1 << 1,
	}
}
