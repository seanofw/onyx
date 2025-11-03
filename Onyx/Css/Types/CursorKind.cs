namespace Onyx.Css.Types
{
	public enum CursorKind : byte
	{
		Auto = 1,
		None,
		Default,
		Pointer,

		// CSS 2
		Crosshair,
		Move,
		EResize,
		NeResize,
		NwResize,
		NResize,
		SeResize,
		SwResize,
		SResize,
		WResize,
		Text,
		Wait,
		Help,
		Progress,

		// CSS 3
		Alias,
		AllScroll,
		Cell,
		ColResize,
		ContextMenu,
		Copy,
		Grab,
		Grabbing,
		NoDrop,
		NotAllowed,
		RowResize,
		VerticalText,

		// CSS 4
		ZoomIn,
		ZoomOut,
	}
}
