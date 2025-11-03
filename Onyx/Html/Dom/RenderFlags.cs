namespace Onyx.Html.Dom
{
	[Flags]
	public enum RenderFlags : byte
	{
		None = 0,
		Visible = (1 << 0),			// Is visible and participates in flow and paint
		NeedsRepaint = (1 << 1),	// Needs repaint, but possibly not reflow
		NeedsReflow = (1 << 2),		// Needs reflow (must be combined with NeedsRepaint)
		VertScroll = (1 << 3),		// Has a displayed vertical scrollbar
		HorzScroll = (1 << 4),		// Has a displayed horizontal scrollbar
	}

	public enum StyleFlags : byte
	{
		None = 0,

		Hover = (1 << 0),           // Affects the :hover pseudo-selector
		Active = (1 << 1),          // Affects the :active pseudo-selector
		Focus = (1 << 2),           // Affects the :focus pseudo-selector
		Disabled = (1 << 3),        // Affects the :disabled and :enabled pseudo-selectors
		Visited = (1 << 4),         // Affects the :visited and :link pseudo-selectors
		Checked = (1 << 5),         // Affects the :checked and :indeterminate pseudo-selectors
		Indeterminate = (1 << 6),   // Affects the :checked and :indeterminate pseudo-selectors
	}
}
