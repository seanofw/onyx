using System.Text;
using Onyx.Extensions;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A single contiguous chunk of text in a document.
	/// </summary>
	public class TextNode : SimpleNode
	{
		/// <summary>
		/// What type of node this is (plain text).
		/// </summary>
		public override NodeType NodeType => NodeType.Text;

		/// <summary>
		/// The name of this node (always "#text").
		/// </summary>
		public override string NodeName => "#text";

		/// <summary>
		/// The value of this node (always the same as its text).  This can be
		/// modified to alter the text.
		/// </summary>
		public override string? Value
		{
			get => _text;
			set
			{
				if (_text != value)
				{
					_text = value;

				}
			}
		}
		private string? _text;

		/// <summary>
		/// The text of this node (the same as its value, but null will be converted
		/// to the empty string).  This can be modified to alter the text.
		/// </summary>
		public override string TextContent
		{
			get => _text ?? string.Empty;
			set => Value = value;
		}

		/// <summary>
		/// Construct a new text node.
		/// </summary>
		/// <param name="text">The new text of the node.</param>
		public TextNode(string? text = null)
			=> _text = text;

		/// <summary>
		/// Make a perfect duplicate of this node (without events).
		/// </summary>
		/// <param name="deep">Whether to perform a shallow or a deep clone (irrelevant
		/// for comment nodes).</param>
		/// <returns>A clone of this node.</returns>
		public override Node CloneNode(bool deep = false)
			=> new TextNode(Value ?? string.Empty);

		/// <summary>
		/// Convert this node back to HTML text, appending it to the given StringBuilder.
		/// </summary>
		/// <param name="stringBuilder">The StringBuilder to append the node's equivalent
		/// HTML text to.</param>
		public override void ToString(StringBuilder stringBuilder)
			=> Value?.HtmlEncodeTo(stringBuilder);
	}
}
