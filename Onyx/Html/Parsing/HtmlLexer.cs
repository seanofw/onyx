using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Extensions;

namespace Onyx.Html.Parsing
{
	/// <summary>
	/// A lexical analyzer that can consume HTML text as input.  This obeys HTML5 lexing
	/// rules, and on each invocation of `Next()`, returns the next HTML token in the input
	/// --- a start tag, an end tag, a comment, or plain text.
	/// </summary>
	public class HtmlLexer
	{
		#region Private state

		/// <summary>
		/// The name of the file being lexed, for error-reporting.
		/// </summary>
		private readonly string _filename;

		/// <summary>
		/// The raw text of the file being lexed.
		/// </summary>
		private readonly string _text;

		/// <summary>
		/// The current read pointer.
		/// </summary>
		private int _ptr;

		/// <summary>
		/// The current line number.
		/// </summary>
		private int _line;
		
		/// <summary>
		/// A pointer into the text to the start character of the current line.
		/// </summary>
		private int _lineStart;

		/// <summary>
		/// Whether to "unget" the last token --- to push it back to the input
		/// so it can be consumed again.
		/// </summary>
		private HtmlToken? _ungetToken;

		/// <summary>
		/// A reused StringBuilder, for creating tag names and text chunks and
		/// attribute values and anything else that is lexed in small pieces.
		/// </summary>
		private StringBuilder _stringBuilder;

		#endregion

		#region Public/internal properties

		/// <summary>
		/// Access for unit tests to the current line number.
		/// </summary>
		internal int Line => _line;

		/// <summary>
		/// Access for unit tests to the current line start character.
		/// </summary>
		internal int LineStart => _lineStart;

