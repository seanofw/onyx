using System.Runtime.CompilerServices;
using System.Text;

namespace Onyx.Css.Parsing
{
	/// <summary>
	/// A lexical analyzer that can consume CSS text as input.  This obeys CSS 2.1 lexing
	/// rules, and on each invocation of `Next()`, returns the next CSS token in the input
	/// --- typically identifiers, numbers, punctuation, etc.  Note that whitespace *is*
	/// returned by this, as whitespace is significant in CSS.
	/// </summary>
	public class CssLexer
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

		private CssToken? _ungetToken;

		private int _start;
		private int _chunkStart;
		private StringBuilder? _stringBuilder;

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

		public CssLexer(string text, string filename, int line, int column, Messages? messages = null)
		{
			_filename = filename;
			_text = text;

			Messages = messages ?? new Messages();

			_ptr = 0;
			_line = line;
			_lineStart = -column;
		}

		public CssLexer(string text, string filename, Messages? messages = null)
		{
			_filename = filename;
			_text = text;

			Messages = messages ?? new Messages();

			_ptr = 0;
			_line = 1;
			_lineStart = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public CssLexerPosition Here()
			=> _ungetToken != null
				? new CssLexerPosition(_ungetToken.SourceLocation.Start, _line, _lineStart)
				: new CssLexerPosition(_ptr, _line, _lineStart);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public void Rewind(CssLexerPosition position)
		{
			_ptr = position.Ptr;
			_line = position.Line;
			_lineStart = position.LineStart;
			_ungetToken = null;
		}

		public CssToken Peek()
		{
			CssToken token = Next();
			Unget(token);
			return token;
		}

		public void Unget(CssToken token)
		{
			if (_ungetToken != null)
				throw new InvalidOperationException("Cannot unget more than one token.");
			_ungetToken = token;
		}

		public CssToken Next()
		{
			if (_ungetToken != null)
			{
				CssToken token = _ungetToken;
				_ungetToken = null;
				return token;
			}

			int startLine = _line;
			int startLineStart = _lineStart;
			int start = _ptr;

			if (_ptr >= _text.Length)
				return new CssToken(CssTokenKind.Eoi, null, default,
					new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

		retry:
			char ch;
			string? ident;
			switch (ch = _text[_ptr])
			{
				case '\x00': case '\x01': case '\x02': case '\x03':
				case '\x04': case '\x05': case '\x06': case '\x07':
				case '\x08': case '\x09': case '\x0A': case '\x0B':
				case '\x0C': case '\x0D': case '\x0E': case '\x0F':
				case '\x10': case '\x11': case '\x12': case '\x13':
				case '\x14': case '\x15': case '\x16': case '\x17':
				case '\x18': case '\x19': case '\x1A': case '\x1B':
				case '\x1C': case '\x1D': case '\x1E': case '\x1F':
				case '\x20':
					ConsumeWhitespace();
					return new CssToken(CssTokenKind.Space, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '/':
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == '*')
					{
						_ptr++;
						if (!ConsumeComment())
						{
							_lineStart = startLineStart;
							_line = startLine;
							_ptr = start;
							Error(start, startLine, startLineStart, 1, "Unclosed comment");
							_ptr++;
							return new CssToken(CssTokenKind.Slash, null, default,
								new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
						}
						goto retry;
					}
					return new CssToken(CssTokenKind.Slash, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '<':
					_ptr++;
					if (_ptr < _text.Length + 2
						&& _text[_ptr] == '!'
						&& _text[_ptr + 1] == '-'
						&& _text[_ptr + 2] == '-')
					{
						_ptr += 3;
						return new CssToken(CssTokenKind.Cdo, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					return new CssToken(CssTokenKind.LessThan, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '-':
					_ptr++;
					if (_ptr < _text.Length + 1
						&& _text[_ptr] == '-'
						&& _text[_ptr + 1] == '>')
					{
						_ptr += 2;
						return new CssToken(CssTokenKind.Cdc, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}

					if (_ptr < _text.Length && ((ch = _text[_ptr]) > 128 || _nameCharKind[ch] == 1 || ch == '\\'))
					{
						ident = ParseIdent()!;
						return new CssToken(CssTokenKind.Ident, ident, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}

					return new CssToken(CssTokenKind.Minus, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '^':
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == '=')
					{
						_ptr += 2;
						return new CssToken(CssTokenKind.CaretEq, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					return new CssToken(CssTokenKind.Caret, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '$':
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == '=')
					{
						_ptr += 2;
						return new CssToken(CssTokenKind.DollarEq, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					return new CssToken(CssTokenKind.Dollar, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '*':
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == '=')
					{
						_ptr += 2;
						return new CssToken(CssTokenKind.StarEq, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					return new CssToken(CssTokenKind.Star, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '~':
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == '=')
					{
						_ptr += 2;
						return new CssToken(CssTokenKind.TildeEq, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					return new CssToken(CssTokenKind.Tilde, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '|':
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == '=')
					{
						_ptr += 2;
						return new CssToken(CssTokenKind.BarEq, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					return new CssToken(CssTokenKind.Bar, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '0': case '1': case '2': case '3': case '4':
				case '5': case '6': case '7': case '8': case '9':
					(CssTokenKind tokenKind, double number, string? suffix) = ParseNumber();
					return new CssToken(tokenKind, suffix, number,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '.':
					_ptr++;
					if (_ptr < _text.Length && (ch = _text[_ptr]) >= '0' && ch <= '9')
					{
						_ptr--;
						(tokenKind, number, suffix) = ParseNumber();
						return new CssToken(tokenKind, suffix, number,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					return new CssToken(CssTokenKind.Dot, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case 'u':
					if (_ptr + 2 < _text.Length
						&& _text[_ptr + 1] == 'r'
						&& _text[_ptr + 2] == 'l'
						&& _text[_ptr + 3] == '(')
					{
						(tokenKind, ident) = ParseUrl(start, startLine, startLineStart);
						return new CssToken(tokenKind, ident, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					else goto case 'a';

				case 'a': case 'b': case 'c': case 'd': case 'e': case 'f':
				case 'g': case 'h': case 'i': case 'j': case 'k': case 'l':
				case 'm': case 'n': case 'o': case 'p': case 'q': case 'r':
				case 's': case 't':           case 'v': case 'w': case 'x':
				case 'y': case 'z':
				case 'A': case 'B': case 'C': case 'D': case 'E': case 'F':
				case 'G': case 'H': case 'I': case 'J': case 'K': case 'L':
				case 'M': case 'N': case 'O': case 'P': case 'Q': case 'R':
				case 'S': case 'T': case 'U': case 'V': case 'W': case 'X':
				case 'Y': case 'Z':
				case '\\':
					ident = ParseIdent()!;
					tokenKind = CssTokenKind.Ident;
					if (_ptr < _text.Length && _text[_ptr] == '(')
					{
						_ptr++;
						tokenKind = CssTokenKind.Func;
					}
					return new CssToken(tokenKind, ident, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '@':
					ident = ParseIdent();
					return new CssToken(CssTokenKind.At, ident, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '#':
					_ptr++;
					ident = ParseName();
					return new CssToken(CssTokenKind.Id, ident, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '!':
					ConsumeWhitespace();
					if (_ptr + 8 < _text.Length
						&& _text.AsSpan().Slice(_ptr).Equals("important", StringComparison.OrdinalIgnoreCase))
					{
						return new CssToken(CssTokenKind.Important, null, default,
							new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
					}
					_ptr = start + 1;
					_line = startLine;
					_lineStart = startLineStart;
					return new CssToken(CssTokenKind.Exclamation, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '\'':
				case '\"':
					(tokenKind, ident) = ParseString();
					return new CssToken(tokenKind, ident, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				case '=':
					_ptr++;
					return new CssToken(CssTokenKind.Equal, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '>':
					_ptr++;
					return new CssToken(CssTokenKind.GreaterThan, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '+':
					_ptr++;
					return new CssToken(CssTokenKind.Plus, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '&':
					_ptr++;
					return new CssToken(CssTokenKind.Ampersand, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '%':
					_ptr++;
					return new CssToken(CssTokenKind.Percent, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '`':
					_ptr++;
					return new CssToken(CssTokenKind.Backtick, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case ',':
					_ptr++;
					return new CssToken(CssTokenKind.Comma, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case ';':
					_ptr++;
					return new CssToken(CssTokenKind.Semicolon, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case ':':
					_ptr++;
					return new CssToken(CssTokenKind.Colon, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '?':
					_ptr++;
					return new CssToken(CssTokenKind.QuestionMark, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '(':
					_ptr++;
					return new CssToken(CssTokenKind.LeftParen, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case ')':
					_ptr++;
					return new CssToken(CssTokenKind.RightParen, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '[':
					_ptr++;
					return new CssToken(CssTokenKind.LeftBracket, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case ']':
					_ptr++;
					return new CssToken(CssTokenKind.RightBracket, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '{':
					_ptr++;
					return new CssToken(CssTokenKind.LeftBrace, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
				case '}':
					_ptr++;
					return new CssToken(CssTokenKind.RightBrace, null, default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));

				default:
					if (ch > 128)
						goto case 'a';
					return new CssToken(CssTokenKind.Error, "Unknown character or symbol", default,
						new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start));
			}
		}

		private (CssTokenKind TokenKind, string Url) ParseUrl(int urlStart, int startLine, int startLineStart)
		{
			ConsumeWhitespace();

			char ch;
			string url;
			int start = _ptr;

			if (_ptr < _text.Length && ((ch = _text[_ptr]) == '\'' || ch == '\"'))
			{
				CssTokenKind tokenKind;
				(tokenKind, url) = ParseString();
				if (tokenKind == CssTokenKind.Error)
					return (tokenKind, url);
			}
			else
			{
				StartParsingChunks();

				while (_ptr < _text.Length)
				{
					if ((ch = _text[_ptr]) == '\\')
					{
						NextChunk();
						char? escape = EatEscape();
						if (escape.HasValue)
							_stringBuilder!.Append(escape.Value);
					}
					else if (ch == '\"' || ch == '\'' || ch == '(' || ch == ')' || ch <= 32)
						break;
					else
						_ptr++;
				}

				url = FinishParsingChunks();
			}

			ConsumeWhitespace();

			if (_ptr >= _text.Length || _text[_ptr] != ')')
			{
				Error(urlStart, startLine, startLineStart, _ptr - urlStart, "Malformed url() expression");
			}

			return (CssTokenKind.Url, url);
		}

		private (CssTokenKind tokenKind, string url) ParseString()
		{
			char startCh = _text[_ptr++];
			char ch;

			int start = _start;
			int startLine = _line;
			int startLineStart = _lineStart;

			string content;

			StartParsingChunks();

			while (_ptr < _text.Length)
			{
				ch = _text[_ptr];
				if (ch == '\\')
				{
					NextChunk();
					if (_ptr + 1 < _text.Length && ((ch = _text[_ptr + 1]) == '\n' || ch == '\r'))
					{
						_ptr += 2;
						if (_ptr < _text.Length && _text[_ptr] == (ch ^ 7))
							_ptr++;
						continue;
					}

					char? escape = EatEscape();
					if (escape.HasValue)
						_stringBuilder!.Append(escape.Value);
				}
				else if (ch == startCh)
				{
					content = FinishParsingChunks();
					_ptr++;
					return (CssTokenKind.String, content);
				}
				else if (ch == '\r' || ch == '\n')
				{
					break;
				}
				else
					_ptr++;
			}

			Error(start, startLine, startLineStart, _ptr - start, "Malformed url() expression");
			content = FinishParsingChunks();
			return (CssTokenKind.String, content);
		}

		private (CssTokenKind tokenKind, double number, string? suffix) ParseNumber()
		{
			double number = 0;
			double multiplier = 1.0;

			int start = _ptr;

			char ch;
			while (_ptr < _text.Length && ((ch = _text[_ptr]) >= '0' && ch <= '9'))
			{
				number = (number * 10) + (ch - '0');
				_ptr++;
			}
			if (_ptr < _text.Length && _text[_ptr] == '.')
			{
				_ptr++;
				while (_ptr < _text.Length && ((ch = _text[_ptr]) >= '0' && ch <= '9'))
				{
					multiplier *= 0.1;
					number += multiplier * (ch - '0');
					_ptr++;
				}

				if (_ptr == start + 1)
					return (CssTokenKind.Dot, default, default);
			}

			string? suffix;
			if (_ptr < _text.Length)
			{
				ch = _text[_ptr];
				if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '-' || ch > 128 || ch == '\\')
				{
					suffix = ParseIdent();
					return (CssTokenKind.Number, number, suffix);
				}
				else if (ch == '%')
				{
					_ptr++;
					return (CssTokenKind.Percentage, number, null);
				}
			}

			return (CssTokenKind.Number, number, null);
		}

		private static readonly byte[] _nameCharKind =
		{
			0, 0, 0, 0, 0, 0, 0, 0,   0, 0, 0, 0, 0, 0, 0, 0,	// 0x00-0x0F
			0, 0, 0, 0, 0, 0, 0, 0,   0, 0, 0, 0, 0, 0, 0, 0,	// 0x10-0x1F
			0, 0, 0, 0, 0, 0, 0, 0,   0, 0, 0, 0, 0, 2, 0, 0,	// 0x20-0x2F
			2, 2, 2, 2, 2, 2, 2, 2,   2, 2, 0, 0, 0, 0, 0, 0,	// 0x30-0x3F
			0, 1, 1, 1, 1, 1, 1, 1,   1, 1, 1, 1, 1, 1, 1, 1,	// 0x40-0x4F
			1, 1, 1, 1, 1, 1, 1, 1,   1, 1, 1, 0, 5, 0, 0, 1,	// 0x50-0x5F
			0, 1, 1, 1, 1, 1, 1, 1,   1, 1, 1, 1, 1, 1, 1, 1,	// 0x60-0x6F
			1, 1, 1, 1, 1, 1, 1, 1,   1, 1, 1, 0, 0, 0, 0, 0,	// 0x70-0x7F
		};

		private string? ParseIdent()
		{
			if (_ptr >= _text.Length)
				return null;

			StartParsingChunks();

			// Optional starting hyphen.
			if (_text[_ptr] == '-')
				_ptr++;

			// CSS identifiers allow letters, underscores, Unicode, and escapes as the starting character.
			char ch;
			if (_ptr >= _text.Length)
			{
				_ptr = _start;
				return null;
			}
			ch = _text[_ptr];

			if (ch >= 128 || (_nameCharKind[ch] & 1) != 0)
				_ptr++;
			else if (ch == '\\')
			{
				NextChunk();
				char? escape = EatEscape();
				if (escape.HasValue)
					_stringBuilder!.Append(escape.Value);
			}

			// Eat successive identifier characters:  Letters, numbers, underscore, hyphen, Unicode, and escapes.
			while (_ptr < _text.Length)
			{
				ch = _text[_ptr];
				if (ch >= 128 || _nameCharKind[ch] != 0)
					_ptr++;
				else if (ch == '\\')
				{
					NextChunk();
					char? escape = EatEscape();
					if (!escape.HasValue)
						break;
					_stringBuilder!.Append(escape.Value);
				}
				else break;
			}

			return FinishParsingChunks();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void StartParsingChunks()
		{
			_start = _ptr;
			_chunkStart = _start;
			_stringBuilder = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private string FinishParsingChunks()
		{
			if (_chunkStart != _start)
			{
				NextChunk();
				return _stringBuilder!.ToString();
			}

			return _text.AsSpan().Slice(_start, _ptr - _start).ToString();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void NextChunk()
		{
			_stringBuilder ??= new StringBuilder();
			if (_ptr > _chunkStart)
				_stringBuilder.Append(_text.AsSpan().Slice(_chunkStart, _ptr - _chunkStart));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private char? EatEscape()
		{
			if (_ptr + 1 >= _text.Length)
			{
				Error(_start, _line, _lineStart, 1, "Invalid backslash escape");
				return '\xFFFD';
			}

			char ch = _text[++_ptr];
			if (HexDigitValue(ch) >= 0)
			{
				_ptr++;
				ch = EatUnicode();
			}
			else if (ch == '\r' || ch == '\n')
			{
				_ptr++;
				if (_ptr < _text.Length && _text[_ptr] == (ch ^ 7))
					_ptr++;
				_lineStart = _ptr;
				_line++;
			}
			else _ptr++;
			return ch;
		}

		private string? ParseName()
		{
			if (_ptr >= _text.Length)
				return null;

			StartParsingChunks();

			// Eat successive identifier characters:  Letters, numbers, underscore, hyphen, Unicode, and escapes.
			char ch;
			while (_ptr < _text.Length)
			{
				ch = _text[_ptr];
				if (ch >= 128 || _nameCharKind[ch] != 0)
					_ptr++;
				else if (ch == '\\')
				{
					NextChunk();
					char? escape = EatEscape();
					if (!escape.HasValue)
						break;
					_stringBuilder!.Append(escape.Value);
				}
				else break;
			}

			return FinishParsingChunks();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int HexDigitValue(char ch)
			=>    ch >= '0' && ch <= '9' ? ch
				: ch >= 'a' && ch <= 'f' ? ch - 'a' + 10
				: ch >= 'A' && ch <= 'F' ? ch - 'A' + 10
				: -1;

		private char EatUnicode()
		{
			// Consume between 1 and 6 hex digits, and convert them to a value and then
			// return it.  If greater than 0xFFFF, convert it to 0xFFFD (unrepresentable character).
			int value = 0;
			int digit;
			while (_ptr < _text.Length && (digit = HexDigitValue(_text[_ptr])) >= 0)
				value = (value << 4) | digit;

			// Unicode escapes allow a single optional trailing whitespace character.  For
			// these purposes, we treat CRLF and LFCR like a single character.
			char ch;
			if (_ptr < _text.Length && (ch = _text[_ptr]) <= 32)
			{
				if (ch == '\x0A' || ch == '\x0D')
				{
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == (ch ^ 7))
						_ptr++;
					_lineStart = _ptr;
					_line++;
				}
				else _ptr++;
			}

			return value < 0xFFFF ? (char)value : (char)0xFFFD;
		}

		private void ConsumeWhitespace()
		{
			char ch;
			while (_ptr < _text.Length)
			{
				if ((ch = _text[_ptr]) == '\x0A' || ch == '\x0D')
				{
					_ptr++;
					if (_ptr < _text.Length && _text[_ptr] == (ch ^ 7))
						_ptr++;
					_lineStart = _ptr;
					_line++;
					continue;
				}

				if (ch > '\x20')
					break;

				_ptr++;
			}
		}

		private bool ConsumeComment()
		{
			while (true)
			{
				if (_ptr + 1 >= _text.Length)
				{
					_ptr = _text.Length;
					return false;
				}
				else if (_text[_ptr] == '*' && _text[_ptr + 1] == '/')
				{
					_ptr += 2;
					return true;
				}
				else
					_ptr++;
			}
		}

		private void Error(int start, int startLine, int startLineStart, int length, string message)
			=> Messages.Add(new Message(MessageKind.Error, message,
				new SourceLocation(_filename, startLine, start - startLineStart, start, _ptr - start)));
	}
}
