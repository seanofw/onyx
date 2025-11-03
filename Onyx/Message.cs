using Onyx.Css.Parsing;
using Onyx.Html.Parsing;

namespace Onyx
{
	public class Message
	{
		public MessageKind Kind { get; }
		public string Text { get; }
		public SourceLocation? Location { get; }

		public Message(MessageKind kind, string text, SourceLocation? location = null)
		{
			Kind = kind;
			Text = text;
			Location = location;
		}

		public override string ToString()
			=> $"{Location}: {Kind}: {Text}";
	}
}
