namespace Onyx.Html.Parsing
{
	/// <summary>
	/// This interface knows how to create nodes and construct a tree from them.
	/// It allows you to use the HtmlParser with nearly any possible type, not just
	/// Onyx's Element class.
	/// </summary>
	/// <typeparam name="TNode">The type of the "node" class.</typeparam>
	public interface INodeCreator<TNode>
	{
		/// <summary>
		/// Create a new element using the Text (name), Attributes, and SourceLocation
		/// from the given start tag.
		/// </summary>
		/// <param name="startTag">The start tag to create an element from.</param>
		/// <returns>The new element.</returns>
		TNode CreateElement(HtmlToken startTag);

		/// <summary>
		/// Create a new text node.
		/// </summary>
		/// <param name="text">The text for the node.</param>
		/// <param name="sourceLocation">The location where the text is found in the document.</param>
		/// <returns>The new node.</returns>
		TNode CreateText(string text, SourceLocation sourceLocation);

		/// <summary>
		/// Create a new comment node.
		/// </summary>
		/// <param name="text">The text for the comment.</param>
		/// <param name="sourceLocation">The location where the comment is found in the document.</param>
		/// <returns>The new node.</returns>
		TNode CreateComment(string text, SourceLocation sourceLocation);

		/// <summary>
		/// Add a new child to an node.
		/// </summary>
		/// <param name="node">The parent node.</param>
		/// <param name="newChild">The new child to append to the end of the parent's children.</param>
		void AddChild(TNode node, TNode newChild);
	}
}
