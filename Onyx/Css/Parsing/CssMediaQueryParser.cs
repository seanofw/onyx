using System.Collections.Concurrent;
using Onyx.Css.Types;
using Onyx.Css.Types.Media;
using Onyx.Extensions;

namespace Onyx.Css.Parsing
{
	public class CssMediaQueryParser
	{
		#region Properties and fields

		/// <summary>
		/// The collection of messages (warnings/errors) emitted by the media query parser.
		/// </summary>
		public Messages Messages { get; }

		/// <summary>
		/// Whether we are parsing in strict mode, in which all warnings are emitted as errors.
		/// </summary>
		private readonly bool _strict;

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new parser.
		/// </summary>
		/// <param name="messages">The messages collection to which any additional messages
		/// will be added.  A messages collection will be created if one is not provided.</param>
		/// <param name="strict">Whether this is in strict mode or not.  In strict mode, all
		/// warnings will be emitted as errors.</param>
		public CssMediaQueryParser(Messages? messages = null, bool strict = false)
		{
			Messages = messages ?? new Messages();
			_strict = strict;
		}

		#endregion

		private static MediaQueryEnumParser<MediaType> _mediaTypeParser = new MediaQueryEnumParser<MediaType>();
		private static MediaQueryEnumParser<MediaFeature> _mediaFeatureParser = new MediaQueryEnumParser<MediaFeature>();

		private static ConcurrentDictionary<Type, MediaQueryEnumParser> _enumParsers =
			new ConcurrentDictionary<Type, MediaQueryEnumParser>();

		// <media-query-list> = . [[ <media-query ]]*
		public MediaQuery? ParseMediaQueryList(CssLexer lexer, bool expectEoi = true)
		{
			CssLexerPosition position = lexer.Here();

			MediaQuery? mediaQuery = MediaQueryNull.Instance;

			CssToken commaToken;
			do
			{
				MediaQuery? next = ParseMediaQuery(lexer, expectEoi: false);

				// We may have a syntax error in the last query we read.  In order
				// to recover well, we really want to consume until the next ',' or '{'
				// or ';' or '}' or ']' or ')' so that we at least have a chance of
				// recognizing the rest of the media query.  It's still a broken media
				// query either way, but we're trying to help the developer have better
				// error messages if we can.
				CssParser.SkipWhitespace(lexer);

				if ((commaToken = lexer.Next()).Kind != CssTokenKind.Comma
					&& commaToken.Kind != CssTokenKind.Semicolon
					&& commaToken.Kind != CssTokenKind.LeftBrace
					&& commaToken.Kind != CssTokenKind.RightBrace
					&& commaToken.Kind != CssTokenKind.RightBracket
					&& commaToken.Kind != CssTokenKind.RightParen
					&& commaToken.Kind != CssTokenKind.Eoi)
				{
					// This query is surely broken, so mark it as "...and error".
					if (next == null || !next.HasErrors)
						next = next != null
							? new MediaQueryAnd(next, MediaQueryError.Instance)
							: MediaQueryError.Instance;

					// Try to recover.
					do
					{
						lexer.Unget(commaToken);
						CssParser.CollectOneInvalidToken(lexer);
						CssParser.SkipWhitespace(lexer);
					} while ((commaToken = lexer.Next()).Kind != CssTokenKind.Comma
						&& commaToken.Kind != CssTokenKind.Semicolon
						&& commaToken.Kind != CssTokenKind.LeftBrace
						&& commaToken.Kind != CssTokenKind.RightBrace
						&& commaToken.Kind != CssTokenKind.RightBracket
						&& commaToken.Kind != CssTokenKind.RightParen
						&& commaToken.Kind != CssTokenKind.Eoi);
				}

				lexer.Unget(commaToken);

				if (next != null)
				{
					mediaQuery = mediaQuery != MediaQueryNull.Instance
						? new MediaQueryOr(mediaQuery, next, isComma: true)
						: next;
				}
			}
			while ((commaToken = lexer.Next()).Kind == CssTokenKind.Comma);

			lexer.Unget(commaToken);

			if (expectEoi)
			{
				CssParser.SkipWhitespace(lexer);

				CssToken token;
				if ((token = lexer.Next()).Kind != CssTokenKind.Eoi)
				{
					Warn(token.SourceLocation, "Syntax error in media query expression");
					goto fail;
				}
			}

			return mediaQuery;

		fail:
			lexer.Rewind(position);
			return null;
		}

