namespace Onyx.Html.Dom
{
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
