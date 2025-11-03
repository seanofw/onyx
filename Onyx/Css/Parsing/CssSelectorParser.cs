using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Css.Selectors;
using Onyx.Html.Dom;

//-----------------------------------------------------------------------------------------------
//
// Supported selector grammar follows the CSS 2.1 ruleset, as described here
// https://www.w3.org/TR/CSS2/grammar.html , with minor changes, mostly enhancements
// brought from CSS 3:
//
//   * A `compound_selector` topmost nonterminal has been added (as this parser only
//     parses selectors, not the full CSS grammar).
//   * The `HASH` terminal is replaced by an `id` nonterminal.
//   * The `FUNCTION` terminal is replaced by `IDENT '('` (consumed greedily).
//   * The `INCLUDES` and `DASHMATCH` operators are replaced by simple character
//     strings, and extended with `=*`, `=^`, and `=$` from CSS 3.
//   * The `pseudo` nonterminal has been extended to support pseudo-classes via `:`
//     and pseudo-elements via `::`, as per CSS 3.
//   * The "general sibling combinator" from CSS 3, `~`, is included in the combinators.
//   * The only terminals here are IDENT, STRING, and various forms of punctuation.
//
// This is the full grammar implemented, in LALR(1):
//
//   compound_selector: selector [ ',' S* selector ]*
//   selector: simple_selector [ combinator selector | S+ [ combinator? selector ]? ]?
//   combinator: '+' S* | '>' S* | '~' S*
//   simple_selector: element_name [ id | class | attrib | pseudo ]* | [ id | class | attrib | pseudo ]+
//   id: '#' IDENT
//   class: '.' IDENT
//   element_name: IDENT | '*'
//   attrib: '[' S* IDENT S* [ [ '=' | '~=' | '|=' | '*=' | '^=' | '$=' ] S* [ IDENT | STRING ] S* ]? ']'
//   pseudo: pseudo_class | pseudo_element
//   pseudo_class: ':' [ IDENT '(' S* [ IDENT S* ]? ')' | IDENT ]
//   pseudo_element: ':' ':' [ IDENT '(' S* [ IDENT S* ]? ')' | IDENT ]
//
//-----------------------------------------------------------------------------------------------

namespace Onyx.Css.Parsing
{
	/// <summary>
	/// One part of the CSS parser:  This knows how to parse CSS selectors,
	/// but not how to parse CSS declarations.  This does not apply recovery rules.
	/// </summary>
	public class CssSelectorParser
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
		public CssSelectorParser(Messages? messages = null, bool strict = false)
		{
			Messages = new Messages();
			_strict = strict;
		}

		#endregion

		#region Top-level API

		/// <summary>
		/// Parse a full compound selector (i.e., a selector that can include commas).
		/// Returns the parsed compound selector, or null if the input did not represent
		/// a valid selector.
		/// </summary>
		/// <remarks>
		/// Parse rule:
		///   compound_selector: . selector [ ',' S* selector ]*
		/// </remarks>
		/// <param name="lexer">The lexical analyzer that supplies tokens to the parser.
		/// On success, this will have been advanced past the selector.  On failure, this
		/// will have been advanced to the location of the failure but not beyond.</param>
		/// <param name="expectEoi">Whether to require that the input must not be followed by
		/// any content other than whitespace (true), or to allow other content to follow it
		/// (false).  The default is true.</param>
		/// <param name="throwOnError">If true, this will throw an exception on an error
		/// instead of simply returning null and recording the error in the Messages.
		/// (Note that this has the side effect of clearing the Messages collection.)</param>
		/// <returns>The compound selector that was read, or null if a syntax error was
		/// detected in the input.  Any warnings/errors will be emitted to the Messages
		/// collection.</returns>
		public CompoundSelector? ParseCompoundSelector(CssLexer lexer, bool expectEoi = true,
			bool throwOnError = false)
		{
			if (throwOnError)
				Messages.Clear();

			List<Selector> selectors = new List<Selector>();

			CssToken token;

			do
			{
				Selector? selector = ParseSelector(lexer, false);
				if (selector == null)
					return !throwOnError ? null
						: throw new ArgumentException(string.Join(", ", Messages.Select(m => m.Text)));

				selectors.Add(selector);

				SkipWhitespace(lexer);

			} while ((token = lexer.Next()).Kind == CssTokenKind.Comma);

			lexer.Unget(token);

			if (expectEoi && !ExpectEoi(lexer))
			{
				Messages.Add(new Message(MessageKind.Error, "Extra content at end of selector",
					lexer.Peek().SourceLocation));
				return !throwOnError ? null
					: throw new ArgumentException(string.Join(", ", Messages.Select(m => m.Text)));
			}

			return new CompoundSelector(selectors);
		}

