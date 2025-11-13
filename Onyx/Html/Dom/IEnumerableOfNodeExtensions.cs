using Onyx.Css.Selectors;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// Extension methods on IEnumerable{Node} and IEnumerable{Element} that can project,
	/// modify, filter, and order collections of nodes and elements.
	/// </summary>
	public static class IEnumerableOfNodeExtensions
	{
		/// <summary>
		/// Find all descendant elements of the given set of nodes that match the given selector.
		/// This will include the starting node also, if it matches the selector.
		/// </summary>
		/// <param name="nodes">A set of nodes to project.</param>
		/// <param name="selector">A selector that describes decsendants of each node to search for.</param>
		/// <returns>A distinct set of all descendant elements within the given subtrees that match
		/// the provided selector.  If the root of each subtree matches, it will also be included.</returns>
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
		/// This will include the starting node also, if it matches the selector.
		/// </summary>
		/// <param name="nodes">A set of nodes to project.</param>
		/// <param name="selector">A selector that describes decsendants of each node to search for.</param>
		/// <returns>A distinct set of all descendant elements within the given subtrees that match
		/// the provided selector.  If the root of each subtree matches, it will also be included.</returns>
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
		/// This will include the starting node also, if it matches the selector.
		/// </summary>
		/// <param name="nodes">A set of nodes to project.</param>
		/// <param name="selector">A selector that describes decsendants of each node to search for.</param>
		/// <returns>A distinct set of all descendant elements within the given subtrees that match
		/// the provided selector.  If the root of each subtree matches, it will also be included.</returns>
		public static IReadOnlySet<Element> Find(this IEnumerable<Node> subtree, CompoundSelector selector)
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
		/// <param name="nodes">A set of nodes to project.</param>
		/// <param name="selector">The selector that describes the closest ancestor of each node to search for.</param>
		/// <returns>A distinct set of all matching closest ancestors to the given node.  If a given
		/// node has no matching ancestor for the given selector, it will not have any entries in the
		/// result set.</returns>
		public static IReadOnlyCollection<Element> Closest(this IEnumerable<Node> nodes, string selector)
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
		/// <param name="nodes">A set of nodes to test.</param>
		/// <param name="selector">A selector to use to test those nodes.</param>
		/// <returns>All elements that match the given selector, enumerated lazily.</returns>
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
		/// <param name="nodes">A set of nodes to test.</param>
		/// <param name="selector">A selector to use to test those nodes.</param>
		/// <returns>All nodes that do *not* match the given selector, enumerated lazily.</returns>
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
		/// <param name="nodes">A set of nodes to test.</param>
		/// <param name="selector">A selector to use to test those nodes.</param>
		/// <returns>True if at least one node matches, false if none of the nodes match.
		/// When given an empty set of nodes, this will return false.</returns>
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
		/// <param name="nodes">A set of nodes to test.</param>
		/// <param name="selector">A selector to use to test those nodes.</param>
		/// <returns>True if all nodes match, false if at least one node does not match.
		/// When given an empty set of nodes, this will return true.</returns>
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
		/// <param name="nodes">A collection of nodes to order.</param>
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
		/// <param name="nodes">A collection of nodes to order.</param>
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
		/// <param name="nodes">The set of nodes to project to their children.</param>
		/// <returns>A distinct set of children of all of the nodes, where each child
		/// matches the given selector.</returns>
		public static IReadOnlyCollection<Element> Children(this IEnumerable<Node> nodes)
		{
			HashSet<Element> set = new HashSet<Element>();

			foreach (Node node in nodes)
			{
				if (node.ChildNodes.Count > 0)
				{
					foreach (Node child in node.ChildNodes)
					{
						if (child is Element childElement)
							set.Add(childElement);
					}
				}
			}

			return set;
		}

		/// <summary>
		/// Project the nodes to their child elements, filtered by the given selector.
		/// </summary>
		/// <param name="nodes">The set of nodes to project to their children.</param>
		/// <param name="selector">A selector to match children against; only the children
		/// that match this selector will be included in the result set.</param>
		/// <returns>A distinct set of children of all of the nodes, where each child
		/// matches the given selector.</returns>
		public static IReadOnlyCollection<Element> Children(this IEnumerable<Node> nodes, string selector)
		{
			HashSet<Element> set = new HashSet<Element>();

			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				if (node.ChildNodes.Count > 0)
				{
					foreach (Node child in node.ChildNodes)
					{
						compoundSelector ??= node.GetCompoundSelector(selector);
						if (child is Element childElement && compoundSelector.IsMatch(child))
							set.Add(childElement);
					}
				}
			}

			return set;
		}

		/// <summary>
		/// Project the nodes to their descendant elements.
		/// </summary>
		/// <param name="nodes">The set of nodes to project to their descendants.</param>
		/// <returns>A distinct set of descendants of all of the nodes.</returns>
		public static IReadOnlyCollection<Element> Descendants(this IEnumerable<Node> nodes)
		{
			HashSet<Element> set = new HashSet<Element>();

			foreach (Node node in nodes)
			{
				foreach (Node descendant in node.Descendants())
				{
					if (descendant is Element descendantElement)
						set.Add(descendantElement);
				}
			}

			return set;
		}

		/// <summary>
		/// Project the nodes to their descendant elements, filtered by the given selector.
		/// </summary>
		/// <param name="nodes">The set of nodes to project to their descendants.</param>
		/// <param name="selector">A selector to match descendants against; only the descendants
		/// that match this selector will be included in the result set.</param>
		/// <returns>A distinct set of descendants of all of the nodes, where each descendant
		/// matches the given selector.</returns>
		public static IReadOnlyCollection<Element> Descendants(this IEnumerable<Node> nodes, string selector)
		{
			HashSet<Element> set = new HashSet<Element>();

			CompoundSelector? compoundSelector = null;
			foreach (Node node in nodes)
			{
				foreach (Node descendant in node.Descendants())
				{
					compoundSelector ??= node.GetCompoundSelector(selector);
					if (descendant is Element descendantElement && compoundSelector.IsMatch(descendant))
						set.Add(descendantElement);
				}
			}

			return set;
		}

		/// <summary>
		/// Project the nodes to their parent elements.
		/// </summary>
		/// <param name="nodes">The set of nodes to project to their parents.</param>
		/// <returns>A distinct set of parents of all of the nodes.</returns>
		public static IReadOnlyCollection<Element> Parents(this IEnumerable<Node> nodes)
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
		/// <param name="nodes">The set of nodes to project to their parents.</param>
		/// <param name="selector">A selector to match parents against; only the parents
		/// that match this selector will be included in the result set.</param>
		/// <returns>A distinct set of parents of all of the nodes, where each parent
		/// matches the given selector.</returns>
		public static IReadOnlyCollection<Element> Parents(this IEnumerable<Node> nodes, string selector)
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
		/// <param name="nodes">The set of nodes to project to their ancestors.</param>
		/// <returns>A distinct set of ancestors of all of the nodes.</returns>
		public static IReadOnlyCollection<Element> Ancestors(this IEnumerable<Node> nodes)
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
		/// <param name="nodes">The set of nodes to project to their ancestors.</param>
		/// <param name="selector">A selector to match ancestors against; only the ancestors
		/// that match this selector will be included in the result set.</param>
		/// <returns>A distinct set of ancestors of all of the nodes, where each ancestor
		/// matches the given selector.</returns>
		public static IReadOnlyCollection<Element> Ancestors(this IEnumerable<Node> nodes, string selector)
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
		/// Filter the set of nodes to elements with any of the given classname(s).
		/// </summary>
		/// <param name="nodes">The set of nodes to filter.</param>
		/// <param name="className">The classname(s) to search for, space-delimited.</param>
		public static IEnumerable<Element> HasClass(this IEnumerable<Node> nodes, string className)
		{
			// Early-out fast case:  If there's no whitespace, test just the single name.
			if (!HasWhitespace(className))
				return nodes.OfType<Element>().Where(e => e.ClassNames.Contains(className));

			// Split on whitespace, then test each name provided.
			List<string> names = Element.SplitClassname(className);

			return nodes.OfType<Element>().Where(e =>
			{
				foreach (string name in names)
				{
					if (e.ClassNames.Contains(name))
						return true;
				}
				return false;
			});
		}

		/// <summary>
		/// Return true if the given classname has whitespace in it.
		/// </summary>
		/// <param name="className">The classname to test for whitespace.</param>
		/// <returns>True if it contains whitespace, false if it does not.</returns>
		private static bool HasWhitespace(string className)
		{
			foreach (char ch in className)
				if (ch <= 32)
					return true;
			return false;
		}

		/// <summary>
		/// Add the given classname(s) to each of the given elements.  Nodes that are not
		/// elements will be ignored.
		/// </summary>
		/// <param name="nodes">The elements to add classnames to.</param>
		/// <param name="className">A set of one or more classname(s), space-delimited.</param>
		public static void AddClass(this IEnumerable<Node> nodes, string className)
		{
			List<string> names = Element.SplitClassname(className);

			foreach (Node node in nodes)
			{
				if (!(node is Element element))
					continue;
				element.UpdateClassesInternal(names, []);
			}
		}

		/// <summary>
		/// Remove the given classname(s) from each of the given elements.  Nodes that are not
		/// elements will be ignored.
		/// </summary>
		/// <param name="nodes">The elements to remove classnames from.</param>
		/// <param name="className">A set of one or more classname(s), space-delimited.</param>
		public static void RemoveClass(this IEnumerable<Node> nodes, string className)
		{
			List<string> names = Element.SplitClassname(className);

			foreach (Node node in nodes)
			{
				if (!(node is Element element))
					continue;
				element.UpdateClassesInternal([], names);
			}
		}

		/// <summary>
		/// Add and remove the given classname(s) to each of the given elements.  Nodes that are not
		/// elements will be ignored.
		/// </summary>
		/// <param name="nodes">The elements to modify classnames on.</param>
		/// <param name="add">A set of one or more classname(s) to add, space-delimited.</param>
		/// <param name="remove">A set of one or more classname(s) to remove, space-delimited.</param>
		public static void UpdateClass(this IEnumerable<Node> nodes, string add, string remove)
		{
			List<string> adds = Element.SplitClassname(add);
			List<string> removes = Element.SplitClassname(remove);

			foreach (Node node in nodes)
			{
				if (!(node is Element element))
					continue;
				element.UpdateClassesInternal(adds, removes);
			}
		}

		/// <summary>
		/// Toggle the given classname(s) on each of the given elements.  Nodes that are not
		/// elements will be ignored.  "Toggling" means adding a classname if it doesn't already
		/// exist, or removing it if it does already exist.
		/// </summary>
		/// <param name="nodes">The elements to toggle classnames on.</param>
		/// <param name="className">A set of one or more classname(s) to toggle, space-delimited.</param>
		public static void ToggleClass(this IEnumerable<Node> nodes, string className)
		{
			List<string> names = Element.SplitClassname(className);

			foreach (Node node in nodes)
			{
				if (!(node is Element element))
					continue;
				element.ToggleClassesInternal(names);
			}
		}
	}
}
