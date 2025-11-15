using System.Text;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A single comment in a document.
	/// </summary>
	public class CommentNode : SimpleNode
	{
		/// <summary>
		/// What type of node this is (a comment).
		/// </summary>
		public override NodeType NodeType => NodeType.Comment;

		/// <summary>
		/// The name of this node (always "#comment").
		/// </summary>
		public override string NodeName => "#comment";

		/// <summary>
		/// The actual text of the comment, included so that document round-tripping
		/// is possible.  This can be modified.
		/// </summary>
		public string CommentText { get; set; }

		/// <summary>
		/// Construct a new comment node with the given text inside it.
		/// </summary>
		/// <param name="text">The text of the comment.</param>
		public CommentNode(string text)
			=> CommentText = text;

		/// <summary>
		/// Make a perfect duplicate of this node (without events).
		/// </summary>
		/// <param name="deep">Whether to perform a shallow or a deep clone (irrelevant
		/// for comment nodes).</param>
		/// <returns>A clone of this node.</returns>
		public override Node CloneNode(bool deep = false)
			=> new CommentNode(CommentText);

		/// <summary>
		/// Convert this node back to HTML text, appending it to the given StringBuilder.
		/// </summary>
		/// <param name="stringBuilder">The StringBuilder to append the node's equivalent
		/// HTML text to.</param>
		public override void ToString(StringBuilder stringBuilder)
		{
			stringBuilder.Append("<!--");
			stringBuilder.Append(CommentText);
			stringBuilder.Append("-->");
		}
	}
}
