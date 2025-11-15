using Onyx.Extensions;
using Onyx.Html.Dom;

namespace Onyx.Html.Parsing
{
	/// <summary>
	/// Standards-compliant HTML5 parser.
	/// </summary>
	public class HtmlParser
	{
		#region Nested classes

		/// <summary>
		/// A rule describing how to untangle a mismatched end tag.
		/// </summary>
		private readonly struct MismatchRule
		{
			/// <summary>
			/// Acceptable alternate end tags.
			/// </summary>
			public IReadOnlySet<string> ClosingTags { get; }

			/// <summary>
			/// Start tags that imply we shouldn't search further up the tree for a matching end tag.
			/// </summary>
			public IReadOnlySet<string> InterruptingTags { get; }

			/// <summary>
			/// Construct a new MismatchRule.
			/// </summary>
			/// <param name="closingTags">Acceptable alternate end tags.</param>
			/// <param name="interruptingTags">Start tags that imply we shouldn't search further up the tree for a matching end tag.</param>
			public MismatchRule(IEnumerable<string> closingTags, IEnumerable<string> interruptingTags)
			{
				ClosingTags = closingTags.ToHashSet();
				InterruptingTags = interruptingTags.ToHashSet();
			}
		}

		#endregion

		#region Static data

		/// <summary>
		/// The table of mismatch rules, which describe how to handle end tags that don't close
		/// the correct ancestor.
		/// </summary>
		private static readonly IReadOnlyDictionary<string, MismatchRule> _mismatchRules =
			new Dictionary<string, MismatchRule>
			{
				{ "li", new MismatchRule(["li"], ["ul", "ol", "menu"]) },
				{ "dt", new MismatchRule(["dt", "dd"], ["dl"]) },
				{ "dd", new MismatchRule(["dt", "dd"], ["dl"]) },
				{ "p", new MismatchRule(["p"], Element.BlockLevelElements) },
				{ "rt", new MismatchRule(["rt", "rp"], ["ruby"]) },
				{ "rp", new MismatchRule(["rt", "rp"], ["ruby"]) },
				{ "optgroup", new MismatchRule(["option", "optgroup"], ["select"]) },
				{ "option", new MismatchRule(["option"], ["optgroup", "select"]) },
				{ "thead", new MismatchRule(["tbody", "tfoot"], ["table"]) },
				{ "tbody", new MismatchRule(["tbody"], ["table"]) },
				{ "tfoot", new MismatchRule(["tbody", "tfoot"], ["table"]) },
				{ "tr", new MismatchRule(["tr"], ["tbody", "thead", "tfoot", "table"]) },
				{ "td", new MismatchRule(["td", "th"], ["tr", "tbody", "thead", "tfoot", "table"]) },
				{ "th", new MismatchRule(["td", "th"], ["tr", "tbody", "thead", "tfoot", "table"]) },
				{ "colgroup", new MismatchRule(["colgroup"], ["table"]) },
			};

		#endregion

		#region Public properties

