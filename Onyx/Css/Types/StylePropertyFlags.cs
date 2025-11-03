namespace Onyx.Css.Types
{
    [Flags]
    public enum StylePropertyFlags : ushort
    {
        None = 0,
		Valid = 1 << 0,
		Inherit = 1 << 1,
		Initial = 1 << 2,
		Unset = 1 << 3,
		Important = 1 << 15,
    }
}
