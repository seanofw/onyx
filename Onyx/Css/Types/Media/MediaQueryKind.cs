namespace Onyx.Css.Types.Media
{
	public enum MediaQueryKind : byte
	{
		Unknown = 0,

		MediaType,
		Feature,
		Measure,
		Number,
		Enum,

		Null,
		False,
		True,

		And,
		Or,
		Not,

		Lt,
		Gt,
		Le,
		Ge,
		Eq,
	}
}