		// <media-query> = . <media-condition>
		//              | . [ not | only ]? <media-type> [ and <media-condition-without-or> ]?
		// <media-type> = . <ident>
		public MediaQuery? ParseMediaQuery(CssLexer lexer, bool expectEoi = true)
		{
			CssLexerPosition position = lexer.Here();

			MediaQuery? mediaQuery = ParseMediaCondition(lexer, allowOr: true);
			if (mediaQuery != null)
				return mediaQuery;

			bool not = false;
			if (TryConsumeKeyword(lexer, "not"))
				not = true;
			else if (TryConsumeKeyword(lexer, "only"))
				{ /* OK */ }

			CssParser.SkipWhitespace(lexer);

			CssToken token = lexer.Peek();
			if (token.Kind != CssTokenKind.Ident)
			{
				Warn(token.SourceLocation, "Missing required media type to start media query");
				goto fail;
			}

			mediaQuery = _mediaTypeParser.TryConsume(lexer, out MediaType mediaType)
				? new MediaQueryMediaType(mediaType)
				: MediaQueryError.Instance;

			if (not)
				mediaQuery = new MediaQueryNot(mediaQuery);

			if (TryConsumeKeyword(lexer, "and"))
			{
				CssParser.SkipWhitespace(lexer);

				token = lexer.Peek();
				MediaQuery? and = ParseMediaCondition(lexer, allowOr: false);
				if (and == null)
				{
					Warn(token.SourceLocation, "Invalid 'and' expression in media query");
					and = MediaQueryError.Instance;
				}
				mediaQuery = new MediaQueryAnd(mediaQuery, and);
			}

			if (expectEoi)
			{
				CssParser.SkipWhitespace(lexer);

				if ((token = lexer.Next()).Kind != CssTokenKind.Eoi)
				{
					Warn(token.SourceLocation, "Syntax error in media query expression");
					mediaQuery = mediaQuery != null && mediaQuery != MediaQueryNull.Instance
						? new MediaQueryAnd(mediaQuery, MediaQueryError.Instance)
						: MediaQueryError.Instance;
				}
			}

			return mediaQuery;

		fail:
			lexer.Rewind(position);
			return null;
		}

		// <media-condition> = . <media-not> | . <media-in-parens> [ <media-and>* | <media-or>* ]
		// <media-condition-without-or> = . <media-not> | . <media-in-parens> <media-and>*
		private MediaQuery? ParseMediaCondition(CssLexer lexer, bool allowOr)
		{
			CssLexerPosition position = lexer.Here();

			MediaQuery? mediaQuery = ParseMediaNotAndOr(lexer, "not");
			if (mediaQuery != null)
				return new MediaQueryNot(mediaQuery);

			mediaQuery = ParseMediaInParens(lexer);
			if (mediaQuery == null)
				goto fail;

			CssParser.SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Peek()).Kind == CssTokenKind.Ident)
			{
				if (token.Text == "and")
				{
					MediaQuery? and;
					while ((and = ParseMediaNotAndOr(lexer, "and")) != null)
					{
						mediaQuery = new MediaQueryAnd(mediaQuery, and);
					}
				}
				else if (allowOr && token.Text == "or")
				{
					MediaQuery? or;
					while ((or = ParseMediaNotAndOr(lexer, "or")) != null)
					{
						mediaQuery = new MediaQueryOr(mediaQuery, or, isComma: false);
					}
				}
			}

			return mediaQuery;

