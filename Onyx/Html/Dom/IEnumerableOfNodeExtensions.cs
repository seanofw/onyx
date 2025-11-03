using Onyx.Css.Selectors;

namespace Onyx.Html.Dom
{
	public static class IEnumerableOfNodeExtensions
	{
		/// <summary>
		/// Find all descendant elements of the given set of nodes that match the given selector.
		/// </summary>
		public static IReadOnlySet<Element> Find(this IEnumerable<Node> subtree, string selector)
		{
			HashSet<Element> result = new HashSet<Element>();

			CompoundSelector? compoundSelector = null;
			foreach (Node node in subtree)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				compoundSelector.Find(node, result);
			}

			return result;
		}

		/// <summary>
		/// Find all descendant elements of the given set of nodes that match the given selector.
		/// </summary>
		public static IReadOnlySet<Element> Find(this IEnumerable<Node> subtree, Selector selector)
		{
			HashSet<Element> result = new HashSet<Element>();

			foreach (Node node in subtree)
			{
				selector.Find(node, result);
			}

			return result;
		}

		/// <summary>
		/// Find all descendant elements of the given set of nodes that match the given selector.
		/// </summary>
		public static IEnumerable<Element> Find(this IEnumerable<Node> subtree, CompoundSelector selector)
		{
			HashSet<Element> result = new HashSet<Element>();

			foreach (Node node in subtree)
			{
				selector.Find(node, result);
			}

			return result;
		}

		/// <summary>
		/// Find the closest ancestor element for each of the given nodes that matches the given selector.
		/// </summary>
		public static IReadOnlySet<Element> Closest(this IEnumerable<Node> nodes, string selector)
		{
			HashSet<Element> result = new HashSet<Element>();

			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				Element? ancestor = compoundSelector.Closest(node);
				if (ancestor != null)
					result.Add(ancestor);
			}