		/// <summary>
		/// The set of error messages to write to any time the parser discovers bad HTML.
		/// </summary>
		public Messages Messages { get; }

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new parser.
		/// </summary>
		/// <param name="messages">The set of error messages to write to any time the parser discovers bad HTML.</param>
		public HtmlParser(Messages? messages = null)
		{
			Messages = new Messages();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Parse an HTML document, and return it.  This handles bad markup the same way that
		/// HTML5 does, by resolving per the usual quirky rules.
		/// </summary>
		/// <remarks>
		/// While the intent here is generally an HTML5 standards-compliant parser, we *do*
		/// have a few small but notable differences here, largely to support the needs of
		/// Onyx's primary use case of building application software.  These differences are
		/// noted below:
		/// <list type="bullet">
		/// <item>When errors are encountered in the HTML, such as mismatched tags, the HTML5
		/// resolution rules are applied, but a warning is issued to the Messages, so that
		/// the author of the bad markup can discover the mistake and fix it.  This is only
		/// a warning, not an error; the markup will still work, but the parser will rightly
		/// complain about bad buggy markup so you can fix it.</item>
		/// <item>The {html}, {head}, and {body} tags do nothing, along with everything that
		/// traditionally goes inside {head}.  This parses the HTML body content only, and
		/// anything that is metadata about the "document" belongs in real code.</item>
		/// <item>If you include a {head} section, it will be parsed, but it will not be
		/// included in the rendered content.</item>
		/// <item>The {style} and {script} tags are ignored as well.  (You may, however, locate
		/// any {style} elements in the document and then feed them into a CssParser to produce
		/// a usable Stylesheet.)</item>
		/// <item>CDATA sections are not supported.  If you need large chunks of data content,
		/// put them in real XML, or JSON, or SQL, or CSV, or any of a hundred other places
		/// you can store your data.</item>
		/// </list>
		/// </remarks>
		/// <param name="text">The HTML text to parse.</param>
		/// <param name="filename">The name of the document, for error-reporting purposes.</param>
		/// <returns>The resulting document fragment.</returns>
		public Document Parse(string text, string filename)
		{
			HtmlLexer lexer = new HtmlLexer(text, filename, Messages);

			Document fragment = new Document();
			ParseTokens(lexer, fragment);

			return fragment;
		}

		/// <summary>
		/// Parse an HTML document fragment, and return it.  This handles bad markup the
		/// same way that HTML5 does, by resolving per the usual quirky rules.  This is really
		/// not fundamentally different than Parse(), but just has less overhead, since
		/// DocumentFragment is a much less useful root than Document is.
		/// </summary>
		public DocumentFragment ParseDocumentFragment(string text, string filename)
		{
			HtmlLexer lexer = new HtmlLexer(text, filename, Messages);

			DocumentFragment fragment = new DocumentFragment();
			ParseTokens(lexer, fragment);

			return fragment;
		}

		private static readonly HtmlParser _innerHtmlParser = new HtmlParser();

		/// <summary>
		/// Parse HTML text for the InnerHtml property of an element.
		/// </summary>
		/// <param name="text">The HTML text to parse.</param>
		/// <param name="parent">The element that will be the new parent.</param>
		internal static void ParseInnerHtml(string text, ContainerNode parent)
		{
			_innerHtmlParser.Messages.Clear();

			parent.Clear();

			HtmlLexer lexer = new HtmlLexer(text, "<innerHtml>", _innerHtmlParser.Messages);

			_innerHtmlParser.ParseTokens(lexer, parent);
		}

		/// <summary>
		/// Parse HTML text for the OuterHtml property of an element.
		/// </summary>
		/// <param name="text">The HTML text to parse.</param>
		/// <returns>The new DocumentFragment.</returns>
		internal static DocumentFragment ParseOuterHtml(string text)
			=> _innerHtmlParser.ParseDocumentFragment(text, "<outerhtml>");

		#endregion

		#region Private internal mechanics

		/// <summary>
		/// Internal parse method: Eat tokens off the lexer until none are left, and attach
		/// all elements that are parsed to the provided root node.
		/// </summary>
		/// <param name="lexer">The lexical analyzer that provides HTML tokens upon request.</param>
		/// <param name="nodeStack">The stack of HTML containing nodes.  This must not be empty;
		/// some kind of containing node must exist at the start of this call, to which
		/// all descendants will be attached.</param>
		private void ParseTokens(HtmlLexer lexer, ContainerNode root)
		{
			NodeStack<ContainerNode> nodeStack = new NodeStack<ContainerNode>(64);
			nodeStack.PushNode(root);

			HtmlToken token;
			while ((token = lexer.Next()).Kind != HtmlTokenKind.Eoi)
			{
				if (token.Kind == HtmlTokenKind.Text)
				{
					nodeStack.CurrentNode.AppendChildFastAndUnsafe(new TextNode(token.Text)
						{ SourceLocation = token.SourceLocation });
				}
				else if (token.Kind == HtmlTokenKind.Comment)
				{
					nodeStack.CurrentNode.AppendChildFastAndUnsafe(new CommentNode(token.Text)
						{ SourceLocation = token.SourceLocation });
				}
				else if (token.Kind == HtmlTokenKind.StartTag)
				{
					Element element = MakeElement(token);

					EnsureTreeAllowsStartTag(nodeStack, element);

					nodeStack.CurrentNode.AppendChildFastAndUnsafe(element);

					if (token.Text.StartsWith("!")
						|| Element.AutoClosingTags.Contains(element.NodeName))
					{
						// Auto-closing tag, so this generates a node but doesn't end up on the stack.
					}
					else if (Element.RawContentTags.Contains(element.NodeName))
					{
						// Consume raw content inside <script>, <style>, <xmp>, and <plaintext>.
						string text = lexer.ConsumeToMarker("</" + element.NodeName,
							StringComparison.OrdinalIgnoreCase);
						element.AppendChild(new TextNode(text)
							{ SourceLocation = token.SourceLocation });
					}
					else
					{
						// Push this opening node onto the tag stack, in hopes that we'll eventually
						// have the closing tag for it.
						nodeStack.PushNode(element);
					}
				}
				else if (token.Kind == HtmlTokenKind.EndTag)
				{
					if (token.Text == nodeStack.CurrentNode.NodeName)
					{
						// We got a match, so this is a clean closing tag.
						nodeStack.PopNode();
					}
					else
					{
						// No match, which means the document is malformed.  This is where
						// things get interesting, because we have to apply rules to recover.
						Warning(token.SourceLocation, $"Mismatched end tag </{token.Text}>");
						RecoverFromMismatchedEndTag(nodeStack, token.Text, pop: true);
					}
				}
			}
		}

		/// <summary>
		/// Given that an end tag has been reached and the previous start tag is
		/// an invalid match for it, this attempts to search backward through the NodeStack
		/// to find the correct start tag to close, if any, following HTML5's weird and
		/// quirky byzantine rules.  This will pop nodes off the stack wherever they are
		/// inappropriate.
		/// </summary>
		/// <param name="nodeStack">The NodeStack to search through.</param>
		/// <param name="name">The name of the start tag that we're searching for.</param>
		/// <param name="pop">Whether to pop the start tag (true) if it's found, or to
		/// leave it on the stack (false).  If the start tag is not found, this parameter
		/// is irrelevant.</param>
		private void RecoverFromMismatchedEndTag(NodeStack<ContainerNode> nodeStack, string name, bool pop)
		{
			// Look up the correct recovery rule.
			IReadOnlySet<string> closingTags, interruptingTags;
			if (_mismatchRules.TryGetValue(name, out MismatchRule mismatchRule))
			{
				// Explicit recovery rule.
				closingTags = mismatchRule.ClosingTags;
				interruptingTags = mismatchRule.InterruptingTags;
			}
			else
			{
				// No explicit rule, so we just try to search back to a matching
				// start tag, or if the search gets interrupted by another block-
				// level element, we stop trying to recover outright.
				closingTags = new HashSet<string> { name };
				interruptingTags = Element.BlockLevelElements;
			}

			// Search back to find a start tag that matches this end tag.  We never
			// pop the root node.
			while (nodeStack.Count > 1)
			{
				string currentNodeName = nodeStack.CurrentNode.NodeName;
				if (closingTags.Contains(currentNodeName))
				{
					// Recovered back to a valid start tag, so pop it.
					nodeStack.PopNode();
					break;
				}
				else if (interruptingTags.Contains(currentNodeName))
				{
					// Have to stop here, because the search for an opening tag has
					// been interrupted by a tag with higher precedence.
					break;
				}

				// Didn't find it, so remove this start tag as "broken" and try again higher.
				Warning(nodeStack.CurrentNode.SourceLocation, $"<{currentNodeName}> has a missing or mismatched end tag.");
				nodeStack.PopNode();
			}
		}

		/// <summary>
		/// Not all start tags are allowed at all places in the tree; for example, {li} must
		/// be placed inside {ul} or {ol}.  Verify that the provided element can be attached
		/// to the tree at the current position in the NodeStack, and if not, modify the
		/// NodeStack so that this element will be allowed.
		/// </summary>
		/// <param name="nodeStack">The NodeStack to analyze, and possibly to update.</param>
		/// <param name="element">The element that must be added to the tree.</param>
		private void EnsureTreeAllowsStartTag(NodeStack<ContainerNode> nodeStack, Element element)
		{
			if (Element.BlockLevelElements.Contains(element.NodeName))
			{
				// Block-level elements must be placed inside other block-level elements.
				// Also "p" is weird and is always automatically closed by a block-level element.
				while (nodeStack.CurrentNode.Parent != null &&
					(!Element.BlockLevelElements.Contains(nodeStack.CurrentNode.NodeName)
						|| nodeStack.CurrentNode.NodeName == "p"))
				{
					Warning(element.SourceLocation,
						$"<{element.NodeName}> cannot be placed inside <{nodeStack.CurrentNode.NodeName}>"
						+ (nodeStack.CurrentNode.SourceLocation != null ? $"on line {nodeStack.CurrentNode.SourceLocation.Line}" : string.Empty));
					nodeStack.PopNode();
				}
				return;
			}

			// There are also special cases in HTML 5.
			switch (element.NodeName)
			{
				case "dt":
				case "dd":
					EnforceAncestor(element, nodeStack, searchFor: ["dl", "dt", "dd"], createIfMissing: "dl");
					if (nodeStack.CurrentNode?.NodeName == "dt" || nodeStack.CurrentNode?.NodeName == "dd")
						nodeStack.PopNode();
					break;

				case "li":
					EnforceAncestor(element, nodeStack, searchFor: ["ol", "ul"], createIfMissing: "ul");
					break;

				case "tbody":
					EnforceAncestor(element, nodeStack, searchFor: ["table"], createIfMissing: "table");
					break;
				case "tr":
					EnforceAncestor(element, nodeStack, searchFor: ["thead", "tbody", "tfoot"], createIfMissing: "tbody");
					break;
				case "td":
				case "th":
					EnforceAncestor(element, nodeStack, searchFor: ["tr"], createIfMissing: "tr");
					break;

				default:
					// It's not block-level, and it's not special.  So assume it's
					// an inline element, which can be placed inside anything.
					break;
			}
		}

		/// <summary>
		/// For certain special cases of `EnsureTreeAllowsStartTag`, special rules must be
		/// applied.  This method applies special rules as generally as possible:  If a specific
		/// ancestor must exist, this pops the NodeStack back to that ancestor if possible, or
		/// inserts a specific new ancestor if popping is impossible, following the HTML5 spec.
		/// </summary>
		/// <param name="elementToAdd">The special element to be added to the tree.</param>
		/// <param name="nodeStack">The NodeStack that is to be searched, and possibly modified.</param>
		/// <param name="searchFor">Acceptable ancestor elements to search for.</param>
		/// <param name="createIfMissing">The type of synthetic element to create if none of
		/// the ancestors are found.</param>
		private void EnforceAncestor(Element elementToAdd,
			NodeStack<ContainerNode> nodeStack, string[] searchFor, string createIfMissing)
		{
			// Search backward on the stack for any node that matches the searchFor list.
			Element? ancestor = nodeStack.FindAncestor(searchFor) as Element;

			if (ancestor == null)
			{
				Warning(elementToAdd.SourceLocation,
					$"Parent element for <{elementToAdd.NodeName}> should be"
						+ (searchFor.Length == 1 ? $"<{searchFor[0]}>" : " one of " + string.Join(", ", searchFor.Select(e => $"<{e}>")))
						+ $"; inserting synthetic <{createIfMissing}>");

				// None of the expected ancestors exist anywhere, so push a synthetic start tag.
				ancestor = new Element(createIfMissing);
				EnsureTreeAllowsStartTag(nodeStack, ancestor);     // This may recursively create more ancestors as necessary.
				nodeStack.PushNode(ancestor);
			}
			else if (!ReferenceEquals(nodeStack.CurrentNode, ancestor))
			{
				// We found the required ancestor, but it's deeper in the stack, so pop
				// back to it, but don't remove it.
				Warning(elementToAdd.SourceLocation,
					$"Parent element for <{elementToAdd.NodeName}> is incorrectly <{nodeStack.CurrentNode.NodeName}> on line {nodeStack.CurrentNode.SourceLocation?.Line ?? 0}"
					+ $" but should be <{ancestor.NodeName}> on line {ancestor.SourceLocation?.Line ?? 0}");
				RecoverFromMismatchedEndTag(nodeStack, ancestor.NodeName, pop: false);
			}
		}

		/// <summary>
		/// Create a new element from the given HTML token (start tag).
		/// </summary>
		/// <param name="token">The HTML token that has just been read by the lexical analyzer.</param>
		/// <returns>The new element, with its attributes, and the SourceLocation copied from the token.
		/// The element name will be converted to lowercase, and the attribute names will as well.
		/// Duplicate attributes will be resolved by dropping all but the first.</returns>
		private Element MakeElement(HtmlToken token)
		{
			Element element = new Element(token.Text.FastLowercase())
				{ SourceLocation = token.SourceLocation };

			if (token.Attributes != null)
			{
				Dictionary<string, string> attributes = new Dictionary<string, string>();
				element.Attributes = new AttributeDictionary(element, attributes);
				foreach (KeyValuePair<string, string?> pair in token.Attributes)
				{
					attributes.TryAdd(pair.Key, pair.Value ?? string.Empty);
					element.OnAttrChange(pair.Key, null, pair.Value ?? string.Empty);
				}
			}

			return element;
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
