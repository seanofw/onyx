namespace Onyx.Html.Dom
{
	public enum NodeType : byte
	{
		None = 0,

		Element = 1,
		Attribute = 2,
		Text = 3,
		CDataSection = 4,
		ProcessingInstruction = 7,
		Comment = 8,
		Document = 9,
		DocumentType = 10,
		DocumentFragment = 11,
	}
}