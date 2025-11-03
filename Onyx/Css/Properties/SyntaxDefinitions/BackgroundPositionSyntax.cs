using Onyx.Css.Parsing;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.SyntaxDefinitions
{
	internal class BackgroundPositionSyntax<TProp> : Syntax<TProp>
		where TProp : class
	{
		public Func<TProp, BackgroundPosition, TProp> Constructor { get; }

		public BackgroundPositionSyntax(Func<TProp, BackgroundPosition, TProp> constructor)
		{
			Constructor = constructor;
		}

		private static readonly Dictionary<string, BackgroundPositionKeyword> _keywordLookup
			= new Dictionary<string, BackgroundPositionKeyword>
			{
				{ "center", BackgroundPositionKeyword.Center },
				{ "left", BackgroundPositionKeyword.Left },
				{ "right", BackgroundPositionKeyword.Right },
				{ "top", BackgroundPositionKeyword.Top },
				{ "bottom", BackgroundPositionKeyword.Bottom },
			};

		private enum BackgroundPositionKeyword
		{
			None = 0,

			Center,
			Left,
			Right,
			Top,
			Bottom,
		}

		// This one is so hideously complicated to get right that it requires a custom parser.
		//
		// [ left | center | right | top | bottom | <length-percentage> ]
		// |    [ left | center | right | <length-percentage> ]
		//      [ top | center | bottom | <length-percentage> ]
		// |    [ center | [ left | right ] <length-percentage>? ]
		//   && [ center | [ top | bottom ] <length-percentage>? ]
		public override TProp? Parse(CssLexer lexer, TProp property)
		{
			CssLexerPosition start = lexer.Here();

			// To get this right, we collect up to two position keywords and lengths/percentages
			// in order, and then attempt to pattern-match the result against the valid patterns,
			// which are:
			//
			//   - one keyword
			//   - one measure
			//   - left/center/right/measure and top/center/bottom/measure
			//   - left/center/right measure
			//   - measure top/center/bottom
			//   - "center center" (which is dumb but legal)
			//
			// This is horrible.

			Measure? firstMeasure = null, secondMeasure = null;
			BackgroundPositionKeyword? firstKeyword = null, secondKeyword = null;

			// Read the first component.
			if (!(firstMeasure = ReadLengthOrPercent(lexer)).HasValue
				&& !(firstKeyword = ReadKeyword(lexer)).HasValue)
			{
				lexer.Rewind(start);
				return null;
			}

			// Read the second component.
			if (!(secondMeasure = ReadLengthOrPercent(lexer)).HasValue)
				secondKeyword = ReadKeyword(lexer);

			BackgroundPosition position;

			if (!secondMeasure.HasValue && !secondKeyword.HasValue)
			{
				// At most one component.
				if (firstKeyword.HasValue)
				{
					// Any one keyword.
					position = new BackgroundPosition
					{
						X = PositionFromKeyword(firstKeyword.Value),
						Y = PositionFromKeyword(firstKeyword.Value)
					};
				}
				else if (firstMeasure.HasValue)
				{
					// Any one measure.
					position = new BackgroundPosition
					{
						X = firstMeasure.Value,
						Y = firstMeasure.Value
					};
				}
				else throw new InvalidOperationException("Internal error while parsing background position.");
			}
			else
			{
				// If there are two components, then the first keyword slot must not be
				// top/bottom, and the second keyword slot must not be left/right.
				if (firstKeyword == BackgroundPositionKeyword.Top
					|| firstKeyword == BackgroundPositionKeyword.Bottom
					|| secondKeyword == BackgroundPositionKeyword.Left
					|| secondKeyword == BackgroundPositionKeyword.Right)
				{
					lexer.Rewind(start);
					return null;
				}

				// Beyond that, anything goes.
				position = new BackgroundPosition
				{
					X = firstMeasure.HasValue
						? firstMeasure.Value
						: PositionFromKeyword(firstKeyword!.Value),

					Y = secondMeasure.HasValue
						? secondMeasure.Value
						: PositionFromKeyword(secondKeyword!.Value),
				};
			}

			property = Constructor(property, position);
			return property;
		}

		private Measure PositionFromKeyword(BackgroundPositionKeyword keyword)
			=> keyword switch
			{
				BackgroundPositionKeyword.Left => new Measure(Units.Percent, 0),
				BackgroundPositionKeyword.Top => new Measure(Units.Percent, 0),
				BackgroundPositionKeyword.Center => new Measure(Units.Percent, 50),
				BackgroundPositionKeyword.Right => new Measure(Units.Percent, 100),
				BackgroundPositionKeyword.Bottom => new Measure(Units.Percent, 100),
				_ => throw new InvalidOperationException("Unknown background position keyword: " + keyword),
			};

		private BackgroundPositionKeyword? ReadKeyword(CssLexer lexer)
		{
			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
			{
				lexer.Unget(token);
				return null;
			}

			return _keywordLookup.TryGetValue(token.Text!, out BackgroundPositionKeyword k) ? k : null;
		}

		public override string ToString()
			=> "<background-position>";
	}
}
