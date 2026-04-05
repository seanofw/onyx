using Onyx.Css.Computed;
using Onyx.Html.Dom;

namespace Onyx.Boxes
{
	/// <summary>
	/// A TextSpan box renders one or more characters in a horizontal row.
	/// </summary>
	public sealed class TextSpanBox : LeafBox
	{
		/// <summary>
		/// The single line of text to render within this box.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// Construct a new TextSpanBox.
		/// </summary>
		/// <param name="element">The element that owns this text span.</param>
		/// <param name="text">The text to present.</param>
		public TextSpanBox(Element element, ComputedStyle computedStyle, string text)
			: base(BoxKind.Text, element, computedStyle)
		{
			Text = text;
		}

		/// <summary>
		/// Construct a new TextSpanBox.
		/// </summary>
		/// <param name="element">The element that owns this text span.</param>
		/// <param name="text">The text to present.</param>
		public TextSpanBox(Element element, ComputedStyle computedStyle, ReadOnlySpan<char> text)
			: base(BoxKind.Text, element, computedStyle)
		{
			Text = text.ToString();
		}
	}
}
