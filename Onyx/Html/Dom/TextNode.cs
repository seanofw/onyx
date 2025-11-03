
using System.Text;

namespace Onyx.Html.Dom
{
	public class TextNode : Node
	{
		public TextNode(string text)
		{
			Value = text;
		}

		public override NodeType NodeType => NodeType.Text;

		public override IReadOnlyList<Node> ChildNodes => Array.Empty<Node>();

		public override string NodeName => "#text";

		public override string? Value { get; set; }

		public override string TextContent
		{
			get => Value ?? string.Empty;
			set => Value = value;
		}

		public override void AppendChild(Node child)
			=> throw new NotSupportedException();

		public override Node CloneNode(bool deep = false)
		{
			throw new NotImplementedException();
		}

		public override bool HasChildNodes()
			=> false;

		public override void InsertBefore(Node newNode, Node referenceNode)
			=> throw new NotSupportedException();

		public override void Normalize()
		{
		}
 
		public override void RemoveChild(Node child)
			=> throw new NotSupportedException();

		public override void ReplaceChild(Node newNode, Node referenceNode)
			=> throw new NotSupportedException();

		public override void ToString(StringBuilder stringBuilder)
		{
			stringBuilder.Append(Value);
		}
	}
}
