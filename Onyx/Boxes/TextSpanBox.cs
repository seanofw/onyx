namespace Onyx.Boxes
{
	/// <summary>
	/// A TextSpan box renders one or more characters in a horizontal row.
	/// </summary>
	public class TextSpanBox : Box
	{
		/// <summary>
		/// The single line of text to render within this box.
		/// </summary>
		public string Text { get; internal set; } = string.Empty;
	}
}
