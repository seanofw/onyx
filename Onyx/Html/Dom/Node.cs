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
	/// This contains three pointers (references) and two 32-bit integers of data:  That's
	/// ~20 bytes on a 32-bit machine, and ~32 bytes on a 64-bit machine.  This is the
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

		/// <summary>
		/// The location in the source HTML file where this node was declared, if known.
		/// </summary>
		public SourceLocation? SourceLocation { get; internal set; }

		#endregion

		#region JS DOM compatible properties

		/// <summary>
		/// JS DOM:  The children of this node.  This property is always non-null, but may be empty.
		/// This property is read-only; if you want to manipulate a container node, use the methods
		/// on the container node to do it.
		/// </summary>
		public abstract IReadOnlyList<Node> ChildNodes { get; }

		/// <summary>
		/// JS DOM: A reference to the first child of this node, if any.
		/// </summary>
		public virtual Node? FirstChild => null;

		/// <summary>
		/// JS DOM: A reference to the last child of this node, if any.
		/// </summary>
		public virtual Node? LastChild => null;

		/// <summary>
		/// JS DOM: A reference to the next sibling of this node under its parent, if any.
		/// </summary>
		public Node? NextSibling
			=> Parent != null && Index < Parent.Children.Count - 1 ? Parent.Children[Index + 1] : null;

		/// <summary>
		/// JS DOM: A reference to the previous sibling of this node under its parent, if any.
		/// </summary>
		public Node? PreviousSibling
			=> Parent != null && Index > 0 ? Parent.Children[Index - 1] : null;

		/// <summary>
		/// JS DOM: The type of this node (document, element, text, etc.), as an enumerated value.
		/// </summary>
		public abstract NodeType NodeType { get; }

		/// <summary>
		/// JS DOM: The name of this node (element or tag name).  Empty string if it doesn't have
		/// a name.  Note that this is *not* the same as the `name` attribute on an element.
		/// </summary>
		public abstract string NodeName { get; }

		/// <summary>
		/// JS DOM: The document that contains this node, if any.  If the owner is not an actual
		/// Document instance, this will be null.
		/// </summary>
		public Document? OwnerDocument => (Root ?? this) as Document;

		/// <summary>
		/// JS DOM: The parent node of this node, if any.
		/// </summary>
		public Node? ParentNode => Parent;

		/// <summary>
		/// JS DOM: The value of this node.  This property may be assigned to change the
		/// node's value, if the node is mutable.  The meaning of the "value" varies depending
		/// on what kind of node it is; for text nodes, it is the raw text; and for elements,
		/// it depends on the kind of element.  For most other nodes, this will be null.
		/// </summary>
		public virtual string? Value
		{
			get => null;
			set { }
		}

		/// <summary>
		/// JS DOM: The text contained within this node's subtree.  For text nodes, this will
		/// be the text.  For elements or documents or document fragments, this will be the
		/// concatenation of all text contained within them.
		/// </summary>
		public virtual string TextContent
		{
			get => string.Empty;
			set { }
		}

		#endregion

		#region Derived properties (and virtual properties)

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

		/// <summary>
		/// Whether this is the root node of this tree.
		/// </summary>
		public bool IsRoot => Root == this;

		/// <summary>
		/// Whether this is a leaf in the tree (as determined by its count of child nodes,
		/// not whether it inherits LeafNode).
		/// </summary>
		public bool IsLeaf => !HasChildNodes();

		/// <summary>
		/// The root of the tree that this node belongs to.
		/// </summary>
		public Node? RootNode => Root;

		/// <summary>
		/// The parent node of this node, as an Element, if it is an element.  If the parent
		/// is not an element, this will be null.
		/// </summary>
		public Element? ParentElement => Parent as Element;

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new node.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected Node()
		{
			_uniqueId = (ushort)Interlocked.Increment(ref _uniqueIdSource);
		}

		#endregion

		#region Tree manipulation

		/// <summary>
		/// JS DOM: Append a child to this node's collection of children.  For nodes that do not
		/// support children, this will throw NotSupportedException.
		/// </summary>
		/// <param name="child">The new child to append.  This will be detached from any existing
		/// position it may hold in any other tree before attaching it to this tree.  After
		/// attaching, this child's ParentNode and RootNode will be updated to point to this
		/// node and its RootNode, respectively, and this child's descendants' RootNode
		/// references will be similarly updated, recursively.</param>
		/// <exception cref="ArgumentNullException">Thrown if the child is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree.</exception>
		/// <exception cref="NotSupportedException">Thrown if this node's child collection is
		/// not modifiable.</exception>
		public abstract void AppendChild(Node child);

		/// <summary>
		/// JS DOM: Make a perfect duplicate of this node, and optionally its descendants.  Note
		/// that this does not copy any event listeners:  It only copies the node's data.
		/// </summary>
		/// <param name="deep">If true, clone not just this node but its descendants, recursively.</param>
		/// <returns>A clone of this node, possibly with its descendants, detached from any tree.</returns>
		public abstract Node CloneNode(bool deep = false);

		/// <summary>
		/// JS DOM: Insert a child in this node's collection of children before some other
		/// reference node.  For nodes that do not support children, this will throw
		/// NotSupportedException.
		/// </summary>
		/// <param name="newNode">The new child to append.  This will be detached from any existing
		/// position it may hold in any other tree before attaching it to this tree.  After
		/// attaching, this child's ParentNode and RootNode will be updated to point to this
		/// node and its RootNode, respectively, and this child's descendants' RootNode
		/// references will be similarly updated, recursively.</param>
		/// <param name="referenceNode">The reference node to insert the child before.  This
		/// reference node must already be a child of this parent.</param>
		/// <exception cref="ArgumentNullException">Thrown if the new node is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree,
		/// or if the reference node is not a child of this container.</exception>
		/// <exception cref="NotSupportedException">Thrown if this node's child collection is
		/// not modifiable.</exception>
		public abstract void InsertBefore(Node newNode, Node referenceNode);

		/// <summary>
		/// JS DOM: Remove a child from this parent container.  The child must be attached to this
		/// parent before invoking this method.
		/// </summary>
		/// <param name="child">The child to remove to this parent container.  After removal,
		/// this child's ParentNode and Root references will both become null.</param>
		/// <exception cref="ArgumentNullException">Thrown if the child node is null.</exception>
		/// <exception cref="HierarchyException">Thrown if the given node is not a child of this container.</exception>
		/// <exception cref="NotSupportedException">Thrown if this node's child collection is
		/// not modifiable.</exception>
		public abstract void RemoveChild(Node child);

		/// <summary>
		/// JS DOM: Replace the given reference node with a new node.  The reference node must be
		/// attached to this parent before invoking this method.  Before insertion, the new node
		/// will be detached if it is currently attached in another tree.
		/// </summary>
		/// <param name="newNode">The new child to insert.  This will be detached from any existing
		/// position it may hold in any other tree before attaching it to this tree.  After
		/// attaching, this child's ParentNode and RootNode will be updated to point to this
		/// node and its RootNode, respectively, and this child's descendants' RootNode
		/// references will be similarly updated, recursively.</param>
		/// <param name="referenceNode">The reference node to replace with the given child.
		/// This reference node must already be a child of this parent.</param>
		/// <exception cref="ArgumentNullException">Thrown if the new child node is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree,
		/// or if the reference node is not a child of this container.</exception>
		/// <exception cref="NotSupportedException">Thrown if this node's child collection is
		/// not modifiable.</exception>
		public abstract void ReplaceChild(Node newNode, Node referenceNode);

		/// <summary>
		/// JS DOM: Return true if this node has children, false if it does not.  For nodes that cannot
		/// have children, this will still return false.
		/// </summary>
		/// <returns>True if this node has at least one child, false if it has zero children.</returns>
		public abstract bool HasChildNodes();

		/// <summary>
		/// JS DOM: Clean up all text nodes under this node, recursively, by merging
		/// adjacent text nodes and removing empty text nodes.  If this node does not have
		/// children, this will be a no-op.
		/// </summary>
		public virtual void Normalize() { }

		/// <summary>
		/// JS DOM: Get the root node of this node's tree, if any.  If the node has no parent,
		/// this returns the node itself.
		/// </summary>
		/// <returns>The root node of this node's tree.</returns>
		public Node GetRootNode() => Root ?? this;

		#endregion

		#region Position comparisons

		/// <summary>
		/// Answer whether this node either contains the given other node, or *is* the given
		/// other node.  This answers a simple subtree relationship between this node and
		/// another node, without invoking the complex mechanics and overhead required by
		/// CompareDocumentPosition().
		/// </summary>
		/// <param name="other">The other node to compare against.</param>
		/// <returns>True if this node has the other node as a descendant, or if this node
		/// *is* the other node, and false for all other relationships.</returns>
		public bool ContainsOrIs(Node other)
		{
			for (; other != null; other = other.Parent!)
				if (other == this)
					return true;
			return false;
		}

		/// <summary>
		/// Answer quickly whether two nodes, which are both known to be under the same
		/// parent node, are before or after each other (or the same node).  This runs in
		/// O(1) time.
		/// </summary>
		/// <param name="other">The other node to compare against.</param>
		/// <returns>-1 if this node comes before other; +1 if this node comes after other;
		/// and 0 if this node *is* the other node.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the other node is null.</exception>
		/// <exception cref="HierarchyException">Thrown if the nodes do not share the
		/// same parent node.</exception>
		public int GetSiblingOrder(Node other)
		{
			if (ReferenceEquals(other, null))
				throw new ArgumentNullException(nameof(other));

			if (!ReferenceEquals(Parent, other.Parent))
				throw new HierarchyException("Nodes do not share the same parent node.");

			if (Index < other.Index)
				return -1;
			else if (Index > other.Index)
				return +1;
			else
				return 0;
		}

		/// <summary>
		/// JS DOM: Compare the position of this node to another node.  This can precisely
		/// answer the relationship between two nodes (siblings, ancestor or descendant, self,
		/// unrelated, etc.), but it is less efficient than ContainsOrIs(), since it has to
		/// perform a full analysis.
		/// </summary>
		/// <param name="other">The other node to compare against.</param>
		/// <returns>The relationship of the two nodes.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the other node is null.</exception>
		public DocumentPosition CompareDocumentPosition(Node other)
		{
			if (ReferenceEquals(other, null))
				throw new ArgumentNullException(nameof(other));

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

		#endregion

		#region Ancestors and descendants

		/// <summary>
		/// Get all ancestors of this node, or optionally stop at a given ancestor to
		/// retrieve a subtree.  This eagerly collects all nodes and returns them as a
		/// list, in order of descendant --> root.
		/// </summary>
		/// <param name="stopAt">If non-null, and this node is included in the ancestry
		/// of this node, no nodes higher than this node will be returned (but this
		/// node will be included in the result).</param>
		/// <returns>A list of all ancestors of this node, including this node, up to
		/// the root node or the `stopAt` node.</returns>
		public List<Node> GetAncestors(Node? stopAt = null)
			=> GetAncestors<Node>(stopAt);

		/// <summary>
		/// Get all ancestors of this node, of the given type; or optionally stop at
		/// a given ancestor to retrieve a subtree.  This eagerly collects all nodes
		/// of the given type and returns them as a list, in order of descendant --> root.
		/// </summary>
		/// <typeparam name="T">The type of the nodes to return.  Nodes that do not
		/// match this type will be excluded.</typeparam>
		/// <param name="stopAt">If non-null, and this node is included in the ancestry
		/// of this node, no nodes higher than this node will be returned (but this
		/// node will be included in the result).</param>
		/// <returns>A list of all ancestors of this node, including this node, up to
		/// the root node or the `stopAt` node.</returns>
		public List<T> GetAncestors<T>(Node? stopAt = null)
		{
			List<T> ancestors = new List<T>();
			GetAncestors(ancestors, stopAt);
			return ancestors;
		}

		/// <summary>
		/// Get all ancestors of this node, or optionally stop at a given ancestor to
		/// retrieve a subtree.  This eagerly collects all nodes and adds them to the
		/// provided collection, in order of descendant --> root.
		/// </summary>
		/// <param name="ancestors">A collection that will be added to.</returns>
		/// <param name="stopAt">If non-null, and this node is included in the ancestry
		/// of this node, no nodes higher than this node will be returned (but this
		/// node will be included in the result).</param>
		public void GetAncestors(ICollection<Node> ancestors, Node? stopAt = null)
			=> GetAncestors<Node>(ancestors, stopAt);

		/// <summary>
		/// Get all ancestors of this node of the given type, or optionally stop at a
		/// given ancestor to retrieve a subtree.  This eagerly collects all nodes and
		/// adds them to the provided collection, in order of descendant --> root.
		/// </summary>
		/// <typeparam name="T">The type of the nodes to return.  Nodes that do not
		/// match this type will be excluded.</typeparam>
		/// <param name="ancestors">A collection that will be added to.</returns>
		/// <param name="stopAt">If non-null, and this node is included in the ancestry
		/// of this node, no nodes higher than this node will be returned (but this
		/// node will be included in the result).</param>
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

		/// <summary>
		/// Get all ancestors of this node, or optionally stop at a given ancestor to
		/// retrieve a subtree.  This lazily collects all nodes and returns them as an
		/// enumerable, in order of descendant --> root.
		/// </summary>
		/// <param name="stopAt">If non-null, and this node is included in the ancestry
		/// of this node, no nodes higher than this node will be returned (but this
		/// node will be included in the result).</param>
		/// <returns>A lazily-evaluated sequence of all ancestors of this node, including
		/// this node, up to the root node or the `stopAt` node.</returns>
		public IEnumerable<Node> Ancestors(Node? stopAt = null)
			=> Ancestors<Node>(stopAt);

		/// <summary>
		/// Get all ancestors of this node of the given type, or optionally stop at a
		/// given ancestor to retrieve a subtree.  This lazily collects all nodes and
		/// returns them as an enumerable, in order of descendant --> root.
		/// </summary>
		/// <typeparam name="T">The type of the nodes to return.  Nodes that do not
		/// match this type will be excluded.</typeparam>
		/// <param name="stopAt">If non-null, and this node is included in the ancestry
		/// of this node, no nodes higher than this node will be returned (but this
		/// node will be included in the result).</param>
		/// <returns>A lazily-evaluated sequence of all ancestors of this node, including
		/// this node, up to the root node or the `stopAt` node.</returns>
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

		/// <summary>
		/// Get all descendants of this node, not including this node, in document order.
		/// This eagerly collects all nodes and returns them as a list.
		/// </summary>
		/// <returns>A list of all descendants of this node, excluding this node,
		/// in document order.</returns>
		public List<Node> GetDescendants()
			=> GetDescendants<Node>();

		/// <summary>
		/// Get all descendants of this node, not including this node, in document order.
		/// This eagerly collects all nodes and returns them as a list.
		/// </summary>
		/// <typeparam name="T">The type of the nodes to return.  Nodes that do not
		/// match this type will be excluded.</typeparam>
		/// <returns>A list of all descendants of this node, excluding this node,
		/// in document order.</returns>
		public List<T> GetDescendants<T>()
		{
			List<T> result = new List<T>();
			GetDescendants(result);
			return result;
		}

		/// <summary>
		/// Get all descendants of this node, not including this node, in document order.
		/// This eagerly collects all nodes and adds them to the given collection.
		/// </summary>
		/// <param name="descendants">A collection that will be added to.</returns>
		public void GetDescendants(ICollection<Node> descendants)
			=> GetDescendants<Node>(descendants);

		/// <summary>
		/// Get all descendants of this node of the given type, not including this node,
		/// in document order.  This eagerly collects all nodes and adds them to the
		/// given collection.
		/// </summary>
		/// <typeparam name="T">The type of the nodes to collect.  Nodes that do not
		/// match this type will be excluded.</typeparam>
		/// <param name="descendants">A collection that will be added to.</returns>
		public void GetDescendants<T>(ICollection<T> descendants)
		{
			foreach (Node node in ChildNodes)
			{
				if (node is T t)
					descendants.Add(t);
				node.GetDescendants(descendants);
			}
		}

		/// <summary>
		/// Get all descendants of this node, not including this node, in document order.
		/// This lazily collects all nodes and returns them as an enumerable sequence.
		/// </summary>
		/// <returns>A lazily-evaluated sequence of all descendants of this node,
		/// excluding this node, in document order.</returns>
		public IEnumerable<Node> Descendants()
			=> GetDescendants<Node>();

		/// <summary>
		/// Get all descendants of this node of the given type, not including this node,
		/// in document order.  This lazily collects all nodes and returns them as an
		/// enumerable sequence.
		/// </summary>
		/// <typeparam name="T">The type of the nodes to collect.  Nodes that do not
		/// match this type will be excluded.</typeparam>
		/// <returns>A lazily-evaluated sequence of all descendants of this node of the
		/// given type, excluding this node, in document order.</returns>
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

		#endregion

		#region Retrieval and matching by selectors

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
		private static bool IsKnownFastSelector(string selector)
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

		#region Stringification

		/// <summary>
		/// Convert this node to an equivalent HTML string representation, and append
		/// it to the given StringBuilder.
		/// </summary>
		/// <param name="stringBuilder">A StringBuilder to which this node's content
		/// will be appended.</param>
		public abstract void ToString(StringBuilder stringBuilder);

		/// <summary>
		/// Convert this node to an equivalent HTML string representation, and return it.
		/// </summary>
		/// <returns>This node, as a string of HTML text.</returns>
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		#endregion

		#region Attach/detach notifications

		/// <summary>
		/// This handler is triggered any time this node is attached or detached from a
		/// parent node.
		/// </summary>
		/// <param name="action">Whether this node has been or is being attached or detached
		/// from the given parent.</param>
		/// <param name="parent">The node that either was or will be this node's parent.</param>
		protected virtual void OnAttach(AttachmentAction action, ContainerNode parent)
		{
		}

		/// <summary>
		/// When a child node is about to be attached to a parent node, this event will
		/// be raised to indicate the change in the tree structure to the child node.
		/// </summary>
		/// <param name="parent">The parent that is about to receive a new child.</param>
		/// <param name="child">The child that parent is about to receive.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnAttaching(ContainerNode parent, Node child)
		{
			child.OnAttach(AttachmentAction.Attaching, parent);
		}

		/// <summary>
		/// When a child node has been attached to a parent node, this event will be raised
		/// to indicate the change in the tree structure to the child node.
		/// </summary>
		/// <param name="parent">The parent that is about to receive a new child.</param>
		/// <param name="child">The child that parent is about to receive.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnAttached(ContainerNode parent, Node child)
		{
			child.OnAttach(AttachmentAction.Attached, parent);

			if (child.Root is IElementLookupContainer fastLookupContainer
				&& child is Element element)
				fastLookupContainer.AddDescendant(element);
		}

		/// <summary>
		/// When a child node is about to be detached to a parent node, this event will
		/// be raised to indicate the change in the tree structure to the child node.
		/// </summary>
		/// <param name="parent">The parent that is about to lose a child.</param>
		/// <param name="child">The child that parent is about to lose.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnDetaching(ContainerNode parent, Node child)
		{
			if (child.Root is IElementLookupContainer fastLookupContainer
				&& child is Element element)
				fastLookupContainer.RemoveDescendant(element);

			child.OnAttach(AttachmentAction.Detaching, parent);
		}

		/// <summary>
		/// When a child node has been detached to a parent node, this event will
		/// be raised to indicate the change in the tree structure to the child node.
		/// </summary>
		/// <param name="parent">The parent that has just lost a child.</param>
		/// <param name="child">The child that parent has just lost.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnDetached(ContainerNode parent, Node child)
		{
			child.OnAttach(AttachmentAction.Detached, parent);
		}

		#endregion
	}
}
