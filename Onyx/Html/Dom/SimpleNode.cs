namespace Onyx.Html.Dom
{
	/// <summary>
	/// A SimpleNode represents a simple node in the tree with no children or attributes.
	/// It is used as the base class for TextNode and CommentNode.
	/// </summary>
	public abstract class SimpleNode : Node
	{
		/// <summary>
		/// The children of this node (always empty).
		/// </summary>
		public override IReadOnlyList<Node> ChildNodes => Array.Empty<Node>();

		/// <summary>
		/// Append a child to this node.  Always throws NotSupportedException.
		/// </summary>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		public override void AppendChild(Node child)
			=> throw new NotSupportedException();

		/// <summary>
		/// Insert a child under this node.  Always throws NotSupportedException.
		/// </summary>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		public override void InsertBefore(Node newNode, Node referenceNode)
			=> throw new NotSupportedException();

		/// <summary>
		/// Remove a child of this node.  Always throws NotSupportedException.
		/// </summary>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		public override void RemoveChild(Node child)
			=> throw new NotSupportedException();

		/// <summary>
		/// Replace a child of this node with another.  Always throws NotSupportedException.
		/// </summary>
		/// <exception cref="NotSupportedException">Always thrown.</exception>
		public override void ReplaceChild(Node newNode, Node referenceNode)
			=> throw new NotSupportedException();

		/// <summary>
		/// Whether this has children (always false).
		/// </summary>
		/// <returns>False, always.</returns>
		public override bool HasChildNodes()
			=> false;
	}
}
