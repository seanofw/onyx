using Onyx.Css.Computed;
using Onyx.Css.Properties;
using Onyx.Css.Selectors;
using Onyx.Extensions;
using Onyx.Html.Dom;

namespace Onyx.Css
{
	public class StyleManager : IStyleManager
	{
		#region Properties and fields

		/// <summary>
		/// All of the stylesheets added to this StyleManager.
		/// </summary>
		public IReadOnlyList<Stylesheet> Stylesheets => _stylesheets;
		private readonly List<Stylesheet> _stylesheets = new List<Stylesheet>();

		/// <summary>
		/// An index of StyleRules, where each is keyed by the element name(s) used in their last simple selector.
		/// </summary>
		private readonly Dictionary<string, List<StyleRule>> _elementNameIndex
			= new Dictionary<string, List<StyleRule>>();

		/// <summary>
		/// An index of StyleRules, where each is keyed by the class name(s) used in their last simple selector.
		/// </summary>
		private readonly Dictionary<string, List<StyleRule>> _classNameIndex
			= new Dictionary<string, List<StyleRule>>();

		/// <summary>
		/// An index of StyleRules, where each is keyed by the ID(s) used in their last simple selector.
		/// </summary>
		private readonly Dictionary<string, List<StyleRule>> _idIndex
			= new Dictionary<string, List<StyleRule>>();
		private readonly List<StyleRule> _genericIndex = new List<StyleRule>();

		/// <summary>
		/// All attribute names that have been used by at least one selector.
		/// </summary>
		public IReadOnlyDictionary<string, int> AttributesUsedByStyles => _attributesUsedByStyles;
		private readonly Dictionary<string, int> _attributesUsedByStyles = new Dictionary<string, int>();

		/// <summary>
		/// All class names that have been used by at least one selector.
		/// </summary>
		public IReadOnlyDictionary<string, int> ClassnamesUsedByStyles => _classnamesUsedByStyles;
		private readonly Dictionary<string, int> _classnamesUsedByStyles = new Dictionary<string, int>();

		/// <summary>
		/// This event is raised whenever the stylesheet collection changes.
		/// </summary>
		public event EventHandler? StylesheetsChanged;

		#endregion

		#region Public API

		/// <summary>
		/// Parse and add a new stylesheet.  This is just a convenience method around
		/// parsing it and then adding it.
		/// </summary>
		/// <param name="text">The text of the stylesheet to add.</param>
		/// <param name="filename">The filename (for error reporting).</param>
		/// <returns>The stylesheet that was added to the StyleManager.</returns>
		public Stylesheet AddStylesheet(string text, string filename)
		{
			Parsing.CssLexer lexer = new Parsing.CssLexer(text, filename);
			Parsing.CssParser parser = new Parsing.CssParser();
			Stylesheet stylesheet = parser.Parse(lexer);

			AddStylesheet(stylesheet);

			return stylesheet;
		}

		/// <summary>
		/// Add a new stylesheet.
		/// </summary>
		/// <param name="stylesheet">The stylesheet to add to the collection
		/// of managed style rules.  This must not already have been added, or an
		/// exception will be thrown.</param>
		public void AddStylesheet(Stylesheet stylesheet)
		{
			if (stylesheet == null)
				throw new ArgumentNullException(nameof(stylesheet));
			if (_stylesheets.Contains(stylesheet))
				throw new ArgumentException("Cannot add duplicate stylesheet.");

			_stylesheets.Add(stylesheet);

			foreach (StyleRule rule in stylesheet.Rules)
			{
				foreach (Selector selector in rule.Selector.Selectors)
				{
					AddLookupsForSelector(selector, rule);
				}
			}

			StylesheetsChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Remove a stylesheet from the collection.
		/// </summary>
		/// <param name="stylesheet">The stylesheet to remove.</param>
		/// <returns>True if the stylesheet existed (and was therefore removed), false
		/// if it did not already exist.</returns>
		public bool RemoveStylesheet(Stylesheet stylesheet)
		{
			if (stylesheet == null)
				throw new ArgumentNullException(nameof(stylesheet));

			if (!_stylesheets.Remove(stylesheet))
				return false;

			foreach (StyleRule rule in stylesheet.Rules)
			{
				foreach (Selector selector in rule.Selector.Selectors)
				{
					RemoveLookupsForSelector(selector, rule);
				}
			}

			StylesheetsChanged?.Invoke(this, EventArgs.Empty);

			return true;
		}

		/// <summary>
		/// Look up styles for the given element.  
		/// </summary>
		/// <param name="element">The element to locate styles for.</param>
		/// <returns>*All* of the StylePropertySets that should be applied to
		/// this element.  Note that they are not ordered by specificity, and those
		/// that are overridden are still included, and shorthand properties are not
		/// expanded:  The returned collection is the "raw" style collection, with
		/// the correct specificity to apply for each property set.</returns>
		public IReadOnlyCollection<StylePropertyBag> GetStyleRules(Element element)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			HashSet<StyleRule> candidates = FindCandidateRules(element);

			List<StylePropertyBag> result = new List<StylePropertyBag>();

			foreach (StyleRule rule in candidates)
			{
				Specificity specificity = Specificity.Zero;

				foreach (Selector selector in rule.Selector.Selectors)
				{
					if (selector.IsMatch(element)
						&& selector.Specificity > specificity)
					{
						specificity = selector.Specificity;
					}
				}

				if (specificity != Specificity.Zero)
				{
					result.Add(new StylePropertyBag(rule.Properties, specificity));
				}
			}

			return result;
		}