		/// <summary>
		/// Parse a full standard selector (i.e., a selector that can include any
		/// syntactically valid selector content except commas).  Returns the parsed
		/// selector, or null if the input did not represent a valid selector.
		/// </summary>
		/// <remarks>
		/// Parse rule:
		///    selector: . simple_selector [ combinator selector | S+ [ combinator? selector ]? ]?
		///    combinator: '+' S* | '>' S*
		/// </remarks>
		/// <param name="lexer">The lexical analyzer that supplies tokens to the parser.
		/// On success, this will have been advanced past the selector.  On failure, this
		/// will have been advanced to the location of the failure but not beyond.</param>
		/// <param name="expectEoi">Whether to require that the input must not be followed by
		/// any content other than whitespace (true), or to allow other content to follow it
		/// (false).  The default is true.</param>
		/// <param name="throwOnError">If true, this will throw an exception on an error
		/// instead of simply returning null and recording the error in the Messages.
		/// (Note that this has the side effect of clearing the Messages collection.)</param>
		/// <returns>The selector that was read, or null if a syntax error was detected in
		/// the input.  Any warnings/errors will be emitted to the Messages collection.</returns>
		public Selector? ParseSelector(CssLexer lexer, bool expectEoi = true,
			bool throwOnError = false)
		{
			if (throwOnError)
				Messages.Clear();

			SkipWhitespace(lexer);

			SimpleSelector? simpleSelector = ParseSimpleSelector(lexer);
			if (simpleSelector == null)
				return !throwOnError ? null
					: throw new ArgumentException(string.Join(", ", Messages.Select(m => m.Text)));

			List<SelectorComponent> path = new List<SelectorComponent>();
			path.Add(new SelectorComponent(Combinator.Self, simpleSelector));

			while (true)
			{
				Combinator combinator = Combinator.Self;

			retry:
				CssToken token = lexer.Next();
				switch (token.Kind)
				{
					case CssTokenKind.Dot:
					case CssTokenKind.Ident:
					case CssTokenKind.LeftBracket:
					case CssTokenKind.Colon:
					case CssTokenKind.Star:
						// Start tokens for a simple_selector.
						lexer.Unget(token);
						goto parseSimple;

					case CssTokenKind.Tilde:
						SkipWhitespace(lexer);
						combinator = Combinator.GeneralSibling;
						goto parseSimple;

					case CssTokenKind.Plus:
						SkipWhitespace(lexer);
						combinator = Combinator.AdjacentSibling;
						goto parseSimple;

					case CssTokenKind.GreaterThan:
						SkipWhitespace(lexer);
						combinator = Combinator.Child;
						goto parseSimple;

					case CssTokenKind.Space:
						combinator = Combinator.Descendant;
						goto retry;

					parseSimple:
						simpleSelector = ParseSimpleSelector(lexer);
						if (simpleSelector == null)
							return !throwOnError ? null
								: throw new ArgumentException(string.Join(", ", Messages.Select(m => m.Text)));
						path.Add(new SelectorComponent(combinator, simpleSelector));
						break;

					default:
						lexer.Unget(token);
						Selector selector = new Selector(path);

						if (expectEoi && !ExpectEoi(lexer))
						{
							Messages.Add(new Message(MessageKind.Error, "Extra content at end of selector",
								lexer.Peek().SourceLocation));
							return !throwOnError ? null
								: throw new ArgumentException(string.Join(", ", Messages.Select(m => m.Text)));
						}

						return selector;
				}
			}
		}

		#endregion

		#region Private parsing methods

