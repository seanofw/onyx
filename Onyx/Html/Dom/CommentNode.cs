
using System.Text;

namespace Onyx.Html.Dom
{
	public class CommentNode : Node
	{
		public CommentNode(string text)
		{
			Value = text;
		}

		public override NodeType NodeType => NodeType.Comment;

		public override IReadOnlyList<Node> ChildNodes => Array.Empty<Node>();

		public override string NodeName => "#comment";

		public override string? Value
		{
			get => string.Empty;
			set => throw new NotSupportedException();
		}

		public override string TextContent
		{
			get => string.Empty;
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
			stringBuilder.Append("<!--");
			stringBuilder.Append(Value);
			stringBuilder.Append("-->");
		}
	}
}