		/// <summary>
		/// Find a superset of all rules that could conceivably apply to this element.
		/// We try to make this as narrow as possible, but the goal is to produce a
		/// quick superset before then actually IsMatch()'ing each rule's selector(s)
		/// the hard way.
		/// </summary>
		/// <param name="element">The element to locate styles for.</param>
		/// <returns>A superset of all style rules that could conceivably apply to
		/// this element.</returns>
		private HashSet<StyleRule> FindCandidateRules(Element element)
		{
			HashSet<StyleRule> candidates = new HashSet<StyleRule>(_genericIndex);

			if (_idIndex.TryGetValue(element.Id, out List<StyleRule>? rules))
				candidates.UnionWith(rules);

			foreach (string className in element.ClassNames)
			{
				if (_classNameIndex.TryGetValue(className, out rules))
					candidates.UnionWith(rules);
			}

			return candidates;
		}

		/// <summary>
		/// Compute the style for the given element, inheriting it (as necessary or as requested)
		/// from the given parent style.
		/// </summary>
		/// <param name="element">The element whose style is to be computed.</param>
		/// <param name="parentStyle">The parent style to inherit from.</param>
		/// <returns>The element's finished computed style.</returns>
		public ComputedStyle ComputeStyle(Element element, ComputedStyle? parentStyle = null)
		{
			if (element == null)
				throw new ArgumentNullException(nameof(element));

			IReadOnlyCollection<StylePropertyBag> styleRules = GetStyleRules(element);

			IReadOnlyCollection<StyleProperty> finalProperties = ExtractMostSpecificStyles(styleRules);

			ComputedStyle computedStyle = parentStyle ?? ComputedStyle.Default;
			foreach (StyleProperty styleProperty in finalProperties)
			{
				if (!styleProperty.HasSpecialApplication)
				{
					// Normal path:  Apply the property.
					computedStyle = styleProperty.Apply(computedStyle);
				}
				else
				{
					// Weirder paths:  Copy the property value from another source.
					if (styleProperty.Initial)
						computedStyle = styleProperty.CopyProperty(computedStyle, ComputedStyle.Default);
					else if (styleProperty.Inherit)
						computedStyle = styleProperty.CopyProperty(computedStyle, parentStyle ?? ComputedStyle.Default);
					else
						{ /* Nothing to do for unset but ignore property application */ }
				}
			}

			return computedStyle;
		}

		/// <summary>
		/// Given bags of style rules, each with their own specificity, reduce them down to *just*
		/// the most specific property that should be applied for each kind of property.
		/// </summary>
		/// <param name="stylePropertyBags">Bags of style properties, with their specificity determined.</param>
		/// <returns>The most specific property of each kind from the given bag(s) of properties.
		/// Each property kind will be represented exactly one.</returns>
		private static IReadOnlyCollection<StyleProperty> ExtractMostSpecificStyles(
			IEnumerable<StylePropertyBag> stylePropertyBags)
		{
			Dictionary<KnownPropertyKind, StylePropertyPair> finalProperties
				= new Dictionary<KnownPropertyKind, StylePropertyPair>();

			foreach (StylePropertyBag bag in stylePropertyBags)
			{
				Specificity specificity = bag.Specificity;
				StylePropertySet stylePropertySet = bag.StylePropertySet;

				foreach (StyleProperty property in stylePropertySet)
				{
					foreach (StyleProperty childProperty in property.Decompose())
					{
						if (!finalProperties.TryGetValue(childProperty.Kind, out var oldProperty)
							|| specificity > oldProperty.Specificity
							|| oldProperty.Specificity == specificity && childProperty.Important)
							finalProperties[childProperty.Kind] = new StylePropertyPair(childProperty, specificity);
					}
				}
			}

			return finalProperties.Select(p => p.Value.StyleProperty).ToArray();
		}

		/// <summary>
		/// A simple tuple pairing a single StyleProperty with its Specificity.
		/// </summary>
		private readonly struct StylePropertyPair
		{
			public StyleProperty StyleProperty { get; }
			public Specificity Specificity { get; }

			public StylePropertyPair(StyleProperty styleProperty, Specificity specificity)
			{
				StyleProperty = styleProperty;
				Specificity = specificity;
			}
		}

