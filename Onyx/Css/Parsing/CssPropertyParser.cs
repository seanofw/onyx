using Onyx.Css.Properties;

namespace Onyx.Css.Parsing
{
	public class CssPropertyParser
	{
		#region Properties and fields

		/// <summary>
		/// The collection of messages (warnings/errors) emitted by the selector parser.
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
		public CssPropertyParser(Messages? messages = null, bool strict = false)
		{
			Messages = messages ?? new Messages();
			_strict = strict;
		}

		#endregion

		#region Top-level API

		/// <summary>
		/// Parse a property declaration of the form "key: values".
		/// </summary>
		/// <param name="lexer">The lexical analyzer that supplies tokens to the parser.
		/// On success, this will have been advanced past the property declaration.  On failure,
		/// this will have been advanced to the location of the failure but not beyond.</param>
		/// <param name="expectEoi">Whether to require that the input must not be followed by
		/// any content other than whitespace (true), or to allow other content to follow it
		/// (false).  The default is true.</param>
		/// <param name="throwOnError">If true, this will throw an exception on an error
		/// instead of simply returning null and recording the error in the Messages.
		/// (Note that this has the side effect of clearing the Messages collection.)</param>
		/// <returns>The style property that was read, or null if a syntax error was
		/// detected in the input.  Any warnings/errors will be emitted to the Messages
		/// collection.</returns>
		public StyleProperty? ParseStyleProperty(CssLexer lexer, bool expectEoi = true,
			bool throwOnError = false)
		{
			CssLexerPosition startPosition = lexer.Here();

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
			{
				// Don't even have a name for this property, so we have a legitimately
				// broken expression here.
				lexer.Unget(token);
				Messages.Add(new Message(MessageKind.Error, "Missing style property name", token.SourceLocation));
				return null;
			}
			SourceLocation sourceLocation = token.SourceLocation;
			string name = token.Text!;

			KnownPropertyKind kind = StyleProperty.PropertyKindLookup.TryGetValue(name, out KnownPropertyKind k)
				? k : KnownPropertyKind.Unknown;

			CssParser.SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.Colon)
			{
				// No colon, so again, we're super-broken here.
				lexer.Unget(token);
				Messages.Add(new Message(MessageKind.Error, $"Missing ':' after style property '{name}'", token.SourceLocation));
				lexer.Rewind(startPosition);
				return null;
			}

			CssParser.SkipWhitespace(lexer);

			CssLexerPosition propertyStart = lexer.Here();

			if (!PropertySyntaxDefinitions.Syntaxes.TryGetValue(kind, out MiniParser? miniParser))
			{
				// Don't know what this is.
				CssToken[] tokens = CssParser.CollectInvalidTokens(lexer);
				return new UnknownProperty { Name = name, Tokens = tokens };
			}

			StyleProperty styleProperty = (StyleProperty)miniParser.MakeNew();

			string? text;
			if ((token = lexer.Next()).Kind == CssTokenKind.Ident
				&& ((text = token.Text) == "inherit" || text == "initial" || text == "unset"))
			{
				// This is just declared as "inherit", "initial", or "unset", with nothing else,
				// so skip the real parsing.
				char ch;
				if ((ch = text[2]) == 'h')   // Fast test for "inherit".
				{
					styleProperty = styleProperty with
					{
						SourceLocation = sourceLocation,
						Kind = kind,
						Inherit = true
					};
				}
				else if (ch == 'i')   // Fast test for "initial".
				{
					styleProperty = styleProperty with
					{
						SourceLocation = sourceLocation,
						Kind = kind,
						Initial = true
					};
				}
				else   // "unset".
				{
					styleProperty = styleProperty with
					{
						SourceLocation = sourceLocation,
						Kind = kind,
						Unset = true
					};
				}
			}
			else
			{
				lexer.Unget(token);

				styleProperty = styleProperty with
				{
					SourceLocation = sourceLocation,
					Kind = kind
				};

				// We have a syntax for it, so try to parse it for real.  If that fails, back up
				// and just collect tokens as an invalid style property.
				StyleProperty? parsedProperty = miniParser.Syntax.OuterParse(lexer, styleProperty) as StyleProperty;
				if (parsedProperty == null)
				{
					lexer.Rewind(propertyStart);
					CssToken[] tokens = CssParser.CollectInvalidTokens(lexer);
					return new UnknownProperty { Name = name, Tokens = tokens };
				}

				styleProperty = parsedProperty;
			}

			// Check for "!important" on the end.
			CssLexerPosition importantStart = lexer.Here();
			if ((token = lexer.Next()).Kind == CssTokenKind.Exclamation)
			{
				if ((token = lexer.Next()).Kind == CssTokenKind.Ident
					&& (token.Text?.Equals("important", StringComparison.OrdinalIgnoreCase) ?? false))
				{
					// It's !important.
					styleProperty = styleProperty with { Important = true };
				}
				else lexer.Rewind(importantStart);
			}
			else lexer.Unget(token);

			// We got it, so return it.
			return styleProperty;
		}

		#endregion
	}
}