		fail:
			lexer.Rewind(position);
			return null;
		}

		// <media-not> = . not <media-in-parens>
		// <media-and> = . and <media-in-parens>
		// <media-or> = . or <media-in-parens>
		private MediaQuery? ParseMediaNotAndOr(CssLexer lexer, string keyword)
		{
			CssLexerPosition position = lexer.Here();

			CssParser.SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind == CssTokenKind.Ident
				&& token.Text == keyword)
			{
				MediaQuery? mediaQuery = ParseMediaInParens(lexer);
				if (mediaQuery == null)
				{
					Warn(token.SourceLocation, $"Invalid media query expression after '{keyword}'");
					return MediaQueryError.Instance;
				}

				return mediaQuery;
			}

			lexer.Rewind(position);
			return null;
		}

		// <media-in-parens> = . ( <media-condition> ) | . ( <media-feature> ) | . <general-enclosed>
		// <general-enclosed> = . [ <function-token> <any-value>? ) ] | . [ ( <any-value>? ) ]
		private MediaQuery? ParseMediaInParens(CssLexer lexer)
		{
			CssLexerPosition position = lexer.Here();

			CssParser.SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind == CssTokenKind.LeftParen)
			{
				MediaQuery? mediaQuery =
					   ParseMediaCondition(lexer, allowOr: true)
					?? ParseMediaFeature(lexer);

				if (mediaQuery != null)
				{
					CssParser.SkipWhitespace(lexer);
					if ((token = lexer.Next()).Kind == CssTokenKind.RightParen)
						return mediaQuery;
				}
				else
				{
					CssParser.CollectInvalidTokens(lexer);

					CssParser.SkipWhitespace(lexer);
					if ((token = lexer.Next()).Kind == CssTokenKind.RightParen)
						return MediaQueryError.Instance;
				}

				Warn(token.SourceLocation, "Missing right parenthesis after expression.");
				mediaQuery = MediaQueryError.Instance;
				return mediaQuery;
			}
			else if (token.Kind == CssTokenKind.Func)
			{
				CssParser.CollectInvalidTokens(lexer);

				CssParser.SkipWhitespace(lexer);
				if ((token = lexer.Next()).Kind == CssTokenKind.RightParen)
					return MediaQueryError.Instance;

				Warn(token.SourceLocation, "Missing right parenthesis after expression.");
				return MediaQueryError.Instance;
			}

			lexer.Rewind(position);
			return null;
		}

		// <media-feature> = . [ <mf-plain> | <mf-boolean> | <mf-range> ]
		// <mf-plain> = . <mf-name> : <mf-value>
		// <mf-boolean> = . <mf-name>
		// <mf-name> = . <ident>
		// <mf-range> = . <mf-name> <mf-comparison> <mf-value>
		// <mf-comparison> = <mf-lt> | <mf-gt> | <mf-eq>
		private MediaQuery? ParseMediaFeature(CssLexer lexer)
		{
			CssLexerPosition position = lexer.Here();

			CssParser.SkipWhitespace(lexer);

			MediaFeature feature;
			if (lexer.Peek().Kind != CssTokenKind.Ident)
			{
				// Doesn't start with a feature, so it's not anything we know how to parse;
				// bail to <mf-range>, which maybe knows how to parse this instead.
				lexer.Rewind(position);
				return ParseRange(lexer);
			}

			if (!_mediaFeatureParser.TryConsume(lexer, out feature))
			{
				feature = MediaFeature.Unknown;
				lexer.Next();	// It was at least an ident, so we can try to eat it.
			}

			CssParser.SkipWhitespace(lexer);

			CssToken featureToken = lexer.Peek();
			Type? type = MediaQueryFeature.GetFeatureType(feature);

			CssParser.SkipWhitespace(lexer);

			CssToken operatorToken = lexer.Next();
			if (operatorToken.Kind == CssTokenKind.Colon)
			{
				// Turn "min-X: N" and "max-X: N" into "X >= N" and "X <= N", respectively.
				MediaQueryKind kind = MediaQueryKind.Eq;
				if ((feature & MediaFeature.Min) != 0)
				{
					feature &= ~MediaFeature.Min;
					kind = MediaQueryKind.Ge;
				}
				else if ((feature & MediaFeature.Max) != 0)
				{
					feature &= ~MediaFeature.Max;
					kind = MediaQueryKind.Le;
				}

				// <mf-plain> = <mf-name> : <mf-value>
				object? value = ParseValue(lexer);
				MediaQuery? mediaQuery = CreateMediaQueryComparison(operatorToken.SourceLocation, kind, feature, value);
				if (mediaQuery != null)
					return mediaQuery;

				lexer.Rewind(position);
				return null;
			}
			else if (operatorToken.Kind == CssTokenKind.Equal
				|| operatorToken.Kind == CssTokenKind.LessThan
				|| operatorToken.Kind == CssTokenKind.GreaterThan)
			{
				if ((feature & (MediaFeature.Min | MediaFeature.Max)) != 0)
				{
					Warn(featureToken.SourceLocation, $"Media feature '{feature.ToString().Hyphenize()}' not allowed in comparison.");
					feature = MediaFeature.Unknown;
				}

				// <mf-range> = <mf-name> <mf-comparison> <mf-value>
				lexer.Unget(operatorToken);
				MediaQueryKind kind = ParseComparison(lexer);
				if (kind != MediaQueryKind.Unknown)
				{
					object? value = ParseValue(lexer);
					MediaQuery? mediaQuery = CreateMediaQueryComparison(operatorToken.SourceLocation, kind, feature, value);
					if (mediaQuery != null)
						return mediaQuery;
				}

				lexer.Rewind(position);
				return null;
			}
			else lexer.Unget(operatorToken);

			if ((feature & (MediaFeature.Min | MediaFeature.Max)) != 0)
			{
				Warn(featureToken.SourceLocation, $"Media feature '{feature.ToString().Hyphenize()}' requires a value to compare against.");
				feature = MediaFeature.Unknown;
			}

			// Plain form, where we have to cast just the feature name itself to a boolean.
			// This requires very cursed parsing rules :(
			return new MediaQueryNot(
				type == typeof(Measure)
					? MediaQueryComparison.Create(MediaQueryKind.Eq, feature, Measure.Zero)
				: type == typeof(double)
					? MediaQueryComparison.Create(MediaQueryKind.Eq, feature, 0.0)
				: type == typeof(MediaHoverKind)
					? MediaQueryComparison.CreateEnum(MediaQueryKind.Eq, feature, MediaHoverKind.None)
				: type == typeof(MediaPointerKind)
					? MediaQueryComparison.CreateEnum(MediaQueryKind.Eq, feature, MediaPointerKind.None)
				: type == typeof(MediaOverflowMode)
					? MediaQueryComparison.CreateEnum(MediaQueryKind.Eq, feature, MediaOverflowMode.None)
				: type == typeof(MediaUpdateMode)
					? MediaQueryComparison.CreateEnum(MediaQueryKind.Eq, feature, MediaUpdateMode.None)
				: MediaQueryError.Instance
			);
		}

		// <mf-range> = . <mf-value> <mf-comparison> <mf-name>
		//            | . <mf-value> <mf-lt> <mf-name> <mf-lt> <mf-value>
		//            | . <mf-value> <mf-gt> <mf-name> <mf-gt> <mf-value>
		private MediaQuery? ParseRange(CssLexer lexer)
		{
			CssLexerPosition position = lexer.Here();

			CssParser.SkipWhitespace(lexer);

			object? value = ParseValue(lexer);
			if (value == null)
				return MediaQueryError.Instance;

			CssParser.SkipWhitespace(lexer);

			CssToken token = lexer.Peek();
			MediaQueryKind firstComparison = ParseComparison(lexer);
			if (firstComparison == MediaQueryKind.Unknown)
			{
				Warn(token.SourceLocation, $"Missing comparison operator after value '{value}'");
				return MediaQueryError.Instance;
			}

			CssParser.SkipWhitespace(lexer);

			CssToken featureToken = lexer.Peek();
			MediaFeature feature;
			if (featureToken.Kind == CssTokenKind.Ident)
			{
				if (!_mediaFeatureParser.TryConsume(lexer, out feature))
				{
					Warn(featureToken.SourceLocation, $"Unknown media feature '{featureToken.Text}'");
					feature = MediaFeature.Unknown;
					lexer.Next();	// It was at least an ident, so we can eat it.
				}
				if ((feature & (MediaFeature.Min | MediaFeature.Max)) != 0)
				{
					Warn(featureToken.SourceLocation, $"Media feature '{feature.ToString().Hyphenize()}' not allowed in comparison.");
					feature = MediaFeature.Unknown;
				}
			}
			else
			{
				Warn(featureToken.SourceLocation, $"Missing media feature name to compare to value '{value}'");
				feature = MediaFeature.Unknown;
			}

			Type? type = MediaQueryFeature.GetFeatureType(feature);

			MediaQueryKind secondComparison = ParseComparison(lexer);
			if (secondComparison == MediaQueryKind.Unknown)
			{
				// <mf-value> <mf-comparison> <mf-name>
				MediaQuery? mediaQuery = CreateMediaQueryComparison(token.SourceLocation,
					MediaQueryComparison.FlipComparison(firstComparison), feature, value);
				if (mediaQuery != null)
					return mediaQuery;

				return MediaQueryError.Instance;
			}

			// <mf-value> <mf-comparison> <mf-name> <mf-comparison> <mf-value>

			CssParser.SkipWhitespace(lexer);

			CssToken secondToken = lexer.Peek();
			object? secondValue = ParseValue(lexer);
			if (secondValue == null)
				return MediaQueryError.Instance;

			// The comparisons must describe ranges.
			if (firstComparison != MediaQueryKind.Lt
				&& firstComparison != MediaQueryKind.Le
				&& firstComparison != MediaQueryKind.Gt
				&& firstComparison != MediaQueryKind.Ge)
			{
				Warn(secondToken.SourceLocation, $"Comparison operators for ranges must be '<' or '<=' or '>' or '>=' operators");
				return MediaQueryError.Instance;
			}

			// The comparison pairs must be compatible.
			if ((firstComparison == MediaQueryKind.Lt || firstComparison == MediaQueryKind.Le)
				&& !(secondComparison == MediaQueryKind.Lt || secondComparison == MediaQueryKind.Le))
			{
				Warn(secondToken.SourceLocation, $"Second comparison operator for range does not match the first");
				return MediaQueryError.Instance;
			}
			if ((firstComparison == MediaQueryKind.Gt || firstComparison == MediaQueryKind.Ge)
				&& !(secondComparison == MediaQueryKind.Gt || secondComparison == MediaQueryKind.Ge))
			{
				Warn(secondToken.SourceLocation, $"Second comparison operator for range does not match the first");
				return MediaQueryError.Instance;
			}

			// Transform the range comparison into a straightforward (a < x && x < b) style
			// pair of comparisons.  That's how it'll execute by the time it gets all the way
			// to the bottom of the expression tree anyway, so there's no point in holding onto it
			// as a first-class range.

			MediaQuery mediaQuery1 = CreateMediaQueryComparison(token.SourceLocation,
				MediaQueryComparison.FlipComparison(firstComparison), feature, value)
				?? MediaQueryError.Instance;

			MediaQuery mediaQuery2 = CreateMediaQueryComparison(secondToken.SourceLocation,
				secondComparison, feature, secondValue)
				?? MediaQueryError.Instance;

			return new MediaQueryAnd(mediaQuery1, mediaQuery2);
		}

		// <mf-lt> = . '<' '='?
		// <mf-gt> = . '>' '='?
		// <mf-eq> = . '='
		// <mf-comparison> = . <mf-lt> | . <mf-gt> | . <mf-eq>
		private MediaQueryKind ParseComparison(CssLexer lexer)
		{
			CssLexerPosition position = lexer.Here();

			CssParser.SkipWhitespace(lexer);

			CssToken token = lexer.Next();
			if (token.Kind == CssTokenKind.Equal)
				return MediaQueryKind.Eq;
			else if (token.Kind == CssTokenKind.LessThan)
			{
				if ((token = lexer.Next()).Kind == CssTokenKind.Equal)
					return MediaQueryKind.Le;
				lexer.Unget(token);
				return MediaQueryKind.Lt;
			}
			else if (token.Kind == CssTokenKind.GreaterThan)
			{
				if ((token = lexer.Next()).Kind == CssTokenKind.Equal)
					return MediaQueryKind.Ge;
				lexer.Unget(token);
				return MediaQueryKind.Gt;
			}

			lexer.Rewind(position);
			return MediaQueryKind.Unknown;
		}

		private MediaQuery? CreateMediaQueryComparison(SourceLocation location, MediaQueryKind kind, MediaFeature feature, object? value)
		{
			Type? enumType;

			if (value is Measure m)
				return MediaQueryComparison.Create(kind, feature, m);
			else if (value is double d)
				return MediaQueryComparison.Create(kind, feature, d);
			else if (value is string s && (enumType = value.GetType()).IsEnum)
				return MediaQueryComparison.CreateEnum(kind, feature, enumType, ResolveEnumType(location, s, enumType));
			else
			{
				Warn(location, $"Feature '{feature.ToString().Hyphenize()}' cannot be compared to value '{value}' because they are different types");
				return MediaQueryError.Instance;
			}
		}

		// <mf-value> = . <number> | . <dimension> | . <ident> | . <ratio>
		private object? ParseValue(CssLexer lexer)
		{
			CssLexerPosition position = lexer.Here();

			CssParser.SkipWhitespace(lexer);

			CssToken token = lexer.Next();
			if (token.Kind == CssTokenKind.Number)
			{
				if (!string.IsNullOrEmpty(token.Text))
				{
					if (!Measure.SuffixToUnits.TryGetValue(token.Text, out Units units))
					{
						Warn(token.SourceLocation, $"Unknown units '{token.Text}'");
						goto fail;
					}
					return new Measure(units, token.Number);
				}

				CssParser.SkipWhitespace(lexer);

				if (lexer.Peek().Kind == CssTokenKind.Slash)
				{
					// We don't support the <ratio> type as a distinct type, but we can parse
					// it into a number for good-enough support.

					double numerator = token.Number;

					lexer.Next();
					CssParser.SkipWhitespace(lexer);

					token = lexer.Next();
					if (token.Kind != CssTokenKind.Number || !string.IsNullOrEmpty(token.Text))
					{
						Warn(token.SourceLocation, "Missing denominator value for ratio");
						goto fail;
					}

					double denominator = token.Number;
					return denominator != 0
						? numerator / denominator
						: double.PositiveInfinity * Math.Sign(numerator) * Math.Sign(denominator);
				}

				return token.Number;
			}
			else if (token.Kind == CssTokenKind.Ident)
			{
				return token.Text;
			}

		fail:
			lexer.Rewind(position);
			return null;
		}

		private object? ResolveEnumType(SourceLocation sourceLocation, string? text, Type type)
		{
			MediaQueryEnumParser parser = _enumParsers.GetOrAdd(type, t =>
				(MediaQueryEnumParser)Activator.CreateInstance(typeof(MediaQueryEnumParser<>).MakeGenericType(t))!);

			if (parser.TryLookup(text, out object? value))
				return value;

			Warn(sourceLocation, $"Feature '{type.Name.Hyphenize()}' does not support a value '{text}'");
			return null;
		}

		#region Helper methods for parsing various atoms

		private bool TryConsumeKeyword(CssLexer lexer, string keyword)
		{
			CssLexerPosition position = lexer.Here();

			CssParser.SkipWhitespace(lexer);

			CssToken token = lexer.Next();
			if (token.Kind == CssTokenKind.Ident && token.Text == keyword)
				return true;

			lexer.Rewind(position);
			return false;
		}

		private abstract class MediaQueryEnumParser
		{
			public abstract bool TryConsume(CssLexer lexer, out object? value);
			public abstract bool TryLookup(string? text, out object? value);
		}

		private class MediaQueryEnumParser<TEnum> : MediaQueryEnumParser
			where TEnum : struct
		{
			private IReadOnlyDictionary<string, TEnum> _lookup;

			public MediaQueryEnumParser()
			{
				_lookup = CreateEnumLookup();
			}

			private Dictionary<string, TEnum> CreateEnumLookup()
			{
				Dictionary<string, TEnum> lookup = new Dictionary<string, TEnum>();

				foreach (string name in Enum.GetNames(typeof(TEnum)))
				{
					TEnum value = Enum.Parse<TEnum>(name, true);
					string cssName = name.Hyphenize();
					lookup[cssName] = value;
				}

				return lookup;
			}

			public bool TryConsume(CssLexer lexer, out TEnum value)
			{
				CssLexerPosition position = lexer.Here();

				CssParser.SkipWhitespace(lexer);

				CssToken token = lexer.Next();
				if (token.Kind == CssTokenKind.Ident
					&& _lookup.TryGetValue(token.Text ?? string.Empty, out value))
					return true;

				lexer.Rewind(position);
				value = default!;
				return false;
			}

			public bool TryLookup(string? text, out TEnum value)
				=> _lookup.TryGetValue(text ?? string.Empty, out value);

			public override bool TryConsume(CssLexer lexer, out object? value)
			{
				if (!TryConsume(lexer, out TEnum e))
				{
					value = default!;
					return false;
				}
				else
				{
					value = e;
					return true;
				}
			}

			public override bool TryLookup(string? text, out object? value)
			{
				if (!TryLookup(text, out TEnum e))
				{
					value = default!;
					return false;
				}
				else
				{
					value = e;
					return true;
				}
			}
		}

		#endregion

		#region Warnings and errors

		private void Warn(SourceLocation location, string message)
		{
			Messages.Add(new Message(_strict ? MessageKind.Error : MessageKind.Warning, message, location));
		}

		#endregion
	}
}