			return result;
		}

		/// <summary>
		/// Filter the set of nodes to just those elements that match the given selector.
		/// </summary>
		public static IEnumerable<Element> Where(this IEnumerable<Node> nodes, string selector)
		{
			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				if (compoundSelector.IsMatch(node))
					yield return (Element)node;
			}
		}

		/// <summary>
		/// Filter the set of nodes to those that do *not* match the given selector.
		/// </summary>
		public static IEnumerable<Node> Except(this IEnumerable<Node> nodes, string selector)
		{
			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				if (!compoundSelector.IsMatch(node))
					yield return node;
			}
		}

		/// <summary>
		/// Return true if any of the nodes match the given selector.
		/// </summary>
		public static bool Any(this IEnumerable<Node> nodes, string selector)
		{
			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				if (compoundSelector.IsMatch(node))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Return true if all of the nodes match the given selector.
		/// </summary>
		public static bool All(this IEnumerable<Node> nodes, string selector)
		{
			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				if (!compoundSelector.IsMatch(node))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Rearrange a collection of nodes into document order.
		/// </summary>
		/// <param name="nodes">A collection of nodes.</param>
		/// <returns>The same nodes, in document order.  If the nodes belong to different
		/// documents, their ordering will be undefined.</returns>
		public static IEnumerable<T> OrderByPosition<T>(this IEnumerable<T> nodes)
			where T : Node
		{
			if (nodes is IReadOnlyCollection<T> collection
				&& collection.Count <= 1)
				return collection;

			List<T> hardenedNodes = nodes.ToList();
			AncestorList aAncestors = new AncestorList();
			AncestorList bAncestors = new AncestorList();
			hardenedNodes.Sort((a, b) => Node.ComparePositionInternal(a, b, aAncestors, bAncestors));
			return hardenedNodes;
		}

		/// <summary>
		/// Rearrange a collection of nodes into reverse document order.
		/// </summary>
		/// <param name="nodes">A collection of nodes.</param>
		/// <returns>The same nodes, in document order.  If the nodes belong to different
		/// documents, their ordering will be undefined.</returns>
		public static IEnumerable<T> OrderByPositionDescending<T>(this IEnumerable<T> nodes)
			where T : Node
		{
			if (nodes is IReadOnlyCollection<T> collection
				&& collection.Count <= 1)
				return collection;

			List<T> hardenedNodes = nodes.ToList();
			AncestorList aAncestors = new AncestorList();
			AncestorList bAncestors = new AncestorList();
			hardenedNodes.Sort((a, b) => -Node.ComparePositionInternal(a, b, aAncestors, bAncestors));
			return hardenedNodes;
		}

		/// <summary>
		/// Project the nodes to their child elements.
		/// </summary>
		public static IEnumerable<Element> Children(this IEnumerable<Node> nodes)
		{
			foreach (Node node in nodes)
			{
				if (node.ChildNodes.Count > 0)
				{
					foreach (Node child in node.ChildNodes)
					{
						if (child is Element childElement)
							yield return childElement;
					}
				}
			}
		}

		/// <summary>
		/// Project the nodes to their child elements, filtered by the given selector.
		/// </summary>
		public static IEnumerable<Element> Children(this IEnumerable<Node> nodes, string selector)
		{
			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				if (node.ChildNodes.Count > 0)
				{
					foreach (Node child in node.ChildNodes)
					{
						compoundSelector ??= node.GetCompoundSelector(selector);
						if (child is Element childElement && compoundSelector.IsMatch(child))
							yield return childElement;
					}
				}
			}
		}

		/// <summary>
		/// Project the nodes to their descendant elements.
		/// </summary>
		public static IEnumerable<Element> Descendants(this IEnumerable<Node> nodes)
		{
			foreach (Node node in nodes)
			{
				foreach (Node descendant in node.Descendants())
				{
					if (descendant is Element descendantElement)
						yield return descendantElement;
				}
			}
		}

		/// <summary>
		/// Project the nodes to their descendant elements, filtered by the given selector.
		/// </summary>
		public static IEnumerable<Element> Descendants(this IEnumerable<Node> nodes, string selector)
		{
			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				foreach (Node descendant in node.Descendants())
				{
					compoundSelector ??= node.GetCompoundSelector(selector);
					if (descendant is Element descendantElement && compoundSelector.IsMatch(descendant))
						yield return descendantElement;
				}
			}
		}

		/// <summary>
		/// Project the nodes to their parent elements.
		/// </summary>
		public static IReadOnlySet<Element> Parents(this IEnumerable<Node> nodes)
		{
			HashSet<Element> set = new HashSet<Element>();

			foreach (Node node in nodes)
			{
				if (node.Parent is Element parentElement)
					set.Add(parentElement);
			}

			return set;
		}

		/// <summary>
		/// Project the nodes to their parent elements, filtered by the given selector.
		/// </summary>
		public static IReadOnlySet<Element> Parents(this IEnumerable<Node> nodes, string selector)
		{
			HashSet<Element> set = new HashSet<Element>();

			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				if (node.Parent is Element parentElement
					&& compoundSelector.IsMatch(parentElement))
					set.Add(parentElement);
			}

			return set;
		}

		/// <summary>
		/// Project the nodes to their ancestor elements.
		/// </summary>
		public static IReadOnlySet<Element> Ancestors(this IEnumerable<Node> nodes)
		{
			HashSet<Element> set = new HashSet<Element>();

			foreach (Node node in nodes)
			{
				foreach (Node ancestor in node.Ancestors())
				{
					if (ancestor is Element ancestorElement)
						set.Add(ancestorElement);
				}
			}

			return set;
		}

		/// <summary>
		/// Project the nodes to their ancestor elements, filtered by the given selector.
		/// </summary>
		public static IReadOnlySet<Element> Ancestors(this IEnumerable<Node> nodes, string selector)
		{
			HashSet<Element> set = new HashSet<Element>();

			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				compoundSelector ??= node.GetCompoundSelector(selector);
				foreach (Node ancestor in node.Ancestors())
				{
					if (ancestor is Element ancestorElement
						&& compoundSelector.IsMatch(ancestor))
						set.Add(ancestorElement);
				}
			}

			return set;
		}

		/// <summary>
		/// Filter the set of nodes to elements with the given classname.
		/// </summary>
		public static IEnumerable<Element> HasClass(this IEnumerable<Node> nodes, string className)
			=> nodes.OfType<Element>().Where(e => e.ClassNames.Contains(className));

		public static void AddClass(this IEnumerable<Node> nodes, string className)
		{
			foreach (Node node in nodes)
			{
				if (!(node is Element element))
					continue;
				element.AddClass(className);
			}
		}

		public static void RemoveClass(this IEnumerable<Node> nodes, string className)
		{
			foreach (Node node in nodes)
			{
				if (!(node is Element element))
					continue;
				element.RemoveClass(className);
			}
		}

		public static void ToggleClass(this IEnumerable<Node> nodes, string className)
		{
			foreach (Node node in nodes)
			{
				if (!(node is Element element))
					continue;
				element.ToggleClass(className);
			}
		}
	}
}
