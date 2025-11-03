using System.Runtime.CompilerServices;
using Onyx.Css.Properties;
using Onyx.Css.Properties.KnownProperties;

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
			Messages = new Messages();
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

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.Colon)
			{
				// No colon, so again, we're super-broken here.
				lexer.Unget(token);
				Messages.Add(new Message(MessageKind.Error, $"Missing ':' after style property '{name}'", token.SourceLocation));
				lexer.Rewind(startPosition);
				return null;
			}

			SkipWhitespace(lexer);

			CssLexerPosition propertyStart = lexer.Here();

			if (!PropertySyntaxDefinitions.Syntaxes.TryGetValue(kind, out MiniParser? miniParser))
			{
				// Don't know what this is.
				CssToken[] tokens = CollectInvalidTokens(lexer);
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
					CssToken[] tokens = CollectInvalidTokens(lexer);
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

		#region Support methods

		/// <summary>
		/// CSS parsing rules require that for invalid input, we must consume to the next
		/// closing ')', ']', '}', or ';', but must respect nesting.  So here's a fun recursive
		/// function that eats invalid declarations (probably).
		/// </summary>
		/// <param name="lexer">The lexer to eat invalid declarations from.</param>
		/// <returns>The tokens collected for the invalid property.</returns>
		private static CssToken[] CollectInvalidTokens(CssLexer lexer)
		{
			List<CssToken> tokens = new List<CssToken>();
			CollectInvalidTokens(lexer, tokens);
			return tokens.ToArray();
		}

		/// <summary>
		/// CSS parsing rules require that for invalid input, we must consume to the next
		/// closing ')', ']', '}', or ';', but must respect nesting.  So here's a fun recursive
		/// function that eats invalid declarations (probably).
		/// </summary>
		/// <param name="lexer">The lexer to eat invalid declarations from.</param>
		/// <param name="tokens">The tokens being collected for the invalid property.</param>
		private static void CollectInvalidTokens(CssLexer lexer, ICollection<CssToken> tokens)
		{
			CssToken token;

			while ((token = lexer.Next()).Kind != CssTokenKind.RightBrace
				&& token.Kind != CssTokenKind.RightParen
				&& token.Kind != CssTokenKind.RightBracket
				&& token.Kind != CssTokenKind.Semicolon
				&& token.Kind != CssTokenKind.Eoi)
			{
				if (token.Kind == CssTokenKind.LeftBrace)
				{
					while (true)
					{
						CollectInvalidTokens(lexer, tokens);
						if ((token = lexer.Next()).Kind == CssTokenKind.Eoi)
						{
							lexer.Unget(token);
							break;
						}
						else
						{
							tokens.Add(token);
							if (token.Kind == CssTokenKind.RightBrace)
								break;
						}
					}
				}
				else if (token.Kind == CssTokenKind.LeftBracket)
				{
					while (true)
					{
						CollectInvalidTokens(lexer, tokens);
						if ((token = lexer.Next()).Kind == CssTokenKind.Eoi)
						{
							lexer.Unget(token);
							break;
						}
						else
						{
							tokens.Add(token);
							if (token.Kind == CssTokenKind.RightBracket)
								break;
						}
					}
				}
				else if (token.Kind == CssTokenKind.LeftParen)
				{
					while (true)
					{
						CollectInvalidTokens(lexer, tokens);
						if ((token = lexer.Next()).Kind == CssTokenKind.Eoi)
						{
							lexer.Unget(token);
							break;
						}
						else
						{
							tokens.Add(token);
							if (token.Kind == CssTokenKind.RightParen)
								break;
						}
					}
				}
				else tokens.Add(token);
			}

			lexer.Unget(token);
		}

		/// <summary>
		/// Support method:  Skip optional whitespace.
		/// </summary>
		/// <param name="lexer">The lexer to eat whitespace tokens from.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SkipWhitespace(CssLexer lexer)
		{
			CssToken token;
			while ((token = lexer.Next()).Kind == CssTokenKind.Space) ;
			lexer.Unget(token);
		}

		/// <summary>
		/// Require an end-of-input marker, or emit an error if one is not found.
		/// </summary>
		/// <param name="lexer">The lexer that should be at the end of its input.</param>
		/// <returns>True if at the end of the input, false if more content was found.</returns>
		private bool ExpectEoi(CssLexer lexer)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Next()).Kind != CssTokenKind.Eoi)
			{
				lexer.Unget(token);
				Error(token.SourceLocation, "Unexpected content in selector");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Emit an error to the Messages collection.  In strict mode, this will be
		/// an actual error; in non-strict mode, this is emitted as a warning, since
		/// the rest of the CSS parser will just recover and skip this rule.
		/// </summary>
		/// <param name="sourceLocation">The location at which the error occurred.</param>
		/// <param name="message">The message to emit.</param>
		private void Error(SourceLocation? sourceLocation, string message)
		{
			Messages.Add(new Message(_strict ? MessageKind.Error : MessageKind.Warning, message, sourceLocation));
		}

		#endregion
	}
}
