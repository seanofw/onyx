namespace Onyx.Html.Dom
{
	/// <summary>
	/// A LeafNode represents a simple node in the tree with no children or attributes.
	/// It is used as the base class for TextNode and CommentNode.
	/// </summary>
	public abstract class LeafNode : Node
	{
		public override IReadOnlyList<Node> ChildNodes => Array.Empty<Node>();

		public override void AppendChild(Node child)
			=> throw new NotSupportedException();

		public override void InsertBefore(Node newNode, Node referenceNode)
			=> throw new NotSupportedException();

		public override void RemoveChild(Node child)
			=> throw new NotSupportedException();

		public override void ReplaceChild(Node newNode, Node referenceNode)
			=> throw new NotSupportedException();

		public override bool HasChildNodes()
			=> false;
	}
}
