using System.Text;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A single contiguous chunk of text in a document.
	/// </summary>
	public class TextNode : SimpleNode
	{
		private string? _text;

		public TextNode(string text)
			=> _text = text;

		public override NodeType NodeType => NodeType.Text;

		public override string NodeName => "#text";

		public override string? Value
		{
			get => _text;
			set => _text = value;
		}

		public override string TextContent
		{
			get => _text ?? string.Empty;
			set => _text = value;
		}

		public override Node CloneNode(bool deep = false)
			=> new TextNode(Value ?? string.Empty);

		public override void ToString(StringBuilder stringBuilder)
			=> stringBuilder.Append(Value);
	}
}
