using System.Text;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A single comment in a document.
	/// </summary>
	public class CommentNode : SimpleNode
	{
		public override NodeType NodeType => NodeType.Comment;

		public override string NodeName => "#comment";

		public string CommentText { get; set; }

		public CommentNode(string text)
			=> CommentText = text;

		public override Node CloneNode(bool deep = false)
			=> new CommentNode(CommentText);

		public override void ToString(StringBuilder stringBuilder)
		{
			stringBuilder.Append("<!--");
			stringBuilder.Append(CommentText);
			stringBuilder.Append("-->");
		}
	}
}