		/// <summary>
		/// Parse a simple selector.
		/// </summary>
		/// <remarks>
		/// Parse rule:
		///   simple_selector: . element_name [ id | class | attrib | pseudo ]*
		///                               | . [ id | class | attrib | pseudo ]+
		/// </remarks>
		/// <param name="lexer">The lexer supplying tokens.</param>
		/// <returns>The simple selector parsed, or null if there were errors in the input.</returns>
		private SimpleSelector? ParseSimpleSelector(CssLexer lexer)
		{
			List<SelectorFilter> filters = new List<SelectorFilter>();

			CssToken token = lexer.Next();
			string? elementName = null;

			if (token.Kind == CssTokenKind.Ident)
				elementName = token.Text?.ToLowerInvariant();
			else if (token.Kind == CssTokenKind.Star)
				elementName = "*";
			else
				lexer.Unget(token);

			SelectorFilter? filter;

			while (true)
			{
				token = lexer.Next();
				switch (token.Kind)
				{
					case CssTokenKind.Dot:
						if ((filter = ParseClassFilter(lexer)) == null)
							return null;
						filters.Add(filter);
						break;

					case CssTokenKind.Id:
						filters.Add(new SelectorFilterId(token.Text));
						break;

					case CssTokenKind.Colon:
						if ((filter = ParsePseudoFilter(lexer)) == null)
							return null;
						filters.Add(filter);
						break;

					case CssTokenKind.LeftBracket:
						if ((filter = ParseAttributeFilter(lexer)) == null)
							return null;
						filters.Add(filter);
						break;

					default:
						lexer.Unget(token);
						return elementName != null || filters.Count > 0
							? new SimpleSelector(elementName, filters)
							: null;
				}
			}
		}

		/// <summary>
		/// Parse a simple identifier-based filter, either a classname or an ID.
		/// </summary>
		/// <remarks>
		/// Parse rules:
		///   id: '#' . IDENT
		///   class: '.' . IDENT
		/// </remarks>
		/// <param name="lexer">The lexer supplying tokens.</param>
		/// <param name="kind">What kind of filter we're parsing (ID or classname).</param>
		/// <returns>The identifier filter parsed, or null if there were errors in the input.</returns>
		private SelectorFilter? ParseClassFilter(CssLexer lexer)
		{
			CssToken token;
			if ((token = lexer.Next()).Kind == CssTokenKind.Ident)
				return new SelectorFilterClass(token.Text);
			else
			{
				Error(token.SourceLocation, "Missing classname after '.'");
				lexer.Unget(token);
				return default;
			}
		}

		private static readonly IReadOnlyDictionary<string, (SelectorFilterKind, SelectorFilter)> _knownPseudoClasses
			= new Dictionary<string, (SelectorFilterKind, SelectorFilter)>
			{
				{ "first-child", (SelectorFilterKind.PseudoFirstChild, SelectorPseudoFirstChild.Instance) },
				{ "last-child", (SelectorFilterKind.PseudoLastChild, SelectorPseudoLastChild.Instance) },

				{ "link", (SelectorFilterKind.PseudoLink, SelectorPseudoStyleFlag.Link) },
				{ "visited", (SelectorFilterKind.PseudoVisited,	SelectorPseudoStyleFlag.Visited) },
				{ "hover", (SelectorFilterKind.PseudoHover, SelectorPseudoStyleFlag.Hover) },
				{ "active", (SelectorFilterKind.PseudoActive, SelectorPseudoStyleFlag.Active) },
				{ "focus", (SelectorFilterKind.PseudoFocus, SelectorPseudoStyleFlag.Focus) },
				{ "enabled", (SelectorFilterKind.PseudoEnabled,	SelectorPseudoStyleFlag.Enabled) },
				{ "disabled", (SelectorFilterKind.PseudoDisabled, SelectorPseudoStyleFlag.Disabled) },
				{ "checked", (SelectorFilterKind.PseudoChecked,	SelectorPseudoStyleFlag.Checked) },
				{ "indeterminate", (SelectorFilterKind.PseudoIndeterminate, SelectorPseudoStyleFlag.Indeterminate) },

				{ "empty", (SelectorFilterKind.PseudoEmpty, SelectorPseudoEmpty.Instance) },
			};

