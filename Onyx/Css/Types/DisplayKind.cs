namespace Onyx.Css.Types
{
	public enum DisplayKind : byte
	{
		None = 1,

		Inline,
		InlineBlock,
		InlineTable,
		InlineFlex,

		Block,
		Flex,
		ListItem,

		Table,
		TableRowGroup,
		TableHeaderGroup,
		TableFooterGroup,
		TableRow,
		TableColumnGroup,
		TableColumn,
		TableCell,
		TableCaption,
	}
}
