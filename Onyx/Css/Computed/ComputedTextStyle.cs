using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedTextStyle : IDefaultInheritedStyle
	{
		public readonly TextAlign TextAlign;
		public readonly WritingDirection Direction;
		public readonly TextTransform TextTransform;
		public readonly WhiteSpaceKind WhiteSpace;
		private readonly Units _wordSpacingUnits;
		private readonly Units _textIndentUnits;
		private readonly Units _letterSpacingUnits;
		private readonly Units _lineHeightUnits;

		private readonly double _wordSpacingValue;
		private readonly double _textIndentValue;
		private readonly double _letterSpacingValue;
		private readonly double _lineHeightValue;

		public Measure WordSpacing => new Measure(_wordSpacingUnits, _wordSpacingValue);
		public Measure TextIndent => new Measure(_textIndentUnits, _textIndentValue);
		public Measure LetterSpacing => new Measure(_letterSpacingUnits, _letterSpacingValue);
		public Measure LineHeight => new Measure(_lineHeightUnits, _lineHeightValue);

		public static ComputedTextStyle Default { get; } = new ComputedTextStyle(
			TextAlign.Start, WritingDirection.Ltr, TextTransform.None, WhiteSpaceKind.Normal,
			wordSpacing: Measure.Zero, textIndent: Measure.Zero, letterSpacing: Measure.Zero,
			lineHeight: new Measure(Units.Percent, 120));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ComputedTextStyle(TextAlign textAlign, WritingDirection direction,
			TextTransform textTransform, WhiteSpaceKind whiteSpace,
			Measure wordSpacing, Measure textIndent, Measure letterSpacing, Measure lineHeight)
		{
			TextAlign = textAlign;
			Direction = direction;
			TextTransform = textTransform;
			WhiteSpace = whiteSpace;

			_wordSpacingUnits = wordSpacing.Units;
			_textIndentUnits = textIndent.Units;
			_letterSpacingUnits = letterSpacing.Units;
			_lineHeightUnits = lineHeight.Units;

			_wordSpacingValue = wordSpacing.Value;
			_textIndentValue = textIndent.Value;
			_letterSpacingValue = letterSpacing.Value;
			_lineHeightValue = lineHeight.Value;
		}

		public ComputedTextStyle WithTextAlign(TextAlign textAlign)
			=> new ComputedTextStyle(textAlign, Direction, TextTransform, WhiteSpace,
				WordSpacing, TextIndent, LetterSpacing, LineHeight);
		public ComputedTextStyle WithDirection(WritingDirection direction)
			=> new ComputedTextStyle(TextAlign, direction, TextTransform, WhiteSpace,
				WordSpacing, TextIndent, LetterSpacing, LineHeight);
		public ComputedTextStyle WithTextTransform(TextTransform textTransform)
			=> new ComputedTextStyle(TextAlign, Direction, textTransform, WhiteSpace,
				WordSpacing, TextIndent, LetterSpacing, LineHeight);
		public ComputedTextStyle WithWhiteSpace(WhiteSpaceKind whiteSpace)
			=> new ComputedTextStyle(TextAlign, Direction, TextTransform, whiteSpace,
				WordSpacing, TextIndent, LetterSpacing, LineHeight);

		public ComputedTextStyle WithWordSpacing(Measure wordSpacing)
			=> new ComputedTextStyle(TextAlign, Direction, TextTransform, WhiteSpace,
				wordSpacing, TextIndent, LetterSpacing, LineHeight);
		public ComputedTextStyle WithTextIndent(Measure textIndent)
			=> new ComputedTextStyle(TextAlign, Direction, TextTransform, WhiteSpace,
				WordSpacing, textIndent, LetterSpacing, LineHeight);
		public ComputedTextStyle WithLetterSpacing(Measure letterSpacing)
			=> new ComputedTextStyle(TextAlign, Direction, TextTransform, WhiteSpace,
				WordSpacing, TextIndent, letterSpacing, LineHeight);
		public ComputedTextStyle WithLineHeight(Measure lineHeight)
			=> new ComputedTextStyle(TextAlign, Direction, TextTransform, WhiteSpace,
				WordSpacing, TextIndent, LetterSpacing, lineHeight);
	}
}
