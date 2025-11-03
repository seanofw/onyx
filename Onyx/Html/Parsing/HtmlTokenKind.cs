namespace Onyx.Html.Parsing
{
	/// <summary>
	/// What kind of token was just read by the lexer.
	/// </summary>
	public enum HtmlTokenKind : sbyte
	{
		/// <summary>
		/// End of input.
		/// </summary>
		Eoi = -1,

		/// <summary>
		/// No token (reserved, never returned).
		/// </summary>
		None = 0,

		/// <summary>
		/// A start tag (or self-closing tag; in HTML, they're equivalent).
		/// </summary>
		StartTag = 1,

		/// <summary>
		/// An end tag.
		/// </summary>
		EndTag = 2,

		/// <summary>
		/// Plain text.
		/// </summary>
		Text = 3,

		/// <summary>
		/// A comment.
		/// </summary>
		Comment = 4,
	}
}