using System.Runtime.CompilerServices;
using Onyx.Css.Properties;
using Onyx.Css.Selectors;

namespace Onyx.Css.Parsing
{
	public class CssParser
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

		private readonly CssSelectorParser _selectorParser;
		private readonly CssPropertyParser _propertyParser;
		private readonly CssMediaQueryParser _mediaQueryParser;
		private readonly CssSupportsQueryParser _supportsQueryParser;

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new parser.
		/// </summary>
		/// <param name="messages">The messages collection to which any additional messages
		/// will be added.  A messages collection will be created if one is not provided.</param>
		/// <param name="strict">Whether this is in strict mode or not.  In strict mode, all
		/// warnings will be emitted as errors.</param>
		public CssParser(Messages? messages = null, bool strict = false)
		{
			Messages = messages ?? new Messages();
			_strict = strict;

			_selectorParser = new CssSelectorParser(messages, strict);
			_propertyParser = new CssPropertyParser(messages, strict);
			_mediaQueryParser = new CssMediaQueryParser(messages, strict);
			_supportsQueryParser = new CssSupportsQueryParser(messages, strict);
		}

		#endregion

		#region Top-level API

		/// <summary>
		/// Parse a whole stylesheet.
		/// </summary>
		/// <param name="text">The text being parsed.</param>
		/// <param name="filename">The name of the file being parsed, for error reporting.</param>
		/// <returns>The parsed stylesheet.</returns>
		public Stylesheet Parse(string text, string filename)
			=> Parse(new CssLexer(text, filename, Messages));

		/// <summary>
		/// Parse a whole stylesheet.
		/// </summary>
		/// <param name="lexer">The lexical analyzer that supplies tokens to the parser.</param>
		/// <returns>The parsed stylesheet.</returns>
		public Stylesheet Parse(CssLexer lexer)
		{
			List<StyleRule> rules = new List<StyleRule>();
			ParseTopLevelDeclarations(lexer, rules, CssTokenKind.Eoi);
			return new Stylesheet(rules);
		}

		/// <summary>
		/// Parse CSS declarations until reaching a token that terminates this scope or EOI.
		/// </summary>
		/// <param name="lexer">The lexer providing the source tokens.</param>
		/// <param name="rules">The collection of rules that is being built.</param>
		/// <param name="endingToken">A token that ends this scope, like 'RightBrace'.  If this
		/// token is reached, it will be consumed.</param>
		/// <returns>True if the expected ending token was reached, or false if the scope was
		/// unterminated due to end-of-input.</returns>
		private bool ParseTopLevelDeclarations(CssLexer lexer, ICollection<StyleRule> rules,
			CssTokenKind endingToken)
		{
			while (true)
			{
				CssToken? badToken = ParseOneTopLevelDeclaration(lexer, rules);

				if (badToken != null)
				{
					if (badToken.Kind == endingToken)
					{
						lexer.Next();
						return true;
					}

					if (badToken.Kind == CssTokenKind.Eoi)
						return false;

					if (badToken.Kind != CssTokenKind.None)
					{
						if (badToken.Kind != CssTokenKind.Semicolon)
							Messages.Add(new Message(MessageKind.Error, $"Illegal '{badToken.Kind}'", badToken.SourceLocation));
						lexer.Next();
					}
				}
			}
		}

