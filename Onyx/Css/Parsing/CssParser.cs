using System.Collections.Immutable;
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
			Messages = new Messages();
			_strict = strict;

			_selectorParser = new CssSelectorParser(messages, strict);
			_propertyParser = new CssPropertyParser(messages, strict);
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

			while (true)
			{
				SkipWhitespace(lexer);

				if (lexer.Peek().Kind == CssTokenKind.Eoi)
					break;

				CompoundSelector? selector = _selectorParser.ParseCompoundSelector(lexer, expectEoi: false);

				SkipWhitespace(lexer);

				CssToken token;
				if ((token = lexer.Next()).Kind != CssTokenKind.LeftBrace)
				{
					Messages.Add(new Message(MessageKind.Error, $"Illegal '{token.Kind}' after selector", token.SourceLocation));
					lexer.Unget(token);
					continue;
				}

				SkipWhitespace(lexer);

				List<StyleProperty> properties =  new List<StyleProperty>();
				ParsePropertyDeclarations(lexer, properties);

				SkipWhitespace(lexer);

				if ((token = lexer.Next()).Kind != CssTokenKind.RightBrace)
				{
					Messages.Add(new Message(MessageKind.Error, $"Illegal '{token.Kind}' at end of property declarations", token.SourceLocation));
					lexer.Unget(token);
				}

				if (selector != null)
					rules.Add(new StyleRule(selector, new StylePropertySet(properties)));
			}

			return new Stylesheet(rules);
		}

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

		#endregion

		#region Support methods

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