using System.Runtime.CompilerServices;
using Onyx.Css.Selectors;

namespace Onyx.Html.Dom
{
	internal class ElementLookupTables
	{
		private readonly Dictionary<string, HashSet<Element>> _elementsById = new Dictionary<string, HashSet<Element>>();
		private readonly Dictionary<string, HashSet<Element>> _elementsByName = new Dictionary<string, HashSet<Element>>();
		private readonly Dictionary<string, HashSet<Element>> _elementsByClassName = new Dictionary<string, HashSet<Element>>();
		private readonly Dictionary<string, HashSet<Element>> _elementsByElementType = new Dictionary<string, HashSet<Element>>();
		private readonly Dictionary<string, HashSet<Element>> _elementsByTypeAttribute = new Dictionary<string, HashSet<Element>>();

		private readonly Stack<HashSet<Element>> _unusedSets = new Stack<HashSet<Element>>();

		public Cache<string, CompoundSelector?> ParsedSelectors = new Cache<string, CompoundSelector?>(1024);

		public Dictionary<SimpleSelector, SimpleSelectorQueryPlanSet> SimpleSelectorQueryPlans { get; }
			= new Dictionary<SimpleSelector, SimpleSelectorQueryPlanSet>();

		public Dictionary<Selector, SelectorQueryPlan> SelectorQueryPlans { get; }
			= new Dictionary<Selector, SelectorQueryPlan>();

		public void AddElement(Element element)
		{
			AddToSet(element, element.NodeName, _elementsByElementType);

			if (element is IAttributeNode attributeNode)
			{
				if (!string.IsNullOrEmpty(attributeNode.Id))
					AddToSet(element, attributeNode.Id, _elementsById);

				if (attributeNode.Attributes.TryGetValue("name", out string? value)
					&& !string.IsNullOrEmpty(value))
					AddToSet(element, value, _elementsByName);

				if (attributeNode.Attributes.TryGetValue("type", out value)
					&& !string.IsNullOrEmpty(value))
					AddToSet(element, value, _elementsByTypeAttribute);

				foreach (string className in attributeNode.ClassNames)
					AddToSet(element, className, _elementsByClassName);
			}
		}

		private void AddToSet(Element node, string key, Dictionary<string, HashSet<Element>> set)
		{
			if (!set.TryGetValue(key, out HashSet<Element>? elements))
			{
				if (_unusedSets.Count != 0)
					elements = _unusedSets.Pop();
				else
					elements = new HashSet<Element>();

				set[key] = elements;
			}
			elements.Add(node);
		}

		internal void RemoveElement(Element element)
		{
			RemoveFromSet(element, element.NodeName, _elementsByElementType);

			if (element is IAttributeNode attributeNode)
			{
				if (!string.IsNullOrEmpty(attributeNode.Id))
					RemoveFromSet(element, attributeNode.Id, _elementsById);

				if (attributeNode.Attributes.TryGetValue("name", out string? value)
					&& !string.IsNullOrEmpty(value))
					RemoveFromSet(element, value, _elementsByName);

				if (attributeNode.Attributes.TryGetValue("type", out value)
					&& !string.IsNullOrEmpty(value))
					RemoveFromSet(element, value, _elementsByTypeAttribute);

				foreach (string className in attributeNode.ClassNames)
					RemoveFromSet(element, className, _elementsByClassName);
			}
		}

		private void RemoveFromSet(Element element, string key, Dictionary<string, HashSet<Element>> set)
		{
			if (set.TryGetValue(key, out HashSet<Element>? elements))
			{
				elements.Remove(element);

				if (elements.Count == 0)
				{
					set.Remove(key);

					if (_unusedSets.Count < 64)
						_unusedSets.Push(elements);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal IReadOnlyCollection<Element> GetElementsById(string id)
			=> _elementsById.TryGetValue(id,
				out HashSet<Element>? elements) ? elements : Array.Empty<Element>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal IReadOnlyCollection<Element> GetElementsByName(string name)
			=> _elementsById.TryGetValue(name,
				out HashSet<Element>? elements) ? elements : Array.Empty<Element>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal IReadOnlyCollection<Element> GetElementsByClassname(string classname)
			=> _elementsByClassName.TryGetValue(classname,
				out HashSet<Element>? elements) ? elements : Array.Empty<Element>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal IReadOnlyCollection<Element> GetElementsByElementType(string type)
			=> _elementsByElementType.TryGetValue(type.ToLowerInvariant(),
				out HashSet<Element>? elements) ? elements : Array.Empty<Element>();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal IReadOnlyCollection<Element> GetElementsByTypeAttribute(string type)
			=> _elementsByTypeAttribute.TryGetValue(type.ToLowerInvariant(),
				out HashSet<Element>? elements) ? elements : Array.Empty<Element>();
	}
}
