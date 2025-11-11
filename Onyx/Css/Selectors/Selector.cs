using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Css.Parsing;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class Selector : IEquatable<Selector>
	{
		public IReadOnlyList<SelectorComponent> Path => _path;
		private readonly SelectorComponent[] _path;

		public Specificity Specificity => _specificity ??= CalculateSpecificity();
		private Specificity? _specificity;

		private static readonly CssSelectorParser _cssParser = new CssSelectorParser();

		public Selector(params SelectorComponent[] path)
			: this((IEnumerable<SelectorComponent>)path)
		{
		}

		public Selector(IEnumerable<SelectorComponent> path)
		{
			_path = path?.ToArray() ?? Array.Empty<SelectorComponent>();
		}

		/// <summary>
		/// Find all nodes at or under the given root node that match this selector,
		/// and add them to the given result set.
		/// </summary>
		/// <param name="root">The root of the subtree to search under.</param>
		/// <returns>The result set.  No guarantees are made as to the order in
		/// which the nodes will be returned, only that the correct nodes will be
		/// returned.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the root is null, or if
		/// it is somehow malformed such that it does not have an actual Root of its own.</exception>
		public IReadOnlySet<Element> Find(Node root)
		{
			HashSet<Element> result = new HashSet<Element>();
			Find(root, result);
			return result;
		}

		/// <summary>
		/// Find all elements at or under the given root node that match this selector,
		/// and add them to the given result set.
		/// </summary>
		/// <param name="root">The root of the subtree to search under.</param>
		/// <param name="result">The result set to append to.  No guarantees are made
		/// as to the order in which the nodes will be added, only that the correct
		/// elements will be added.</param>
		/// <returns>The number of elements added to the result set.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the root is null, or if
		/// it is somehow malformed such that it does not have an actual Root of its own.</exception>
		public int Find(Node root, ISet<Element> result)
		{
			Node? trueRoot = root?.Root;
			if (trueRoot == null)
				throw new ArgumentNullException("Node subtree must not be null.");

			if (_path.Length == 0)
				return 0;		// Empty selector matches nothing.

			IReadOnlyCollection<Node> baseSet;
			if (trueRoot is IElementLookupContainer fastLookupContainer)
			{
				// Use the query planner and optimized caches and selector analysis to
				// retrieve the base set as efficiently as possible.
				baseSet = QueryPlanner.ExecuteQuery(this, trueRoot, fastLookupContainer.ElementLookupTables);
			}
			else
			{
				baseSet = trueRoot.Descendants().ToList();
			}

			int numAdded = 0;

			foreach (Node candidate in baseSet)
			{
				if (!(candidate is Element element))
					continue;

				// Only add candidates if they match the whole selector.
				if (!IsMatch(element))
					continue;

				// Make sure this candidate is under the given root.
				if (root != trueRoot && !root!.ContainsOrIs(element))
					continue;

				// Add it to the set, counting if it was new.
				if (result.Add(element))
					numAdded++;
			}

			return numAdded;
		}

		public bool IsMatch(Element? element)
		{
			if (element == null || Path.Count == 0)
				return false;   // Empty selector can't match anything.

			// Test the deepest simple selector against the node directly.
			SimpleSelector simpleSelector = Path[^1].SimpleSelector;
			if (!simpleSelector.IsMatch(element))
				return false;

			// Pattern-match up the tree, searching for any combination of ancestors
			// that could match the path, excluding the one we just matched.  This
			// potentially can have high combinatorial complexity, but in practice
			// tends to approach a linear walk.
			return RecursivelyTestPath(Path.Count - 2, Path[^1].Combinator, element);
		}

		private bool RecursivelyTestPath(int index, Combinator combinator, Node node)
		{
			if (index < 0)
				return true;

			if (combinator == Combinator.AdjacentSibling)
			{
				// Test the immediate prior sibling only.
				SimpleSelector simpleSelector = Path[index].SimpleSelector;
				Node? prev = node.PreviousSibling;
				while (prev != null && !(prev is Element))
					prev = prev.PreviousSibling;
				if (!(prev is Element element) || !simpleSelector.IsMatch(element))
					return false;

				// Prior sibling matches, so keep going.
				if (index <= 0)
					return true;
				return RecursivelyTestPath(index - 1, Path[index - 1].Combinator, prev);
			}
			else if (combinator == Combinator.Child)
			{
				// Test the immediate parent only.
				SimpleSelector simpleSelector = Path[index].SimpleSelector;
				Node? parent = node.Parent;
				if (!(parent is Element element) || !simpleSelector.IsMatch(element))
					return false;

				// Parent matches, so keep going.
				if (index <= 0)
					return true;
				return RecursivelyTestPath(index - 1, Path[index - 1].Combinator, parent);
			}
			else if (combinator == Combinator.Descendant)
			{
				// Test all possible ancestor relationships going up the tree.
				SimpleSelector simpleSelector = Path[index].SimpleSelector;
				for (Node? ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
				{
					if (!(ancestor is Element element) || !simpleSelector.IsMatch(element))
						continue;

					// Ancestor matches, so try the rest of the selector against it.
					if (index <= 0)
						return true;
					if (RecursivelyTestPath(index - 1, Path[index - 1].Combinator, ancestor))
						return true;
				}
				return false;
			}
			else if (combinator == Combinator.GeneralSibling)
			{
				// Test all possible sibling relationships.
				SimpleSelector simpleSelector = Path[index].SimpleSelector;
				ContainerNode? parent = node.Parent;
				if (parent == null)
					return false;
				IReadOnlyList<Node> childNodes = parent.ChildNodes;
				for (int i = node.Index - 1; i >= 0; i--)
				{
					Node prev = childNodes[i];
					if (!(prev is Element element) || !simpleSelector.IsMatch(element))
						continue;

					// Previous matches, so try the rest of the selector against it.
					if (index <= 0)
						return true;
					if (RecursivelyTestPath(index - 1, Path[index - 1].Combinator, prev))
						return true;
				}
				for (int i = node.Index + 1; i < childNodes.Count; i++)
				{
					Node next = childNodes[i];
					if (!(next is Element element) || !simpleSelector.IsMatch(element))
						continue;

					// Next matches, so try the rest of the selector against it.
					if (index <= 0)
						return true;
					if (RecursivelyTestPath(index - 1, Path[index - 1].Combinator, next))
						return true;
				}
				return false;
			}
			else if (combinator == Combinator.Self)
			{
				SimpleSelector simpleSelector = Path[index].SimpleSelector;
				if (!(node is Element element) || !simpleSelector.IsMatch(element))
					return false;

				// Self matches, so keep going.
				if (index <= 0)
					return true;
				return RecursivelyTestPath(index - 1, Path[index - 1].Combinator, node);
			}
			else return false;
		}

		/// <summary>
		/// Find the closest ancestor element (including this node) that matches this compound selector.
		/// </summary>
		/// <param name="node">The starting node.</param>
		/// <returns>The closest matching ancestor element, or null if no ancestors match.</returns>
		public Element? Closest(Node node)
		{
			for (Node? current = node; current != null; current = current.Parent)
			{
				if (current is Element element && IsMatch(element))
					return element;
			}
			return null;
		}

		private Specificity CalculateSpecificity()
		{
			Specificity specificity = Specificity.Zero;
			foreach (SelectorComponent component in _path)
				specificity += component.SimpleSelector.Specificity;
			return specificity;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Selector other && Equals(other);

		public bool Equals(Selector? other)
			=> ReferenceEquals(other, null) ? false
				: ReferenceEquals(this, other) ? true
				: _path.SequenceEqual(other._path);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				foreach (SelectorComponent component in _path)
					hashCode = hashCode * 65599 + component.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(Selector? a, Selector? b)
			=> ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);

		public static bool operator !=(Selector? a, Selector? b)
			=> ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !a.Equals(b);

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		public void ToString(StringBuilder stringBuilder)
		{
			foreach (SelectorComponent component in _path)
			{
				component.ToString(stringBuilder);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Selector Parse(string text)
			=> _cssParser.ParseSelector(new CssLexer(text, "<no source>"),
				expectEoi: true, throwOnError: true)!;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Selector Parse(CssLexer lexer)
			=> _cssParser.ParseSelector(lexer,
				expectEoi: true, throwOnError: true)!;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string text, [MaybeNullWhen(false)] out Selector selector)
			=> (selector = _cssParser.ParseSelector(new CssLexer(text, "<no source>"))) != null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(CssLexer lexer, [MaybeNullWhen(false)] out Selector selector)
			=> (selector = _cssParser.ParseSelector(lexer)) != null;
	}
}
