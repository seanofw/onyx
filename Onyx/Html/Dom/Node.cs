using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Css.Selectors;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// This abstract base class represents a node --- an element, text, comment, or even
	/// the document root itself --- in the document tree.
	/// 
	/// This contains two pointers (references) and two 32-bit integers of data:  That's
	/// ~16 bytes on a 32-bit machine, and ~24 bytes on a 64-bit machine.  This is the
	/// baseline overhead for all nodes --- fairly small, but not zero.
	/// </summary>
	[DebuggerDisplay("Node #{UniqueId}: {NodeName} (at {Index})")]
	public abstract class Node
	{
		#region Properties and fields that store data

		/// <summary>
		/// The root of the tree this node belongs to, if any.
		/// </summary>
		internal ContainerNode? Root;

		/// <summary>
		/// The parent container node this node is attached under, if any.
		/// </summary>
		internal ContainerNode? Parent;

		/// <summary>
		/// A unique ID for this node, purely for debugging purposes.  This should *not*
		/// be used for any purposes other than identifying nodes during debugging, as
		/// it can and *will* roll over if enough nodes are allocated over time.  It is
		/// marked [Obsolete] to help ensure you don't write code against it.
		/// </summary>
		[Obsolete]
		public int UniqueId => _uniqueId;
		private ushort _uniqueId;
		private static int _uniqueIdSource = 0;

		/// <summary>
		/// Flags controlling how this node is rendered.
		/// </summary>
		internal RenderFlags RenderFlags;

		/// <summary>
		/// Flags controlling how this node interacts with styling.
		/// </summary>
		internal StyleFlags StyleFlags;

		/// <summary>
		/// The zero-based position of this node in the array of its parents' children.
		/// A negative number indicates that this node is not attached to a parent node.
		/// </summary>
		public int Index { get; internal set; } = -1;

		#endregion

		/// <summary>
		/// The number of immediate-child elements this node contains.
		/// </summary>
		public virtual int ChildElementCount => 0;

		/// <summary>
		/// The count of descendant NODES of this node, including this node itself.
		/// </summary>
		public virtual int SubtreeNodeCount => 1;

		/// <summary>
		/// The count of descendant ELEMENTS of this node, including this node itself.
		/// </summary>
		public virtual int SubtreeElementCount => 0;

		/// <summary>
		/// The count of descendant NODES of this node, not including this node itself.
		/// </summary>
		public int DescendantNodeCount => SubtreeNodeCount - 1;

		/// <summary>
		/// The count of descendant NODES of this node, not including this node itself.
		/// </summary>
		public int DescendantElementCount => this is Element ? SubtreeElementCount - 1 : SubtreeElementCount;

		public bool IsRootNode => Root == this;
		public bool IsLeafNode => !HasChildNodes();

		public SourceLocation? SourceLocation { get; internal set; }

		public abstract IReadOnlyList<Node> ChildNodes { get; }
		public virtual Node? FirstChild => null;
		public bool IsConnected
			=> Parent != null;
		public virtual Node? LastChild => null;
		public Node? NextSibling
			=> Parent != null && Index < Parent.Children.Count - 1 ? Parent.Children[Index + 1] : null;
		public Node? PreviousSibling
			=> Parent != null && Index > 0 ? Parent.Children[Index - 1] : null;
		public abstract NodeType NodeType { get; }
		public abstract string NodeName { get; }

		public Document? OwnerDocument => (Root ?? this) as Document;
		public Node? ParentNode => Parent;
		public Element? ParentElement => Parent as Element;

		public virtual string? Value
		{
			get => null;
			set { }
		}

		public virtual string TextContent
		{
			get => string.Empty;
			set { }
		}

		public abstract void AppendChild(Node child);
		public abstract Node CloneNode(bool deep = false);
		public abstract bool HasChildNodes();
		public abstract void InsertBefore(Node newNode, Node referenceNode);
		public abstract void RemoveChild(Node child);
		public abstract void ReplaceChild(Node newNode, Node referenceNode);

		public virtual void Normalize() { }

		public Node GetRootNode() => Root ?? this;

		public Node()
		{
			_uniqueId = (ushort)Interlocked.Increment(ref _uniqueIdSource);
		}

		public bool ContainsOrIs(Node other)
		{
			for (; other != null; other = other.Parent!)
				if (other == this)
					return true;
			return false;
		}

		public DocumentPosition CompareDocumentPosition(Node other)
		{
			// Do some easy fast checks first.
			if (ReferenceEquals(this, other))
				return DocumentPosition.Contains;
			if (ReferenceEquals(Parent, other.Parent))
				return Index.CompareTo(other.Index) < 0 ? DocumentPosition.Preceding : DocumentPosition.Following;
			if (ReferenceEquals(this, other.Parent))
				return DocumentPosition.Contains;
			if (ReferenceEquals(other, Parent))
				return DocumentPosition.ContainedBy;

			// Collect the full ancestry of each node, and if we manage to reach
			// the other node, stop since we'll have answered the question.
			List<Node> thisAncestry = GetAncestors(stopAt: other);
			if (thisAncestry[0] == other)
				return DocumentPosition.ContainedBy;
			List<Node> otherAncestry = other.GetAncestors(stopAt: this);
			if (otherAncestry[0] == this)
				return DocumentPosition.Contains;

			// If the root nodes are different, these aren't related.
			if (thisAncestry[0] != otherAncestry[0])
				return DocumentPosition.Disconnected;

			// Spin down the list and find the first mismatched pair, then compare them.
			int end = Math.Min(thisAncestry.Count, otherAncestry.Count);
			for (int i = 0; i < end; i++)
				if (!ReferenceEquals(thisAncestry[i], otherAncestry[i]))
					return thisAncestry[i].Index.CompareTo(otherAncestry[i].Index) < 0
						? DocumentPosition.Preceding : DocumentPosition.Following;

			// Whichever list is longer is therefore a descendant of the other.
			return thisAncestry.Count > otherAncestry.Count
				? DocumentPosition.ContainedBy
				: DocumentPosition.Contains;
		}

		/// <summary>
		/// Simple ordering comparison between nodes A and B:  This answers whether
		/// A is before or after B, in document order.
		/// </summary>
		/// <param name="a">The first node to compare.</param>
		/// <param name="b">The second node to compare.</param>
		/// <returns>-1 if A comes before B; 0 if A and B are the same node; +1 if A comes after B;
		/// and int.MinValue if they belong to disjoint trees.</returns>
		public static int ComparePosition(Node? a, Node? b)
			=> ComparePositionInternal(a, b, new AncestorList(), new AncestorList());

		/// <summary>
		/// Simple ordering comparison between nodes A and B:  This answers whether
		/// A is before or after B, in document order.
		/// </summary>
		/// <param name="a">The first node to compare.</param>
		/// <param name="b">The second node to compare.</param>
		/// <param name="aAncestors">A temporary collection used to collect and compare the ancestors of A.</param>
		/// <param name="bAncestors">A temporary collection used to collect and compare the ancestors of B.</param>
		/// <returns>-1 if A comes before B; 0 if A and B are the same node; +1 if A comes after B;
		/// and int.MinValue if they belong to disjoint trees.</returns>
		internal static int ComparePositionInternal(Node? a, Node? b, AncestorList aAncestors, AncestorList bAncestors)
		{
			// Easy cases first.
			if (ReferenceEquals(a, b))
				return 0;
			if (ReferenceEquals(a, null))
				return +1;
			if (ReferenceEquals(b, null))
				return -1;

			// Build ancestor chains for both nodes, in order of root --> leaf.
			static void GetAncestors(Node node, AncestorList ancestors)
			{
				ancestors.Clear();
				for (Node? current = node; current != null; current = current.Parent)
					ancestors.Insert(current);
			}

			GetAncestors(a, aAncestors);
			GetAncestors(b, bAncestors);

			// If they have different roots, they’re in disjoint trees.
			if (!ReferenceEquals(aAncestors[0], bAncestors[0]))
				return int.MinValue;

			// Walk down until the paths diverge.
			int depth = Math.Min(aAncestors.Count, bAncestors.Count);
			int i = 0;
			while (i < depth && ReferenceEquals(aAncestors[i], bAncestors[i]))
				i++;

			// If one is an ancestor of the other:
			if (i == aAncestors.Count)
				return -1; // a is ancestor of b, so a comes before
			if (i == bAncestors.Count)
				return +1; // b is ancestor of a, so a comes after

			// Compare index positions, since they're siblings.
			return aAncestors[i].Index < bAncestors[i].Index ? -1 : +1;
		}

		public List<Node> GetAncestors(Node? stopAt = null)
			=> GetAncestors<Node>(stopAt);

		public List<T> GetAncestors<T>(Node? stopAt = null)
		{
			List<T> ancestors = new List<T>();
			GetAncestors(ancestors, stopAt);
			return ancestors;
		}

		public void GetAncestors(ICollection<Node> ancestors, Node? stopAt = null)
			=> GetAncestors<Node>(ancestors, stopAt);

		public void GetAncestors<T>(ICollection<T> ancestors, Node? stopAt = null)
		{
			for (Node? node = this; node != null; node = node.Parent)
			{
				if (node is T t)
					ancestors.Add(t);
				if (node == stopAt)
					break;
			}
		}

		public IEnumerable<Node> Ancestors(Node? stopAt = null)
			=> Ancestors<Node>(stopAt);

		public IEnumerable<T> Ancestors<T>(Node? stopAt = null)
		{
			for (Node? node = this; node != null; node = node.Parent)
			{
				if (node is T t)
					yield return t;
				if (node == stopAt)
					break;
			}
		}

		public IEnumerable<Node> Descendants()
			=> GetDescendants<Node>();

		public IEnumerable<T> Descendants<T>()
		{
			foreach (Node node in ChildNodes)
			{
				if (node is T t)
					yield return t;
				foreach (T childT in node.Descendants<T>())
					yield return childT;
			}
		}

		public List<Node> GetDescendants()
			=> GetDescendants<Node>();

		public List<T> GetDescendants<T>()
		{
			List<T> result = new List<T>();
			GetDescendants(result);
			return result;
		}

		public void GetDescendants(ICollection<Node> collection)
			=> GetDescendants<Node>(collection);

		public void GetDescendants<T>(ICollection<T> collection)
		{
			foreach (Node node in ChildNodes)
			{
				if (node is T t)
					collection.Add(t);
				node.GetDescendants(collection);
			}
		}

		#region Selectors

		/// <summary>
		/// Match this node against a selector.
		/// </summary>
		/// <param name="selector">The selector to match this node against.</param>
		/// <returns>True if this node matches the selector, false if it does not.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsMatch(Selector selector)
			=> this is Element element && selector.IsMatch(element);

		/// <summary>
		/// Match this node against a selector.
		/// </summary>
		/// <param name="selector">The selector to match this node against.</param>
		/// <returns>True if this node matches the selector, false if it does not.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsMatch(CompoundSelector selector)
			=> selector.IsMatch(this);

		/// <summary>
		/// Match this node against a selector.
		/// </summary>
		/// <param name="selector">The selector to match this node against.</param>
		/// <returns>True if this node matches the selector, false if it does not.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsMatch(string selector)
			=> GetCompoundSelector(selector).IsMatch(this);

		/// <summary>
		/// Find all matching elements at or under this subtree root.
		/// </summary>
		/// <param name="selector">The selector to search for at or under this subtree root.</param>
		/// <returns>The set of all matching elements.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlySet<Element> Find(string selector)
		{
			if (this is IElementLookupContainer container
				&& Parent == null
				&& selector.Length >= 2
				&& IsKnownFastSelector(selector))
			{
				// Short-circuit, because we can prove it's fast:  We're the root node,
				// and this is an ID or classname selector, so there's no filtering, so we
				// can use a straight lookup.
				if (selector[0] == '#')
				{
					IReadOnlyCollection<Element> ids = container.GetElementsById(selector.Substring(1));
					return ids.Count == 0 ? []
						: ids is HashSet<Element> idsSet ? idsSet
						: ids.ToHashSet();
				}
				else
				{
					IReadOnlyCollection<Element> ids = container.GetElementsByClassname(selector.Substring(1));
					return ids.Count == 0 ? []
						: ids is HashSet<Element> idsSet ? idsSet
						: ids.ToHashSet();
				}
			}

			IReadOnlySet<Element> set = GetCompoundSelector(selector).Find(this);
			return set;
		}

		/// <summary>
		/// Test for forms that are *just* an ID or a classname, so we can skip
		/// the selector engine for cases where we could just use a single fast lookup.
		/// If you do anything even remotely fancy, like put whitespace around it, the
		/// answer is "no", but for the really common case where it's just an ID or a
		/// classname off the document root, we optimize the heck out of it.
		/// </summary>
		/// <param name="selector">The selector to test.</param>
		/// <returns>True if the selector is a known "fast form", false otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private bool IsKnownFastSelector(string selector)
		{
			if (selector.Length <= 2)
				return false;

			char ch;
			if ((ch = selector[0]) != '.' && ch != '#')
				return false;

			if (!(((ch = selector[1]) >= 'a' && ch <= 'z')
				|| ch >= 'A' && ch <= 'Z'
				|| ch == '_'))
				return false;

			for (int i = 2; i < selector.Length; i++)
			{
				if (!(((ch = selector[i]) >= 'a' && ch <= 'z')
					|| ch >= 'A' && ch <= 'Z'
					|| ch >= '0' && ch <= '9'
					|| ch == '_'))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Find all matching elements at or under this subtree root.
		/// </summary>
		/// <param name="selector">The selector to search for at or under this subtree root.</param>
		/// <returns>The set of all matching elements.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlySet<Element> Find(Selector selector)
			=> selector.Find(this);

		/// <summary>
		/// Find all matching elements at or under this subtree root.
		/// </summary>
		/// <param name="selector">The selector to search for at or under this subtree root.</param>
		/// <returns>The set of all matching elements.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IReadOnlySet<Element> Find(CompoundSelector selector)
			=> selector.Find(this);

		/// <summary>
		/// Get the *first* matching element at or under this subtree root, in document order.
		/// </summary>
		/// <param name="selector">The selector to search for at or under this subtree root.</param>
		/// <returns>The first matching element.</returns>
		public Element? Get(string selector)
			=> Find(selector).OrderByPosition().FirstOrDefault();

		/// <summary>
		/// Get the *first* matching element at or under this subtree root, in document order.
		/// </summary>
		/// <param name="selector">The selector to search for at or under this subtree root.</param>
		/// <returns>The first matching element.</returns>
		public Element? Get(Selector selector)
			=> selector.Find(this).OrderByPosition().FirstOrDefault();

		/// <summary>
		/// Get the *first* matching element at or under this subtree root, in document order.
		/// </summary>
		/// <param name="selector">The selector to search for at or under this subtree root.</param>
		/// <returns>The first matching element.</returns>
		public Element? Get(CompoundSelector selector)
			=> selector.Find(this).OrderByPosition().FirstOrDefault();

		/// <summary>
		/// Parse or look up the given CompoundSelector.
		/// </summary>
		/// <param name="selector">The text of the selector to parse, or to look up a
		/// previously-parsed form of.</param>
		/// <returns>The parsed CompoundSelector.</returns>
		/// <exception cref="ArgumentException">Thrown if the selector is not syntactically valid.</exception>
		public CompoundSelector GetCompoundSelector(string selector)
		{
			CompoundSelector compoundSelector;
			if (Root is IElementLookupContainer fastLookupContainer)
			{
				compoundSelector = fastLookupContainer.ElementLookupTables.ParsedSelectors.GetOrAdd(
					selector, s => CompoundSelector.TryParse(s, out CompoundSelector? cs) ? cs : null)
					?? CompoundSelector.Parse(selector);
			}
			else
			{
				compoundSelector = CompoundSelector.Parse(selector);
			}

			return compoundSelector;
		}

		#endregion

		public abstract void ToString(StringBuilder stringBuilder);

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// This handler is triggered any time this node is attached or detached from a
		/// parent node.
		/// </summary>
		protected virtual void OnAttach(AttachmentAction action, ContainerNode parent)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnAttaching(ContainerNode parent, Node child)
		{
			child.OnAttach(AttachmentAction.Attaching, parent);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnAttached(ContainerNode parent, Node child)
		{
			child.OnAttach(AttachmentAction.Attached, parent);

			if (child.Root is IElementLookupContainer fastLookupContainer
				&& child is Element element)
				fastLookupContainer.AddDescendant(element);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnDetaching(ContainerNode parent, Node child)
		{
			if (child.Root is IElementLookupContainer fastLookupContainer
				&& child is Element element)
				fastLookupContainer.RemoveDescendant(element);

			child.OnAttach(AttachmentAction.Detaching, parent);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnDetached(ContainerNode parent, Node child)
		{
			child.OnAttach(AttachmentAction.Detached, parent);
		}
	}
}
