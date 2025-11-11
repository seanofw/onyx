namespace Onyx.Html.Dom
{
	[Flags]
	public enum RenderFlags : byte
	{
		None = 0,
		Visible = (1 << 0),				// Is visible and participates in flow and paint
		NeedsRepaint = (1 << 1),		// Needs repaint, but possibly not reflow
		NeedsReflow = (1 << 2),			// Needs reflow (must be combined with NeedsRepaint)
		VertScroll = (1 << 3),			// Has a displayed vertical scrollbar
		HorzScroll = (1 << 4),			// Has a displayed horizontal scrollbar
		ReadOnlyContainer = (1 << 7),	// If this is a ContainerNode, whether its child collection is modifiable.
	}
}
