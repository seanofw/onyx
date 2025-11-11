using System.Data;
using System.Diagnostics;
using System.Text;
using Onyx.Css;
using Onyx.Css.Computed;
using Onyx.Css.Parsing;
using Onyx.Css.Properties;

namespace Onyx.Html.Dom
{
	public class Element : ContainerNode, IAttributeNode
	{
		public override NodeType NodeType => NodeType.Element;

		private NamedNodeMap? _attributes;
		internal Dictionary<string, Attribute>? AttributesDict;

		public string Id
		{
			get => _id;
			set
			{
				if (_id != value)
				{
					NamedNodeMap attributes = Attributes;
					attributes["id"] = new Attribute(this, "id", value);
				}
			}
		}
		private string _id = string.Empty;

		public string ClassName
		{
			get => _className;
			set
			{
				if (_className != value)
				{
					NamedNodeMap attributes = Attributes;
					attributes["class"] = new Attribute(this, "class", value);
				}
			}
		}
		private string _className = string.Empty;

		public IReadOnlySet<string> ClassNames => _classNames ??= _className
			.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
			.ToHashSet();
		private HashSet<string>? _classNames;

		public StylePropertySet InlineStyles => _inlineStyles ??= ParseInlineStyle();
		private StylePropertySet? _inlineStyles;

		public void AddClass(string className)
		{
			IReadOnlySet<string> _ = ClassNames;    // Force the classnames to be expanded.

			if (_classNames!.Add(className))
				UpdateClassnameFromSet();
		}

		public void RemoveClass(string className)
		{
			IReadOnlySet<string> _ = ClassNames;    // Force the classnames to be expanded.

			if (_classNames!.Remove(className))
				UpdateClassnameFromSet();
		}

		public void ToggleClass(string className)
		{
			IReadOnlySet<string> _ = ClassNames;    // Force the classnames to be expanded.

			if (!_classNames!.Add(className))
				_classNames!.Remove(className);

			UpdateClassnameFromSet();
		}

		private void UpdateClassnameFromSet()
		{
			string className = string.Join(" ", ClassNames.OrderBy(c => c.ToLowerInvariant()));

			NamedNodeMap attributes = Attributes;
			attributes["class"] = new Attribute(this, "class", className);
		}

		public override string NodeName { get; }

		public override string TextContent
		{
			get => string.Join("", ChildNodes.Select(c => c.TextContent));

			set
			{
				Clear();
				AppendChild(new TextNode(value));
			}
		}

		public NamedNodeMap Attributes => _attributes ??= new NamedNodeMap(this);

		public string StartTag
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				AppendStartTag(stringBuilder);
				return stringBuilder.ToString();
			}
		}

		private void AppendStartTag(StringBuilder stringBuilder)
		{
			stringBuilder.Append('<');

			stringBuilder.Append(NodeName);

			if (_attributes != null)
			{
				foreach (KeyValuePair<string, Attribute> pair in _attributes)
				{
					stringBuilder.Append(' ');
					pair.Value.ToString(stringBuilder);
				}
			}

			if (AutoClosingTags.Contains(NodeName))
				stringBuilder.Append(" /");

			stringBuilder.Append('>');
		}

		public string EndTag
			=> AutoClosingTags.Contains(NodeName) ? string.Empty : $"</{NodeName}>";

		internal static IReadOnlySet<string> AutoClosingTags { get; } = new HashSet<string>
		{
			"meta", "link", "img", "input", "br", "hr",
		};

		internal static IReadOnlySet<string> RawContentTags { get; } = new HashSet<string>
		{
			"script", "style", "xmp", "plaintext",
		};

		internal static IReadOnlySet<string> BlockLevelElements { get; } = new HashSet<string>
		{
			"address", "article", "aside", "blockquote", "details",
			"dialog", "div", "dl", "fieldset", "figcaption", "figure",
			"footer", "form", "h1", "h2", "h3", "h4", "h5", "h6",
			"header", "hgroup", "hr", "main", "nav", "ol", "p",
			"pre", "section", "table", "ul", "row", "column",
		};

		public Element(string name)
		{
			NodeName = name;
		}

		public override Node CloneNode(bool deep = false)
		{
			Element clone = new Element(NodeName);
			clone.SourceLocation = SourceLocation;

			if (_attributes != null)
			{
				NamedNodeMap cloneAttributes = clone.Attributes;
				foreach (KeyValuePair<string, Attribute> pair in _attributes)
				{
					Attribute cloneAttribute = new Attribute(clone, pair.Key, pair.Value.Value);
					cloneAttributes.Add(new KeyValuePair<string, Attribute>(pair.Key, cloneAttribute));
				}
			}

			if (deep)
				CloneDescendantsTo(clone);
			return clone;
		}

		internal virtual void OnAttrChange(string? name, Attribute? attr, string? oldValue)
		{
			if (name == "id")
			{
#if DEBUG
				if (attr != null && !string.IsNullOrEmpty(attr.Value) && !char.IsLetterOrDigit(attr.Value[0]) && attr.Value[0] != '_')
					Debug.WriteLine($"Note: ID '{attr.Value}' starts with punctuation, which is often a mistake. This may be a bug in your code.");
#endif

				IElementLookupContainer? fastLookupContainer = Root as IElementLookupContainer;

				fastLookupContainer?.RemoveDescendant(this);
				_id = attr?.Value ?? string.Empty;
				fastLookupContainer?.AddDescendant(this);

				InvalidateComputedStyle();
			}
			else if (name == "class")
			{
#if DEBUG
				if (attr != null && !string.IsNullOrEmpty(attr.Value) && !char.IsLetterOrDigit(attr.Value[0]) && attr.Value[0] != '_')
					Debug.WriteLine($"Note: Classname '{attr.Value}' starts with punctuation, which is often a mistake. This may be a bug in your code.");
#endif

				IElementLookupContainer? fastLookupContainer = Root as IElementLookupContainer;

				fastLookupContainer?.RemoveDescendant(this);

				_className = attr?.Value ?? string.Empty;
				_classNames = _className
					.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
					.ToHashSet();

				HashSet<string> diff = oldValue?
					.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
					.ToHashSet() ?? new HashSet<string>();
				diff.SymmetricExceptWith(_classNames);

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

		private static readonly CssParser _inlineStyleParser = new CssParser();

		private StylePropertySet ParseInlineStyle()
		{
			_inlineStyleParser.Messages.Clear();

			string? inlineStyle = AttributesDict?.GetValueOrDefault("style")?.Value;
			if (string.IsNullOrEmpty(inlineStyle))
				return StylePropertySet.Empty;

			CssLexer lexer = SourceLocation != null
				? new CssLexer(inlineStyle, SourceLocation.Filename, SourceLocation.Line, SourceLocation.Column, _inlineStyleParser.Messages)
				: new CssLexer(inlineStyle, "<inline style>", _inlineStyleParser.Messages);

			List<StyleProperty> properties = new List<StyleProperty>();
		retry:
			_inlineStyleParser.ParsePropertyDeclarations(lexer, properties);
			if (lexer.Next().Kind != CssTokenKind.Eoi)	// Recover from floating garbage.
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
	}
}