		/// <summary>
		/// The collection of messages, for error reporting.
		/// </summary>
		public Messages Messages { get; }

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new HTML lexical analyzer for reading the given document.
		/// Each lexer can read exactly one document; if you need to parse more
		/// documents, use more lexer instances.
		/// </summary>
		/// <param name="text">The text to lex.</param>
		/// <param name="filename">The filename for that text, for error-reporting purposes.</param>
		/// <param name="messages">The messages to which errors/warnings will be written;
		/// if null, a new Messages collection will be created.</param>
		public HtmlLexer(string text, string filename, Messages? messages = null)
		{
			_filename = filename;
			_text = text;

			Messages = messages ?? new Messages();

			_ptr = 0;
			_line = 1;
			_lineStart = 0;

			_stringBuilder = new StringBuilder();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Peek at the next token without removing it from the input.
		/// </summary>
		/// <returns>The next token in the input.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HtmlToken Peek()
		{
			HtmlToken token = Next();
			Unget(token);
			return token;
		}

		/// <summary>
		/// Unget this token, so that it will be read again by Next().  Exactly
		/// one token may be un-gotten at a time (the unget stack is at most one entry deep).
		/// </summary>
		/// <param name="token">The token to unget.</param>
		/// <exception cref="InvalidOperationException">Thrown if Unget() is invoked more
		/// than once before Next() is invoked.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Unget(HtmlToken token)
		{
			if (_ungetToken != null)
				throw new InvalidOperationException("Cannot unget more than one token.");
			_ungetToken = token;
		}

		/// <summary>
		/// Read the next token from the input and return it, advancing the input pointer.
		/// </summary>
		/// <returns>The next token in the input.  At the end of the document, this will
		/// return a special EOI token to indicate that no more tokens remain.</returns>
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public HtmlToken Next()
		{
			char ch;

			if (_ungetToken != null)
			{
				HtmlToken result = _ungetToken;
				_ungetToken = null;
				return result;
			}

			if (_ptr >= _text.Length)
				return new HtmlToken(HtmlTokenKind.Eoi, string.Empty, null,
					new SourceLocation(_filename, _line, _ptr - _lineStart, _ptr, 0));

			int start = _ptr;
			int startLine = _line;
			int startColumn = _ptr - _lineStart;

			if (_text[_ptr] != '<')
			{
				// Plain text.
				while (_ptr < _text.Length && (ch = _text[_ptr]) != '<')
					EatChar(ch);

				_text.AsSpan().Slice(start, _ptr - start).HtmlDecodeTo(_stringBuilder);
				string result = _stringBuilder.ToString();
				_stringBuilder.Clear();
				return new HtmlToken(HtmlTokenKind.Text, result,
					null, new SourceLocation(_filename, startLine, startColumn, start, _ptr - start));
			}

			// An open tag, self-closing tag, or end tag, or possibly a comment.
			_ptr++;

			if (_ptr >= _text.Length)
			{
				// '<' with nothing after it is just text.
				SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
				Warning(location, "'<' should not be written in HTML - prefer &lt; instead");
				return new HtmlToken(HtmlTokenKind.Text, "<", null, location);
			}

			if ((ch = _text[_ptr]) == '!')
			{
				// Start of a document type node, or a comment.
				if (_ptr + 2 < _text.Length
					&& _text[_ptr + 1] == '-'
					&& _text[_ptr + 2] == '-')
				{
					// Start of a comment.  Consume to the closing '-->' symbol.
					_ptr += 3;

					while (true)
					{
						if (_ptr + 2 >= _text.Length)
						{
							_ptr = _text.Length;
							SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
							Warning(location, "Start comment marker '<!--' has no ending '-->' marker");
							return new HtmlToken(HtmlTokenKind.Comment,
								_text.AsSpan().Slice(start + 4, (_ptr - start) - 4).ToString(),    // Body of the comment, not including the markers
								null, location);
						}
						if ((ch = _text[_ptr]) == '-'
							&& _text[_ptr + 1] == '-'
							&& _text[_ptr + 2] == '>')
						{
							_ptr += 3;
							return new HtmlToken(HtmlTokenKind.Comment,
								_text.AsSpan().Slice(start + 4, (_ptr - start) - 7).ToString(),    // Body of the comment, not including the markers
								null,
								new SourceLocation(_filename, startLine, startColumn, start, _ptr - start));
						}

						EatChar(ch);
					}
				}
			}

			// Check for an initial slash for a closing tag.
			string name;
			if (ch == '/')
			{
				_ptr++;

				if (_ptr >= _text.Length)
				{
					// '<' with nothing after it is just text.
					SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
					Warning(location, "'<' should not be written in HTML - prefer &lt; instead");
					return new HtmlToken(HtmlTokenKind.Text, "</", null, location);
				}

				ch = _text[_ptr];
				if (!(ch >= 'a' && ch <= 'z'
					|| (ch >= 'A' && ch <= 'Z')))
				{
					// "</" that's not followed by text is just a floating '</' and
					// is emitted verbatim per the HTML 5 spec.
					SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
					Warning(location, "'<' should not be written in HTML - prefer &lt; instead");
					return new HtmlToken(HtmlTokenKind.Text, "</", null, location);
				}

				// Consume the name.
				name = CollectName();

				// HTML 5 spec disallows any characters other than '>' as the next character.
				// But realistically this just means we consume an optional '>'.
				if (_ptr < _text.Length && _text[_ptr] == '>')
					_ptr++;
				else
				{
					SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
					Warning(location, "Missing closing '>' on end tag");
				}

				return new HtmlToken(HtmlTokenKind.EndTag, name,
					null,
					new SourceLocation(_filename, startLine, startColumn, start, _ptr - start));
			}

			if (!(ch >= 'a' && ch <= 'z' || (ch >= 'A' && ch <= 'Z')))
			{
				// "<" that's not followed by '!' or '/' is just a floating '<' and
				// is emitted verbatim per the HTML 5 spec.
				SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
				Warning(location, "'<' should not be written in HTML - prefer &lt; instead");
				return new HtmlToken(HtmlTokenKind.Text, "<", null, location);
			}

			// Consume the name.
			name = CollectName();

			// Collect attributes, which are either isolated names, or are of the
			// form [name=value], or of the form [name="value"], possibly with whitespace
			// around the equal sign.  '/' is a disallowed character in an attribute name
			// or value, and if it's found, it's simply rejected.  This is, after all,
			// HTML, not XML, so self-closing tags don't actually exist here.
			List<KeyValuePair<string, string?>> attributes = new List<KeyValuePair<string, string?>>();
			while (true)
			{
				// Consume whitespace before the next attribute.
				SkipWhitespace();

				// Discard any slashes as bad input, and '>' ends the tag.
				if (_ptr >= _text.Length)
					break;
				if ((ch = _text[_ptr]) == '/')
				{
					if (_ptr + 1 < _text.Length && _text[_ptr + 1] != '>')
					{
						SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
						Warning(location, "HTML tags should not contain a floating '/' character");
					}
					_ptr++;
					continue;
				}
				else if (ch == '>')
				{
					_ptr++;
					break;
				}

				// Collect a name, if there is one.
				string attrName = CollectName();
				if (string.IsNullOrEmpty(attrName))
				{
					SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
					Warning(location, $"Illegal character '{ch}' in tag");
					_ptr++;		// Can't use this character for anything, so skip it.
					continue;
				}

				SkipWhitespace();

				string? attrValue;
				if (_ptr < _text.Length && _text[_ptr] == '=')
				{
					_ptr++;
					SkipWhitespace();

					// Got a value.  Is it quoted?
					if (_ptr < _text.Length && ((ch = _text[_ptr]) == '\"' || ch == '\''))
					{
						attrValue = CollectQuotedText(ch);
						if (_ptr + 1 < _text.Length && (ch = _text[_ptr + 1]) != '>' && ch != '/' && ch > 32)
						{
							SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
							Warning(location, "Extra text after attribute is ignored");
						}
						CollectName();	// Skip any trailing garbage immediately after a closing " character.
					}
					else
					{
						attrValue = CollectName();
						if (string.IsNullOrEmpty(attrValue))
							attrValue = null;
					}
				}
				else
				{
					attrValue = null;
				}

				attributes.Add(new KeyValuePair<string, string?>(attrName, attrValue));
			}

			return new HtmlToken(HtmlTokenKind.StartTag,
				name, attributes,
				new SourceLocation(_filename, startLine, startColumn, start, _ptr - start));
		}

		/// <summary>
		/// Skim through the input, searching for the given marker, and eating every
		/// character before that ending marker.
		/// </summary>
		/// <param name="marker">The marker to search for.</param>
		/// <param name="stringComparison">How to perform the search.</param>
		/// <returns>The content before that end tag.</returns>
		public string ConsumeToMarker(string marker, StringComparison stringComparison = StringComparison.Ordinal)
		{
			int endTagIndex = _text.IndexOf(marker, _ptr, stringComparison);
			if (endTagIndex < 0)
				endTagIndex = _text.Length;

			string result = _text.Substring(_ptr, endTagIndex - _ptr);
			_ptr = endTagIndex;

			return result;
		}

		#endregion

		#region Private internal mechanics

		/// <summary>
		/// Eat a single character from the input, handling newlines specially so that
		/// line numbers are correctly counted.  All four of '\r', '\n', '\r\n', and '\n\r'
		/// are supported as newlines.
		/// </summary>
		/// <param name="ch">The last character that was read from the input.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private void EatChar(char ch)
		{
			_ptr++;

			if (ch == '\n' || ch == '\r')
			{
				if (_ptr < _text.Length && _text[_ptr] == (ch ^ 7))
					_ptr++;
				_line++;
				_lineStart = _ptr;
			}
		}

		/// <summary>
		/// Read an HTML "name" (tag name or attribute name) from the input, and return it.
		/// This automatically performs HTML entity decoding on the name, resolving
		/// ampersand-something-semicolon sequences into their equivalent values.
		/// </summary>
		/// <returns>The "name" that was read.</returns>
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private string CollectName()
		{
			int nameStart = _ptr;

			char ch;
			while (_ptr < _text.Length && (ch = _text[_ptr]) > 32
				&& ch != '<' && ch != '>' && ch != '/' && ch != '\"' && ch != '=')
				EatChar(ch);

			int nameEnd = _ptr;
			_text.AsSpan().Slice(nameStart, nameEnd - nameStart).HtmlDecodeTo(_stringBuilder);
			string result = _stringBuilder.ToString();
			_stringBuilder.Clear();
			return result;
		}

		/// <summary>
		/// Collect a quoted string up to the given closing quotation character.
		/// </summary>
		/// <param name="startCh">The start-quotation character, also used when searching for
		/// the end-quotation character.</param>
		/// <returns>The resulting string value, HTML-entity-decoded.</returns>
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private string CollectQuotedText(char startCh)
		{
			int startLine = _line;
			int startColumn = _ptr - _lineStart;
			int start = _ptr;

			int textStart = ++_ptr;

			char ch;
			while (_ptr < _text.Length && (ch = _text[_ptr]) != startCh)
				EatChar(ch);

			int textEnd = _ptr;

			if (_ptr < _text.Length && _text[_ptr] == startCh)
				_ptr++;
			else
			{
				SourceLocation location = new SourceLocation(_filename, startLine, startColumn, start, _ptr - start);
				Warning(location, $"Missing closing {startCh} character on attribute value");
			}

			string text = textEnd > textStart
				? _text.AsSpan().Slice(textStart, textEnd - textStart).HtmlDecode()
				: string.Empty;

			return text;
		}

		/// <summary>
		/// Skip any whitespace in the next chunk of input, correctly tracking line numbers.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
		private void SkipWhitespace()
		{
			char ch;
			while (_ptr < _text.Length && (ch = _text[_ptr]) <= 32)
			{
				EatChar(ch);
			}
		}

		/// <summary>
		/// Issue a warning message to the collection of messages.
		/// </summary>
		/// <param name="sourceLocation">The location at which the message occurred in the input.</param>
		/// <param name="message">The text of the message to issue.</param>
		private void Warning(SourceLocation? sourceLocation, string message)
		{
			Messages.Add(new Message(MessageKind.Warning, message, sourceLocation));
		}

		#endregion
	}
}
