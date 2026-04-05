namespace Onyx.Boxes
{
	public enum BoxKind
	{
		None = 0,

		//-------------------------------------------------------------------
		// Decorations

		/// <summary>
		/// Purely decorative visual boxes are in category 0x10.
		/// </summary>
		Visual = 0x10,

		/// <summary>
		/// A border box is a decorator that draws a border around its content.
		/// </summary>
		Border,

		/// <summary>
		/// A background box is a decorator that draws a background behind its content.
		/// </summary>
		Background,

		//-------------------------------------------------------------------
		// Basic layout: block and inline.

		Layout = 0x20,

		/// <summary>
		/// 'display:block' formatting: top-to-bottom rows, with margin-collapse rules applying.
		/// </summary>
		Block,

		/// <summary>
		/// 'float' formatting: This box is pulled out of the usual inline layout, and it creates
		/// extra "margins" on the sides of nearby inline content to fit it.
		/// </summary>
		Float,

		/// <summary>
		/// An inline box lays out text like lines in paragraphs, creating Line boxes inside it.
		/// </summary>
		Inline,

		/// <summary>
		/// A line box contains a single horizontal span of child boxes, and aligns them relative
		/// to their baselines.
		/// </summary>
		Line,

		/// <summary>
		/// A text box displays a sequence of horizontal text.  It does not wrap.
		/// </summary>
		Text,

		//-------------------------------------------------------------------
		// Special replaced elements.

		/// <summary>
		/// Special replaced elements.
		/// </summary>
		Special = 0x30,

		/// <summary>
		/// A line-break box is a special box created by either a &lt;br/&gt; or an appropriately-
		/// placed newline. It has zero width and height, but it causes the current Line box to be
		/// ended and a new one to be started, always.
		/// </summary>
		LineBreak,

		/// <summary>
		/// An image is a replaced element that displays a static picture.
		/// </summary>
		Image,

		//-------------------------------------------------------------------
		// Form inputs

		/// <summary>
		/// Form-input elements.
		/// </summary>
		Form = 0x40,

		/// <summary>
		/// A text-input field, which supports not just selection but single-line editing.
		/// </summary>
		InputText,

		/// <summary>
		/// A checkbox-input field, which displays a small check rectangle beside it that can be toggled.
		/// </summary>
		InputCheckbox,

		/// <summary>
		/// A radio-button-input field, which displays a small radio area beside it that can be selected.
		/// </summary>
		InputRadio,

		/// <summary>
		/// A dropdown input control.
		/// </summary>
		Select,
	}
}
