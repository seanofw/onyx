
namespace Onyx.Css.Parsing
{
	public enum CssTokenKind : sbyte
	{
		Error = -2,
		Eoi = -1,

		None = 0,

		Space,

		At,
		Ident,
		Integer,
		Number,

		String,

		Percentage,

		Cdo,		// "<!--"
		Cdc,		// "-->"

		TildeEq,
		BarEq,
		StarEq,
		CaretEq,
		DollarEq,

		Id,

		Important,

		Url,

		Func,

		Semicolon,
		Comma,
		Dot,
		Colon,

		Minus,
		Plus,
		Star,
		Slash,
		Equal,

		Tilde,
		Backtick,
		Bar,
		Caret,
		Exclamation,
		QuestionMark,
		Ampersand,
		Dollar,
		Percent,

		LessThan,
		GreaterThan,

		LeftBrace,
		RightBrace,
		LeftBracket,
		RightBracket,
		LeftParen,
		RightParen,
	}
}