		#endregion

		#region Internal lookup-tracking mechanics for referenced classnames and attributes

		private void AddLookupsForSelector(Selector selector, StyleRule rule)
		{
			foreach (SelectorComponent component in selector.Path)
			{
				AddLookupsForSimpleSelector(component.SimpleSelector);
			}

			if (selector.Path.Count > 0)
				AddLookupsForLastSimpleSelector(selector.Path[^1].SimpleSelector, rule);
		}

		private void RemoveLookupsForSelector(Selector selector, StyleRule rule)
		{
			foreach (SelectorComponent component in selector.Path)
			{
				RemoveLookupsForSimpleSelector(component.SimpleSelector);
			}

			if (selector.Path.Count > 0)
				RemoveLookupsForLastSimpleSelector(selector.Path[^1].SimpleSelector, rule);
		}

		private void AddLookupsForSimpleSelector(SimpleSelector simpleSelector)
		{
			foreach (SelectorFilter filter in simpleSelector.Filters)
			{
				AddLookupsForSelectorFilter(filter);
			}
		}

		private void RemoveLookupsForSimpleSelector(SimpleSelector simpleSelector)
		{
			foreach (SelectorFilter filter in simpleSelector.Filters)
			{
				RemoveLookupsForSelectorFilter(filter);
			}
		}

		private void AddLookupsForSelectorFilter(SelectorFilter filter)
		{
			if (filter is SelectorFilterClass filterClass)
				IncrementDictionaryReference(_classnamesUsedByStyles, filterClass.Class);
			else if (filter is SelectorFilterHasAttrib filterHasAttrib)
				IncrementDictionaryReference(_attributesUsedByStyles, filterHasAttrib.Name);
			else if (filter is SelectorFilterAttrib filterAttrib)
				IncrementDictionaryReference(_attributesUsedByStyles, filterAttrib.Name);
		}

		private void RemoveLookupsForSelectorFilter(SelectorFilter filter)
		{
			if (filter is SelectorFilterClass filterClass)
				DecrementDictionaryReference(_classnamesUsedByStyles, filterClass.Class);
			else if (filter is SelectorFilterHasAttrib filterHasAttrib)
				DecrementDictionaryReference(_attributesUsedByStyles, filterHasAttrib.Name);
			else if (filter is SelectorFilterAttrib filterAttrib)
				DecrementDictionaryReference(_attributesUsedByStyles, filterAttrib.Name);
		}

		private static void IncrementDictionaryReference<K>(Dictionary<K, int> dict, K key)
			where K : notnull
		{
			if (!dict.TryGetValue(key, out int count))
				count = 0;
			dict[key] = count + 1;
		}

		private static void DecrementDictionaryReference<K>(Dictionary<K, int> dict, K key)
			where K : notnull
		{
			if (!dict.TryGetValue(key, out int count))
				return;

			if (--count == 0)
				dict.Remove(key);
			else
				dict[key] = count;
		}

		#endregion

		#region Internal lookup-tracking mechanics for the last simple selector

		private void AddLookupsForLastSimpleSelector(SimpleSelector tail, StyleRule rule)
		{
			bool hasFastLookup = false;

			if (!string.IsNullOrEmpty(tail.ElementName) && tail.ElementName != "*")
			{
				_elementNameIndex.AddToDictionaryOfList(tail.ElementName, rule);
				hasFastLookup = true;
			}

			foreach (SelectorFilter filter in tail.Filters)
			{
				if (filter is SelectorFilterClass classFilter)
				{
					_classNameIndex.AddToDictionaryOfListWithUniq(classFilter.Class, rule);
					hasFastLookup = true;
				}
				else if (filter is SelectorFilterId idFilter)
				{
					_idIndex.AddToDictionaryOfListWithUniq(idFilter.Id, rule);
					hasFastLookup = true;
				}
			}

			if (!hasFastLookup)
				_genericIndex.Add(rule);
		}

		private void RemoveLookupsForLastSimpleSelector(SimpleSelector tail, StyleRule rule)
		{
			if (!string.IsNullOrEmpty(tail.ElementName) && tail.ElementName != "*")
				_elementNameIndex.RemoveFromDictionaryOfList(tail.ElementName, rule);

			foreach (SelectorFilter filter in tail.Filters)
			{
				if (filter is SelectorFilterClass classFilter)
					_classNameIndex.RemoveFromDictionaryOfList(classFilter.Class, rule);
				else if (filter is SelectorFilterId idFilter)
					_idIndex.RemoveFromDictionaryOfList(idFilter.Id, rule);
			}

			_genericIndex.Remove(rule);
		}

		#endregion
	}
}
