
using System.Globalization;
using Onyx.Extensions;

namespace Onyx.Css.Parsing
{
	/// <summary>
	/// A single CSS token read by the lexer:  This represents either an identifier,
	/// or a number, or punctuation, or a string, or similar.  This is immutable.
	/// </summary>
	public class CssToken
	{
		public readonly CssTokenKind Kind;
		public readonly string? Text;
		public readonly double Number;

		public readonly SourceLocation SourceLocation;

		public string StringNumber => Number.ToString(CultureInfo.InvariantCulture);

		public CssToken(CssTokenKind kind, string? text, double number, SourceLocation sourceLocation)
		{
			Kind = kind;
			Text = text;
			Number = number;
			SourceLocation = sourceLocation;
		}

		/// <summary>
		/// Convert this token back into CSS text form.
		/// </summary>
		/// <returns>The same text, as CSS text.</returns>
		public override string ToString()
			=> Kind switch
			{
				CssTokenKind.Error => "<error>",
				CssTokenKind.Eoi => "<eoi>",
				CssTokenKind.None => "<none>",
				CssTokenKind.Space => " ",
				CssTokenKind.At => "@",
				CssTokenKind.Ident => Text ?? string.Empty,
				CssTokenKind.Number => StringNumber,
				CssTokenKind.String => "\"" + (Text ?? string.Empty).AddCSlashes() + "\"",
				CssTokenKind.Percentage => StringNumber + "%",
				CssTokenKind.Cdo => "<!--",
				CssTokenKind.Cdc => "-->",
				CssTokenKind.TildeEq => "~=",
				CssTokenKind.BarEq => "|=",
				CssTokenKind.StarEq => "*=",
				CssTokenKind.CaretEq => "~=",
				CssTokenKind.DollarEq => "$=",
				CssTokenKind.Id => "#" + Text,
				CssTokenKind.Important => "!important",
				CssTokenKind.Url => "url(\"" + (Text ?? string.Empty).AddCSlashes() + "\")",
				CssTokenKind.Func => Text + "(",
				CssTokenKind.Semicolon => ";",
				CssTokenKind.Comma => ",",
				CssTokenKind.Dot => ".",
				CssTokenKind.Colon => ":",
				CssTokenKind.Minus => "-",
				CssTokenKind.Plus => "+",
				CssTokenKind.Star => "*",
				CssTokenKind.Slash => "/",
				CssTokenKind.Equal => "=",
				CssTokenKind.Tilde => "~",
				CssTokenKind.Backtick => "`",
				CssTokenKind.Bar => "|",
				CssTokenKind.Caret => "^",
				CssTokenKind.Exclamation => "!",
				CssTokenKind.QuestionMark => "?",
				CssTokenKind.Ampersand => "&",
				CssTokenKind.Dollar => "$",
				CssTokenKind.Percent => "%",
				CssTokenKind.LessThan => "<",
				CssTokenKind.GreaterThan => ">",
				CssTokenKind.LeftBrace => "{",
				CssTokenKind.RightBrace => "}",
				CssTokenKind.LeftBracket => "[",
				CssTokenKind.RightBracket => "]",
				CssTokenKind.LeftParen => "(",
				CssTokenKind.RightParen => ")",
				_ => "?",
			};
	}
}