		private static readonly IReadOnlyDictionary<string, (SelectorFilterKind, SelectorFilter)> _knownPseudoElements
			= new Dictionary<string, (SelectorFilterKind, SelectorFilter)>
			{
			};

		private static readonly IReadOnlyDictionary<string, (SelectorFilterKind, Func<CompoundSelector?, SelectorFilter>)> _knownPseudosWithChildSelectors
			= new Dictionary<string, (SelectorFilterKind, Func<CompoundSelector?, SelectorFilter>)>
			{
				{ "is", (SelectorFilterKind.PseudoIs, (CompoundSelector? c) => new SelectorPseudoIsNot(false, c)) },
				{ "not", (SelectorFilterKind.PseudoNot, (CompoundSelector? c) => new SelectorPseudoIsNot(true, c)) },
			};

		/// <summary>
		/// Parse a pseudo-class or pseudo-element, or the function-based version of either.
		/// </summary>
		/// <remarks>
		/// Parse rules:
		///   pseudo: pseudo_class | pseudo_element
		///   pseudo_class: ':' . [ IDENT '(' S* [ IDENT S* ]? ')' | IDENT ]
		///   pseudo_element: ':' . ':' [ IDENT '(' S* [ IDENT S* ]? ')' | IDENT ]
		/// </remarks>
		/// <param name="lexer">The lexer supplying tokens.</param>
		/// <returns>The pseudo-class/pseudo-element parsed, or null if there were errors in the input.</returns>
		private SelectorFilter? ParsePseudoFilter(CssLexer lexer)
		{
			bool isPseudoElement;

			CssToken token;
			if ((token = lexer.Next()).Kind == CssTokenKind.Colon)
				isPseudoElement = true;
			else
			{
				isPseudoElement = false;
				lexer.Unget(token);
			}

			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
			{
				Error(token.SourceLocation, isPseudoElement
					? "Missing pseudo-element after '::'"
					: "Missing pseudo-class after ':'");
				lexer.Unget(token);
				return null;
			}

			string name = token.Text ?? string.Empty;
			if ((token = lexer.Next()).Kind != CssTokenKind.LeftParen)
			{
				lexer.Unget(token);

				IReadOnlyDictionary<string, (SelectorFilterKind Kind, SelectorFilter Filter)> table
					= isPseudoElement ? _knownPseudoElements : _knownPseudoClasses;
				if (table.TryGetValue(name, out (SelectorFilterKind Kind, SelectorFilter Filter) entry))
					return entry.Filter;

				return new SelectorUnknownPseudoClass(isPseudoElement, name, null);
			}

			if (!isPseudoElement && _knownPseudosWithChildSelectors.TryGetValue(name,
				out (SelectorFilterKind Kind, Func<CompoundSelector?, SelectorFilter> Constructor) entry2))
			{
				// Handle cases where the filter function is actually another selector.
				CompoundSelector? args = ParseCompoundSelector(lexer);
				SkipWhitespace(lexer);

				if ((token = lexer.Next()).Kind != CssTokenKind.RightParen)
				{
					Error(token.SourceLocation, $"Missing ')' after pseudo-class function ':{name}()'");
					lexer.Unget(token);
					return default;
				}

				return entry2.Constructor(args);
			}

			string value = ParseCustomSelectorContent(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.RightParen)
			{
				Error(token.SourceLocation, isPseudoElement
					? $"Missing ')' after pseudo-element function '::{name}()'"
					: $"Missing ')' after pseudo-class function ':{name}()'");
				lexer.Unget(token);
				return default;
			}

			return new SelectorUnknownPseudoClass(isPseudoElement, name, value);
		}