		/// <summary>
		/// Attempt to parse a single top-level CSS declaration.
		/// </summary>
		/// <param name="lexer">The lexer providing the source tokens.</param>
		/// <param name="rules">The collection of rules that is being built.</param>
		/// <returns>If no token could be consumed, this will be the failing token;
		/// otherwise, this will be None.</returns>
		private CssToken? ParseOneTopLevelDeclaration(CssLexer lexer, ICollection<StyleRule> rules)
		{
			SkipWhitespace(lexer);

			CssToken token;
			if ((token = lexer.Peek()).Kind == CssTokenKind.Eoi
				|| token.Kind == CssTokenKind.RightBrace
				|| token.Kind == CssTokenKind.RightParen
				|| token.Kind == CssTokenKind.RightBracket
				|| token.Kind == CssTokenKind.Semicolon)
				return token;

			if (token.Kind == CssTokenKind.At)
			{
				// A CSS at-rule, most likely.  This should be followed by an identifier;
				// if not, we discard it and attempt recovery to the next CSS declaration.
				lexer.Next();
				CssToken identToken;
				string ident;
				if ((identToken = lexer.Next()).Kind != CssTokenKind.Ident)
				{
					lexer.Unget(identToken);
					ident = string.Empty;
				}
				else ident = identToken.Text ?? string.Empty;

				switch (identToken.Text ?? string.Empty)
				{
					case "media":
						ParseMediaQuery(lexer, rules);
						break;

					case "supports":
						ParseSupportsQuery(lexer, rules);
						break;

					default:
						Messages.Add(new Message(MessageKind.Error, $"Invalid '@' rule", token.SourceLocation));
						CollectInvalidTokens(lexer);
						break;
				}

				return null;
			}

			CompoundSelector? selector = _selectorParser.ParseCompoundSelector(lexer, expectEoi: false);

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.LeftBrace)
			{
				Messages.Add(new Message(MessageKind.Error, $"Illegal '{token.Kind}' after selector", token.SourceLocation));
				lexer.Unget(token);
				return null;
			}

			SkipWhitespace(lexer);

			List<StyleProperty> properties = new List<StyleProperty>();
			ParsePropertyDeclarations(lexer, properties);

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.RightBrace)
			{
				Messages.Add(new Message(MessageKind.Error, $"Illegal '{token.Kind}' at end of property declarations", token.SourceLocation));
				lexer.Unget(token);
			}

			if (selector != null)
				rules.Add(new StyleRule(selector, new StylePropertySet(properties)));

			return null;
		}

		/// <summary>
		/// Parse a sequence of property declarations until a closing curly brace.
		/// </summary>
		/// <param name="lexer"></param>
		/// <param name="properties"></param>
		public void ParsePropertyDeclarations(CssLexer lexer, ICollection<StyleProperty> properties)
		{
			while (true)
			{
				SkipWhitespace(lexer);

				if (lexer.Peek().Kind == CssTokenKind.Eoi)
					break;

				StyleProperty? property = _propertyParser.ParseStyleProperty(lexer, expectEoi: false);
				if (property != null)
					properties.Add(property);

				SkipWhitespace(lexer);

				CssToken token;
				if ((token = lexer.Next()).Kind == CssTokenKind.RightBrace)
				{
					lexer.Unget(token);
					break;
				}
				else if (token.Kind != CssTokenKind.Semicolon)
				{
					Messages.Add(new Message(MessageKind.Error, $"Illegal '{token.Kind}' in property declarations", token.SourceLocation));

					lexer.Unget(token);

					while ((token = lexer.Next()).Kind != CssTokenKind.Semicolon
						&& token.Kind != CssTokenKind.Semicolon
						&& token.Kind != CssTokenKind.Eoi) ;

					lexer.Unget(token);
				}
			}
		}

		/// <summary>
		/// Parse a "@media" query and its child declarations.
		/// </summary>
		/// <param name="lexer">The lexer that provides tokens.</param>
		/// <param name="rules">The bag of rules being created.</param>
		private void ParseMediaQuery(CssLexer lexer, ICollection<StyleRule> rules)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Parse a "@supports" query and its child declarations, which in Onyx can test
		/// for property support and selector support...  by simply attempting to parse
		/// those and then answering true or false for them.
		/// </summary>
		/// <param name="lexer">The lexer that provides tokens.</param>
		/// <param name="rules">The bag of rules being created.</param>
		private void ParseSupportsQuery(CssLexer lexer, ICollection<StyleRule> rules)
		{
			throw new NotImplementedException();
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
		internal static CssToken[] CollectInvalidTokens(CssLexer lexer)
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
		internal static void CollectInvalidTokens(CssLexer lexer, ICollection<CssToken> tokens)
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
		internal static void SkipWhitespace(CssLexer lexer)
		{
			CssToken token;
			while ((token = lexer.Next()).Kind == CssTokenKind.Space) ;
			lexer.Unget(token);
		}

		#endregion
	}
}
