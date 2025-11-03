namespace Onyx.Css.Selectors
{
	[Flags]
	public enum SimpleSelectorQueryPlanKind : ushort
	{
		ScanAll = 0,

		ElementType,
		Id,
		Classname,
		Name,
		TypeAttribute,

		SourceMask = 0xFF,

		Self = 1 << 8,
		Children = 2 << 8,
		Descendants = 3 << 8,

		TraversalMask = 0xFF << 8,
	}
}
