using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Css;
using Onyx.Css.Computed;
using Onyx.Css.Parsing;
using Onyx.Css.Properties;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A single element on a page.
	/// </summary>
	public class Element : ContainerNode, IAttributeNode
	{
		#region Properties and fields

		/// <summary>
		/// JS DOM: Elements are of type "Element."
		/// </summary>
		public override NodeType NodeType => NodeType.Element;

		/// <summary>
		/// The attributes of this element.  This is a dictionary-like construct that may
		/// be modified live to affect the element.
		/// </summary>
		public AttributeDictionary Attributes
		{
			get => _attributes ??= new AttributeDictionary(this);
			internal set => _attributes = value;
		}
		private AttributeDictionary? _attributes;

		/// <summary>
		/// JS DOM: The "id" of this element, if an "id" attribute has been assigned.  May be the
		/// empty string.  We hold the ID in a local field to avoid the dictionary lookup, which
		/// costs memory, but which allows retrieving this to run in true constant time (only
		/// a few clock cycles), which is important for selector performance, as IDs are very
		/// commonly queried in selectors.
		/// </summary>
		public string Id
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _id;

			set => Attributes["id"] = value;
		}
		private string _id = string.Empty;

		/// <summary>
		/// The "class" of this element, if a "class" attribute has been assigned.  May be
		/// the empty string.  This is the class as a simple whitespace-delimited string;
		/// but it's also available as a `HashSet{string}` in the `ClassNames` property for
		/// more efficient lookups.
		/// </summary>
		public string ClassName
		{
			get => Attributes.TryGetValue("class", out string? value) ? value : string.Empty;
			set => Attributes["class"] = value;
		}

		/// <summary>
		/// The ClassNames of this element.  This set may be empty but will never be null.
		/// Use either the Attributes dictionary or AddClass/RemoveClass to modify this.  We
		/// hold the classnames set in a local field, which costs memory, but which allows
		/// retrieving this set to run in true constant time (only a few clock cycles), which
		/// is important for selector performance, as classnames are very commonly queried
		/// in selectors.
		/// </summary>
		public IReadOnlySet<string> ClassNames => _classNames;
		private HashSet<string> _classNames;

		/// <summary>
		/// An empty collection of classnames, to avoid constructing classname sets on
		/// elements that have no classnames.
		/// </summary>
		private static readonly HashSet<string> _emptyClassNames = new HashSet<string>();

		/// <summary>
		/// The collection of parsed inline styles, if the "style" attribute has been assigned.
		/// </summary>
		public StylePropertySet InlineStyles => _inlineStyles ??= ParseInlineStyle();
		private StylePropertySet? _inlineStyles;

		/// <summary>
		/// A CSS parser used for parsing the style attribute, if this has a style attribute.
		/// </summary>
		private static readonly CssParser _inlineStyleParser = new CssParser();

		/// <summary>
		/// JS DOM: The name of this element.  Also called the "tag name."
		/// </summary>
		public override string NodeName { get; }

		/// <summary>
		/// A string that represents this element's name and attributes as an HTML "start tag."
		/// </summary>
		public string StartTag
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				AppendStartTag(stringBuilder);
				return stringBuilder.ToString();
			}
		}

		/// <summary>
		/// A string that represents this element's name as an "end tag," if this element
		/// type has an end tag.  If it is a self-closing tag, this will be the empty string.
		/// </summary>
		public string EndTag
			=> AutoClosingTags.Contains(NodeName) ? string.Empty : $"</{NodeName}>";

		#endregion

		#region Static data tables

		/// <summary>
		/// The standard six HTML auto-closing tags, as a set.
		/// </summary>
		internal static IReadOnlySet<string> AutoClosingTags { get; } = new HashSet<string>
		{
			"meta", "link", "img", "input", "br", "hr",
		};

		/// <summary>
		/// The standard four HTML tags that contain "raw content" (i.e., not HTML inside), as a set.
		/// </summary>
		internal static IReadOnlySet<string> RawContentTags { get; } = new HashSet<string>
		{
			"script", "style", "xmp", "plaintext",
		};

		/// <summary>
		/// The set of HTML block-level elements, for parsing purposes.
		/// </summary>
		internal static IReadOnlySet<string> BlockLevelElements { get; } = new HashSet<string>
		{
			"address", "article", "aside", "blockquote", "details",
			"dialog", "div", "dl", "fieldset", "figcaption", "figure",
			"footer", "form", "h1", "h2", "h3", "h4", "h5", "h6",
			"header", "hgroup", "hr", "main", "nav", "ol", "p",
			"pre", "section", "table", "ul",
			
			// Special to Onyx, but honestly should be standard:
			"row", "column",
		};

		#endregion

		#region Construction

		public Element(string name)
		{
			NodeName = name;
			_classNames = _emptyClassNames;
		}

		#endregion

		#region Classname management

		/// <summary>
		/// Given a classname, split it on whitespace into one or more non-whitespace names.
		/// </summary>
		/// <param name="className">The classname to split.</param>
		/// <returns>One or more classnames derived from the given source.</returns>
		internal static List<string> SplitClassname(string? className)
		{
			List<string> result = new List<string>();

			if (string.IsNullOrEmpty(className))
				return result;

			int ptr = 0;
			while (ptr < className.Length)
			{
				// Skip whitespace.
				while (ptr < className.Length
					&& className[ptr] <= 32)
					ptr++;

				// Collect non-whitespace.
				int start = ptr;
				while (ptr < className.Length
					&& className[ptr] > 32)
					ptr++;

				if (ptr > start)
					result.Add(className.Substring(start, ptr - start));
			}

			return result;
		}

		/// <summary>
		/// Modify the classnames on this element by adding one or more to it (space-delimited).
		/// </summary>
		/// <param name="className">One or more space-delimited classnames to add.</param>
		public void AddClass(string className)
			=> UpdateClassesInternal(SplitClassname(className), Array.Empty<string>());

		/// <summary>
		/// Modify the classnames on this element by removing one or more from it (space-delimited).
		/// </summary>
		/// <param name="className">One or more space-delimited classnames to remove.</param>
		public void RemoveClass(string className)
			=> UpdateClassesInternal(Array.Empty<string>(), SplitClassname(className));

		/// <summary>
		/// Modify the classnames on this element by toggling one or more on it (space-delimited).
		/// "Toggling" means adding a classname if it doesn't already exist, or removing
		/// it if it does already exist.
		/// </summary>
		/// <param name="className">One or more space-delimited classnames to add or remove.</param>
		public void ToggleClass(string className)
			=> ToggleClassesInternal(SplitClassname(className));

		/// <summary>
		/// Modify the classnames on this element by adding one or more to it (space-delimited),
		/// and also removing one or more from it (also space-delimited).  Removing classes will
		/// occur *before* adding classes, so any classname included in both sets will be added.
		/// </summary>
		/// <param name="add">One or more space-delimited classnames to add.</param>
		/// <param name="remove">One or more space-delimited classnames to remove.</param>
		public void UpdateClass(string add, string remove)
			=> UpdateClassesInternal(SplitClassname(add), SplitClassname(remove));

		/// <summary>
		/// Internal mechanics for adding or removing one or more classes in bulk.
		/// </summary>
		/// <param name="adds">Classnames to add.</param>
		/// <param name="removes">Classnames to remove.</param>
		internal void UpdateClassesInternal(IEnumerable<string> adds, IEnumerable<string> removes)
		{
			HashSet<string> clone = new HashSet<string>(_classNames);

			bool changed = false;
			foreach (string name in adds)
				changed |= clone.Add(name);
			foreach (string name in removes)
				changed |= clone.Remove(name);

			if (changed)
				ClassName = string.Join(' ', clone.OrderBy(c => c));
		}

		/// <summary>
		/// Internal mechanics for toggling one or more classes in bulk.
		/// </summary>
		/// <param name="names">Classnames to add or remove.</param>
		internal void ToggleClassesInternal(IEnumerable<string> names)
		{
			HashSet<string> clone = new HashSet<string>(_classNames);

			bool changed = false;
			foreach (string name in names)
			{
				if (clone.Contains(name))
					clone.Remove(name);
				else
					clone.Add(name);
				changed = true;
			}

			if (changed)
				ClassName = string.Join(' ', clone.OrderBy(c => c));
		}

		#endregion

		public override Node CloneNode(bool deep = false)
		{
			Element clone = new Element(NodeName);
			clone.SourceLocation = SourceLocation;

			if (_attributes != null)
			{
				AttributeDictionary cloneAttributes = clone.Attributes;
				foreach (KeyValuePair<string, string> pair in _attributes)
					cloneAttributes.Add(pair.Key, pair.Value);
			}

			if (deep)
				CloneDescendantsTo(clone);
			return clone;
		}

		protected internal virtual void OnAttrChange(string? name, string? newValue, string? oldValue)
		{
			if (name == "id")
			{
#if DEBUG
				if (!string.IsNullOrEmpty(newValue) && !char.IsLetterOrDigit(newValue[0]) && newValue[0] != '_')
					Debug.WriteLine($"Note: ID '{newValue}' starts with punctuation, which is often a mistake. This may be a bug in your code.");
#endif

				IElementLookupContainer? fastLookupContainer = Root as IElementLookupContainer;

				fastLookupContainer?.RemoveDescendant(this);
				_id = newValue ?? string.Empty;
				fastLookupContainer?.AddDescendant(this);

				InvalidateComputedStyle();
			}
			else if (name == "class")
			{
#if DEBUG
				if (!string.IsNullOrEmpty(newValue) && !char.IsLetterOrDigit(newValue[0]) && newValue[0] != '_')
					Debug.WriteLine($"Note: Classname '{newValue}' starts with punctuation, which is often a mistake. This may be a bug in your code.");
#endif

				IElementLookupContainer? fastLookupContainer = Root as IElementLookupContainer;

				fastLookupContainer?.RemoveDescendant(this);

				HashSet<string> newClassNames = new HashSet<string>(SplitClassname(newValue));
				HashSet<string> diff = new HashSet<string>(newClassNames);
				diff.SymmetricExceptWith(_classNames);

				_classNames = newClassNames;

				fastLookupContainer?.AddDescendant(this);

				if (Root is IStyleRoot styleRoot)
				{
					IStyleManager styleManager = styleRoot.StyleManager;
					foreach (string changedClassname in diff)
					{
						if (styleManager.ClassnamesUsedByStyles.ContainsKey(changedClassname))
						{
							InvalidateComputedStyle();
							break;
						}
					}
				}
			}
			else if (name == "style")
			{
				_inlineStyles = null;
				InvalidateComputedStyle();
			}
			else
			{
				if (Root is IStyleRoot styleRoot)
				{
					IStyleManager styleManager = styleRoot.StyleManager;
					if (styleManager.AttributesUsedByStyles.ContainsKey(name ?? string.Empty))
						InvalidateComputedStyle();
				}
			}
		}

		private StylePropertySet ParseInlineStyle()
		{
			_inlineStyleParser.Messages.Clear();

			string inlineStyle = Attributes["style"];
			if (string.IsNullOrEmpty(inlineStyle))
				return StylePropertySet.Empty;

			CssLexer lexer = SourceLocation != null
				? new CssLexer(inlineStyle, SourceLocation.Filename, SourceLocation.Line, SourceLocation.Column, _inlineStyleParser.Messages)
				: new CssLexer(inlineStyle, "<inline style>", _inlineStyleParser.Messages);

			List<StyleProperty> properties = new List<StyleProperty>();
			retry:
			_inlineStyleParser.ParsePropertyDeclarations(lexer, properties);
			if (lexer.Next().Kind != CssTokenKind.Eoi)  // Recover from floating garbage.
				goto retry;

			return new StylePropertySet(properties);
		}

		public override void ToString(StringBuilder stringBuilder)
		{
			AppendStartTag(stringBuilder);

			foreach (Node child in Children)
			{
				child.ToString(stringBuilder);
			}

			if (!AutoClosingTags.Contains(NodeName))
				stringBuilder.Append($"</{NodeName}>");
		}

		public virtual bool HasPseudoClass(string name, string? value)
			=> false;

		public virtual bool HasPseudoElement(string name, string? value)
			=> false;

		#region Style stuff

		private ComputedStyle? _computedStyle;

		/// <summary>
		/// Mark this element as no longer having a valid computed style and needing to
		/// be recomputed.  The next invocation of Document.ValidateComputedStyles() or
		/// of GetComputedStyle() will re-validate this style.
		/// </summary>
		public void InvalidateComputedStyle()
		{
			_computedStyle = null;

			if (Root is IStyleRoot styleRoot)
				styleRoot.StyleQueue.Enqueue(this);
		}

		/// <summary>
		/// Retrieve the current computed style for this element, or recompute it if it
		/// is invalid.  This may implicitly recompute its ancestors' computed styles
		/// as well if they are invalid, and recomputing *those* may also invalidate sibling
		/// or cousin elements' styles (but as long as the tree doesn't change attributes,
		/// the entire thing *will* settle down to a steady-state).
		/// </summary>
		/// <returns>The current computed style for this element.</returns>
		public ComputedStyle GetComputedStyle()
		{
			if (_computedStyle != null)
				return _computedStyle;

			if (Root is not IStyleRoot styleRoot)
				return _computedStyle = ComputedStyle.Default;

			ComputedStyle? parentStyle = Parent is Element parentElement
				? parentElement.GetComputedStyle().MakeChildStyle()
				: null;

			_computedStyle = styleRoot.StyleManager.ComputeStyle(this, parentStyle);

			styleRoot.StyleQueue.Remove(this);

			return _computedStyle;
		}

		#endregion

		private void AppendStartTag(StringBuilder stringBuilder)
		{
			stringBuilder.Append('<');

			stringBuilder.Append(NodeName);

			if (_attributes != null)
			{
				foreach (KeyValuePair<string, string> pair in _attributes)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(pair.Value);
				}
			}

			if (AutoClosingTags.Contains(NodeName))
				stringBuilder.Append(" /");

			stringBuilder.Append('>');
		}
	}
}
