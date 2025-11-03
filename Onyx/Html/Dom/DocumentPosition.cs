namespace Onyx.Html.Dom
{
	[Flags]
	public enum DocumentPosition
	{
		Disconnected = (1 << 0),
		Preceding = (1 << 1),
		Following = (1 << 2),
		Contains = (1 << 3),
		ContainedBy = (1 << 4),
	}
}