		/// <summary>
		/// In pseudoclass functions that don't contain a child selector inside them,
		/// we collect just identifiers and whitespace until we run out.
		/// </summary>
		/// <param name="lexer">The lexer to eat whitespace and identifier tokens from.</param>
		/// <returns>The resulting string, with all whitespace compacted to single spaces,
		/// and the whole string trimmed.</returns>
		private string ParseCustomSelectorContent(CssLexer lexer)
		{
			StringBuilder stringBuilder = new StringBuilder();

			while (true)
			{
				CssToken token = lexer.Next();
				if (token.Kind == CssTokenKind.Space)
				{
					if (stringBuilder.Length > 0 && stringBuilder[^1] != ' ')
						stringBuilder.Append(' ');
				}
				else if (token.Kind == CssTokenKind.Ident)
				{
					stringBuilder.Append(token.Text);
				}
				else break;
			}

			if (stringBuilder.Length > 0 && stringBuilder[^1] == ' ')
				stringBuilder.Length--;

			return stringBuilder.ToString();
		}

		/// <summary>
		/// Parse an attribute filter of the form `[ident op value]`, where the
		/// operator is any of the CSS 3 attribute operators, and value is either
		/// a bare identifier or a quoted string.
		/// </summary>
		/// <remarks>
		///   attrib: '[' . S* IDENT S* [ [ '=' | '~=' | '|=' | '*=' | '^=' | '$=' ]
		///                 S* [ IDENT | STRING ] S* ]? ']'
		/// </remarks>
		/// <param name="lexer">The lexer supplying tokens.</param>
		/// <returns>The attribute filter parsed, or null if there were errors in the input.</returns>
		private SelectorFilter? ParseAttributeFilter(CssLexer lexer)
		{
			CssToken token;

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind != CssTokenKind.Ident)
			{
				Error(token.SourceLocation, "Missing attribute name after '['");
				lexer.Unget(token);
				return null;
			}
			string? attrName = token.Text?.ToLowerInvariant();

			SkipWhitespace(lexer);

			SelectorFilterKind selectorFilterKind = SelectorFilterKind.None;

			token = lexer.Next();
			switch (token.Kind)
			{
				case CssTokenKind.RightBracket:
					return new SelectorFilterHasAttrib(attrName);

				case CssTokenKind.Equal:
					selectorFilterKind = SelectorFilterKind.AttribEq;
					break;

				case CssTokenKind.TildeEq:
					selectorFilterKind = SelectorFilterKind.AttribIncludes;
					break;

				case CssTokenKind.BarEq:
					selectorFilterKind = SelectorFilterKind.AttribDashMatch;
					break;

				case CssTokenKind.CaretEq:
					selectorFilterKind = SelectorFilterKind.AttribStartsWith;
					break;

				case CssTokenKind.DollarEq:
					selectorFilterKind = SelectorFilterKind.AttribEndsWith;
					break;

				case CssTokenKind.StarEq:
					selectorFilterKind = SelectorFilterKind.AttribContains;
					break;

				default:
					Error(token.SourceLocation, "Invalid operator in attribute selector");
					lexer.Unget(token);
					return default;
			}

			SkipWhitespace(lexer);

			string? attrValue;
			if ((token = lexer.Next()).Kind == CssTokenKind.Ident)
				attrValue = token.Text;
			else if (token.Kind == CssTokenKind.String)
				attrValue = token.Text;
			else
			{
				Error(token.SourceLocation, "Invalid value in attribute selector");
				lexer.Unget(token);
				return default;
			}

			SkipWhitespace(lexer);

			if ((token = lexer.Next()).Kind == CssTokenKind.Ident)
			{
				string? caseSensitivity = token.Text;
				SkipWhitespace(lexer);

				if (caseSensitivity == "i" || caseSensitivity == "I")
					selectorFilterKind = selectorFilterKind - SelectorFilterKind.AttribEq + SelectorFilterKind.CaseInsensitive;
				else if (caseSensitivity == "s" || caseSensitivity == "S")
					selectorFilterKind = selectorFilterKind - SelectorFilterKind.AttribEq + SelectorFilterKind.CaseSensitive;
				else
				{
					Error(token.SourceLocation, "Attribute suffix must be either 'i' or 's'");
					return default;
				}
			}
			else lexer.Unget(token);

			if ((token = lexer.Next()).Kind != CssTokenKind.RightBracket)
			{
				Error(token.SourceLocation, "Missing ']' at end of attribute selector");
				lexer.Unget(token);
				return default;
			}

			return new SelectorFilterAttrib(selectorFilterKind, attrName, attrValue);
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
