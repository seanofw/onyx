using System.Text;
using Onyx.Css;
using Onyx.Html.Parsing;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A "container node" is a base class for all nodes that can potentially contain
	/// children (including Element and Document).  This implements all of the necessary
	/// DOM-like mechanics and events for a Node to really be part of a tree.
	/// </summary>
	public abstract class ContainerNode : Node, IList<Node>
	{
		#region Private fields with storage

		/// <summary>
		/// The actual internal storage of the children, as a list whose internal storage
		/// shape will vary depending on how many children there are.
		/// </summary>
		private NodeList<Node>? _children;

		/// <summary>
		/// The number of immediate children of this container that are ELEMENTS.
		/// </summary>
		private int _childElementCount;

		/// <summary>
		/// The count of descendant NODES of this node, including this node itself.
		/// </summary>
		private int _subtreeNodeCount;

		/// <summary>
		/// The count of descendant ELEMENTS of this node, including this node itself.
		/// </summary>
		private int _subtreeElementCount;

		#endregion

		#region Tree properties

		/// <summary>
		/// The children of this container, as an immutable list.  This is an alternate
		/// name for the JS DOM ChildNodes property, but is slightly shorter and more readable.
		/// (Use methods on this ContainerNode to modify this collection.)
		/// </summary>
		public IReadOnlyList<Node> Children => _children ?? (IReadOnlyList<Node>)Array.Empty<Node>();

		/// <summary>
		/// The number of immediate children of this container that are ELEMENTS.
		/// Equivalent to Children.Count(e => e is Element) but returns in O(1) time.
		/// </summary>
		public override int ChildElementCount => _childElementCount;

		/// <summary>
		/// The count of descendant NODES of this node, including this node itself.
		/// Equivalent to recursively counting the nodes in this subtree, but returns in O(1) time.
		/// </summary>
		public override int SubtreeNodeCount => _subtreeNodeCount;

		/// <summary>
		/// The count of descendant ELEMENTS of this node, including this node itself.
		/// Equivalent to recursively counting the ELEMENTS in this subtree, but returns in O(1) time.
		/// </summary>
		public override int SubtreeElementCount => _subtreeElementCount;

		/// <summary>
		/// The number of child nodes contained by this container.  (This is children,
		/// not descendants.)
		/// </summary>
		public int Count => _children?.Count ?? 0;

		#endregion

		#region DOM properties

		/// <summary>
		/// JS DOM: The children of this container, as an immutable list.  This property
		/// is provided for JS DOM compatibility, and is equivalent to the Children property,
		/// but it's slightly longer and less readable.
		/// </summary>
		public override IReadOnlyList<Node> ChildNodes => Children;

		/// <summary>
		/// JS DOM: The first child of this container.
		/// </summary>
		public override Node? FirstChild => _children != null ? _children[0] : null;

		/// <summary>
		/// JS DOM: The last child of this container.
		/// </summary>
		public override Node? LastChild => _children != null ? _children[_children.Count - 1] : null;

		/// <summary>
		/// JS DOM: Reading the TextContent will provide the combined text of all child nodes.
		/// Writing the TextContent will replace all child nodes with the provided string.
		/// </summary>
		public override string TextContent
		{
			get => string.Join("", Children.Select(c => c.TextContent));

			set
			{
				Clear();
				AppendChild(new TextNode(value));
			}
		}

		/// <summary>
		/// JS DOM: Reading the InnerHtml property will produce equivalent HTML to that which was used
		/// to create all descendant nodes.  Writing the InnerHtml property will parse the given HTML
		/// text and then replace all descendant nodes of this container with the parsed nodes.
		/// </summary>
		public string InnerHtml
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Node child in Children)
				{
					child.ToString(stringBuilder);
				}
				return stringBuilder.ToString();
			}

			set
			{
				HtmlParser.ParseInnerHtml(value, this);
			}
		}

		/// <summary>
		/// JS DOM: Reading the OuterHtml property will produce equivalent HTML to that which was used
		/// to create this node and all of its descendants.  Writing the OuterHtml property will
		/// parse the given HTML text, and will then replace *this* node itself with the resulting
		/// node(s), if and only if the immediate parent of this node is a ContainerNode.
		/// </summary>
		public string OuterHtml
		{
			get => ToString();

			set
			{
				if (Parent is not ContainerNode containerNode)
					throw new InvalidOperationException("OuterHtml cannot be assigned on a node whose parent is not a modifiable ContainerNode.");

				DocumentFragment fragment = HtmlParser.ParseOuterHtml(value);

				while (fragment.FirstChild != null)
					containerNode.InsertBefore(fragment.FirstChild, this);
			}
		}

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new container.
		/// </summary>
		protected ContainerNode()
		{
			_subtreeNodeCount = 1;
			_subtreeElementCount = this is Element ? 1 : 0;
		}

		#endregion

		#region High-level collection-manipulation functions

		/// <summary>
		/// Access this container's children as an ordinary C# list.
		/// </summary>
		/// <param name="index">The index of the child to retrieve or replace.  This must
		/// be a valid index within the collection.</param>
		/// <returns>The current child at the given index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not
		/// within the range of children of this container.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the container node
		/// cannot be modified.</exception>
		public Node this[int index]
		{
			get
			{
				if (_children == null)
					throw new ArgumentOutOfRangeException(nameof(index), "Child node index is out of range.");
				return _children[index];
			}

			set
			{
				ReplaceChild(index, value);
			}
		}

		/// <summary>
		/// JS DOM: Append a child node to the end of this container.  If the node is already attached to the
		/// tree, it will be detached first (in O(n) time).  Otherwise, this runs in O(1) for small
		/// containers, O(lg n) for large containers.
		/// </summary>
		/// <param name="child">The child to append.</param>
		/// <exception cref="ArgumentNullException">Thrown if the child is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree.</exception>
		public override void AppendChild(Node child)
		{
			ThrowIfReadOnly();
			ThrowIfInvalidChild(child);
			ThrowIfCycle(child);

			if (child.Parent is not null)
				child.Parent.RemoveChild(child);

			AppendChildFastAndUnsafe(child);
		}

		/// <summary>
		/// Append a child, but only update the references:  Assume this will not create cycles
		/// and that all references are valid, and that no safety checks are required.  Runs in O(1)
		/// for small containers, O(lg n) for large containers.
		/// </summary>
		/// <param name="child">The new child to append to the collection of children of this container.
		/// This must not be null or already attached to the tree or able to a create a cycle.</param>
		internal void AppendChildFastAndUnsafe(Node child)
		{
			Node.OnAttaching(this, child);

			_children ??= new NodeList<Node>();
			_children.Add(child);

			child.Parent = this;
			SetRoot(child, Root);
			child.Index = _children.Count - 1;

			_childElementCount += (child is Element ? +1 : 0);
			UpdateSubtreeCount(child.SubtreeNodeCount, child.SubtreeElementCount);

			Node.OnAttached(this, child);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// JS DOM: Return whether this has child nodes (O(1) lookup).
		/// </summary>
		/// <returns>True if this has child nodes, false if it does not.</returns>
		public override bool HasChildNodes()
			=> _children?.Count != 0;

		/// <summary>
		/// JS DOM: Insert the given node before another reference node that is known to already be a child
		/// of this container.  If the node is already attached to the tree, it will be detached first
		/// (in O(n) time).  Otherwise, this runs in O(1) for small containers, O(lg n) for large containers.
		/// </summary>
		/// <param name="newNode">The new node to insert.</param>
		/// <param name="referenceNode">The reference node that is already a child of this container.
		/// If this is null, the new node will be appended after the last child of the container.</param>
		/// <exception cref="ArgumentNullException">Thrown if the new node is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree,
		/// or if the reference node is not a child of this container.</exception>
		public override void InsertBefore(Node newNode, Node referenceNode)
		{
			if (referenceNode is null)
			{
				AppendChild(newNode);
				return;
			}

			ThrowIfReadOnly();
			ThrowIfInvalidChild(newNode);
			ThrowIfCycle(newNode);
			ThrowIfWrongParent(referenceNode);

			if (newNode.Parent is not null)
				newNode.Parent.RemoveChild(newNode);

			InsertChildFastAndUnsafe(newNode, referenceNode, referenceNode.Index);
		}

		/// <summary>
		/// Insert the given node at the given position of this container.  If the node is already attached
		/// to the tree, it will be detached first (in O(n) time).  Otherwise, this runs in O(1) for small
		/// containers, O(lg n) for large containers.
		/// </summary>
		/// <param name="newNode">The new node to insert.</param>
		/// <param name="index">The index at which to insert.  This must be a valid index from 0 to
		/// the number of nodes (inclusive).</param>
		/// <exception cref="ArgumentNullException">Thrown if the new node is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the index is less than 0 or greater than
		/// the current count of nodes.</exception>
		public void Insert(int index, Node newNode)
		{
			if (index == (_children?.Count ?? 0))
			{
				AppendChild(newNode);
				return;
			}

			ThrowIfInvalidChild(newNode);
			ThrowIfCycle(newNode);
			ThrowIfReadOnly();

			if (newNode.Parent is not null)
				newNode.Parent.RemoveChild(newNode);

			Node referenceNode = _children![index];

			InsertChildFastAndUnsafe(newNode, referenceNode, index);
		}

		/// <summary>
		/// Insert a child, but only update the references:  Assume this will not create cycles
		/// and that all references are valid.  Runs in O(1) for small containers, O(lg n) for
		/// large containers.
		/// </summary>
		/// <param name="newNode">The new child to insert before the given reference node.
		/// This must not be null or already attached to the tree or able to create a cycle.</param>
		/// <param name="referenceNode">The reference node the child must be inserted before.
		/// This must not be null, and must have this container as its parent.</param>
		/// <param name="referenceIndex">The index of the reference node.</param>
		internal void InsertChildFastAndUnsafe(Node newNode, Node referenceNode, int referenceIndex)
		{
			Node.OnAttaching(this, newNode);

			_children ??= new NodeList<Node>();
			_children!.Insert(referenceIndex, newNode);

			newNode.Parent = this;
			SetRoot(newNode, Root);
			_childElementCount += (newNode is Element ? +1 : 0);
			UpdateSubtreeCount(newNode.SubtreeNodeCount, newNode.SubtreeElementCount);
			UpdateIndexesOnAndAfter(referenceIndex);

			Node.OnAttached(this, newNode);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// Recursively update the index (and subtree OrderingKeys) of the given child
		/// (by index) and all children after it.
		/// </summary>
		/// <param name="startIndex">The first index to update.</param>
		private void UpdateIndexesOnAndAfter(int startIndex)
		{
			for (int i = startIndex; i < _children!.Count; i++)
				_children[i].Index = i;
		}

		/// <summary>
		/// JS DOM: Remove this child from this parent.  Runs in O(n) time.
		/// </summary>
		/// <param name="child">The child to detach.</param>
		/// <exception cref="ArgumentNullException">Thrown if the child node is null.</exception>
		/// <exception cref="HierarchyException">Thrown if the given node is not a child of this container.</exception>
		public override void RemoveChild(Node child)
		{
			ThrowIfInvalidChild(child);
			ThrowIfWrongParent(child);
			ThrowIfReadOnly();

			Node.OnDetaching(this, child);

			if (_children != null)
			{
				_children.Remove(child);

				if (_children.Count == 0)
					_children = null;
			}

			child.Parent = null;
			SetRoot(child, null);
			UpdateIndexesOnAndAfter(child.Index);
			_childElementCount += (child is Element ? -1 : 0);
			UpdateSubtreeCount(-child.SubtreeNodeCount, -child.SubtreeElementCount);
			child.Index = -1;

			Node.OnDetached(this, child);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// Remove a single child from this parent, at the given index.  Runs in O(n) time.
		/// </summary>
		/// <param name="index">The index of the child to remove.  This must be from 0 to Count-1,
		/// inclusive.</param>
		public void RemoveAt(int index)
		{
			ThrowIfIndexOutOfRange(index);
			ThrowIfReadOnly();

			Node child = _children![index];

			Node.OnDetaching(this, child);

			_children.RemoveAt(index);

			if (_children.Count == 0)
				_children = null;

			UpdateIndexesOnAndAfter(index);
			_childElementCount += (child is Element ? -1 : 0);
			UpdateSubtreeCount(child.SubtreeNodeCount, child.SubtreeElementCount);
			child.Index = -1;

			Node.OnDetached(this, child);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// JS DOM: Remove the given reference node from this container's children, and insert the given
		/// new node in its place.  This is more efficient than InsertBefore+RemoveChild, not just
		/// replacing two calls with one, but more efficient under the hood too, avoiding the O(n)
		/// overhead of RemoveChild.  If the new node is already attached to the tree, it will be
		/// detached first in O(n) time).  Otherwise, this runs in O(1) for small containers,
		/// O(lg n) for large containers.
		/// </summary>
		/// <param name="newNode">The new node to be inserted.</param>
		/// <param name="referenceNode">The reference node to be removed.</param>
		/// <exception cref="ArgumentNullException">Thrown if the new child node is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree,
		/// or if the reference node is not a child of this container.</exception>
		public override void ReplaceChild(Node newNode, Node referenceNode)
		{
			if (referenceNode is null)
			{
				AppendChild(newNode);
				return;
			}

			ThrowIfReadOnly();
			ThrowIfInvalidChild(newNode);
			ThrowIfCycle(newNode);
			ThrowIfWrongParent(referenceNode);

			if (newNode.Parent is not null)
				newNode.Parent.RemoveChild(newNode);

			ReplaceChildFastAndUnsafe(newNode, referenceNode);
		}

		/// <summary>
		/// Remove the given reference node from this container's children, and insert the given
		/// new node in its place.  This is more efficient than InsertBefore+RemoveChild, not just
		/// replacing two calls with one, but more efficient under the hood too, avoiding the O(n)
		/// overhead of RemoveChild.  If the new node is already attached to the tree, it will be
		/// detached first in O(n) time).  Otherwise, this runs in O(1) for small containers,
		/// O(lg n) for large containers.
		/// </summary>
		/// <param name="index">The index of the existing node to be replaced.</param>
		/// <param name="newNode">The new node to be inserted.</param>
		/// <exception cref="ArgumentNullException">Thrown if the new child node is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range of the
		/// existing nodes of this container.</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree,
		/// or if the reference node is not a child of this container.</exception>
		public void ReplaceChild(int index, Node newNode)
		{
			ThrowIfReadOnly();
			ThrowIfInvalidChild(newNode);
			ThrowIfCycle(newNode);
			ThrowIfIndexOutOfRange(index);

			if (newNode.Parent is not null)
				newNode.Parent.RemoveChild(newNode);

			ReplaceChildFastAndUnsafe(newNode, _children![index]);
		}

		/// <summary>
		/// Remove the given reference node from this container's children, and insert the given
		/// new node in its place.  This skips all safety checks, and performs just the replacement
		/// directly.  It still runs in O(n), but it has much smaller constant overhead without all
		/// the checks.
		/// </summary>
		/// <param name="newNode">The new node to be inserted.</param>
		/// <param name="referenceNode">The reference node to be removed.</param>
		internal void ReplaceChildFastAndUnsafe(Node newNode, Node referenceNode)
		{
			Node.OnDetaching(this, referenceNode);
			Node.OnAttaching(this, newNode);

			_children![referenceNode.Index] = newNode;
			newNode.Parent = this;
			referenceNode.Parent = null;
			SetRoot(newNode, Root);
			newNode.Index = referenceNode.Index;
			SetRoot(referenceNode, null);
			_childElementCount += (referenceNode is Element ? -1 : 0) + (newNode is Element ? +1 : 0);
			UpdateSubtreeCount(newNode.SubtreeNodeCount - referenceNode.SubtreeNodeCount,
				newNode.SubtreeElementCount - referenceNode.SubtreeElementCount);
			referenceNode.Index = -1;

			Node.OnDetached(this, referenceNode);
			Node.OnAttached(this, newNode);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// JS DOM: Normalize text nodes in this container and in all deeper subtrees:
		/// Any adjacent text nodes are joined, and any empty text nodes are removed.
		/// </summary>
		public override void Normalize()
		{
			if (_children == null)
				return;

			ThrowIfReadOnly();

			int dest = 0;
			for (int src = 0; src < _children.Count;)
			{
				Node child = _children[src], next;
				if (child.NodeType == NodeType.Text)
				{
					if (string.IsNullOrEmpty(child.Value))
					{
						child.Parent = null;
						SetRoot(child, null);
						child.Index = -1;
						src++;
					}
					else if (src + 1 < _children.Count && (next = _children[src + 1]).NodeType == NodeType.Text)
					{
						child.Value += next.Value;
						next.Parent = null;
						SetRoot(next, null);
						child.Index = -1;
					}
				}
				else
				{
					child.Normalize();
					_children[dest] = child;
					child.Index = dest;
					src++;
					dest++;
				}
			}

			if (dest < _children.Count)
				_children.RemoveRange(dest, _children.Count - dest);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// Remove all children of this container.
		/// </summary>
		public virtual void Clear()
		{
			if (_children == null)
				return;

			ThrowIfReadOnly();

			for (int i = _children.Count - 1; i >= 0; i--)
			{
				_children[i].Parent = null;
				SetRoot(_children[i], null);
				_children[i].Index = -1;
			}

			_childElementCount = 0;
			UpdateSubtreeCount(-(SubtreeNodeCount - 1),
				this is Element ? -(SubtreeElementCount - 1) : -SubtreeElementCount);

			_children.Clear();

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// Find the index of the given child in this container.  This is like a search,
		/// but takes advantage of the child's data to run in O(1) time.
		/// </summary>
		/// <param name="child">The child to search for.</param>
		/// <returns>The index of the child, or -1 if the child was not found.</returns>
		public int IndexOf(Node child)
			=> ReferenceEquals(child.Parent, this) ? child.Index : -1;

		#endregion

		#region Low-level tree manipulation

		/// <summary>
		/// Replace the 'Root' pointer in the given node and all of its descendants.
		/// </summary>
		/// <param name="node">The top of the local subtree.</param>
		/// <param name="root">The new root pointer for all nodes in this subtree.</param>
		private static void SetRoot(Node node, ContainerNode? root)
		{
			Stack<Node> nodeStack = new Stack<Node>();
			nodeStack.Push(node);

			while (nodeStack.Count > 0)
			{
				Node current = nodeStack.Pop();

				if (root is null && current is Element currentElement && current.Root is IStyleRoot styleRoot)
					styleRoot.StyleQueue.Remove(currentElement);

				current.Root = root;

				if (root is IStyleRoot styleRoot2 && current is Element currentElement2)
					styleRoot2.StyleQueue.Enqueue(currentElement2);

				if (current is ContainerNode container && container._children != null)
				{
					for (int i = container._children.Count - 1; i >= 0; i--)
						nodeStack.Push(container._children[i]);
				}
			}
		}

		/// <summary>
		/// Invalidate all computed styles for all child elements.
		/// </summary>
		public void InvalidateChildComputedStyles()
		{
			foreach (Node node in ChildNodes)
				if (node is Element element)
					element.InvalidateComputedStyle();
		}

		/// <summary>
		/// Update the subtree count in this and all ancestor nodes.
		/// </summary>
		/// <param name="nodeDelta">The amount to add/subtract to each subtree NODE count.</param>
		/// <param name="elementDelta">The amount to add/subtract to each subtree ELEMENT count.</param>
		private void UpdateSubtreeCount(int nodeDelta, int elementDelta)
		{
			for (ContainerNode? node = this; node != null; node = node.Parent as ContainerNode)
			{
				node._subtreeElementCount += elementDelta;
				node._subtreeNodeCount += nodeDelta;
			}
		}

		/// <summary>
		/// Recursively copy all descendants of this node to the given parent, which must
		/// be an empty container.
		/// </summary>
		/// <param name="parent">The parent to copy all of the descendants to.</param>
		protected void CloneDescendantsTo(ContainerNode parent)
		{
			if (parent.Count != 0)
				throw new InvalidOperationException("Cannot clone to a parent with contents.");

			foreach (Node node in ChildNodes)
			{
				Node clone = node.CloneNode(true);
				parent.AppendChildFastAndUnsafe(clone);
			}

#if DEBUG
			VerifyTree(parent);
#endif
		}

#if DEBUG
		private static void VerifyTree(Node? root)
			=> VerifyTree(root, (ContainerNode?)root);

		private static (int nodeCount, int elementCount) VerifyTree(Node? root, Node? node)
		{
			if (root == null)
				throw new InvalidOperationException("Root is null");
			if (node == null)
				throw new InvalidOperationException("Node is null");
			if (node.Root != root)
				throw new InvalidOperationException("Root pointer set incorrectly.");

			int nodeCount = 1;
			int elementCount = node is Element ? 1 : 0;

			if (node is ContainerNode containerNode)
			{
				int childElementCount = 0;
				int count = containerNode.ChildNodes.Count;
				Node? prev = null;
				for (int i = 0; i < count; i++)
				{
					Node child = containerNode.ChildNodes[i];
					if (child.Parent != node)
						throw new InvalidOperationException("Parent pointer set incorrectly.");
					if (child.Index != i)
						throw new InvalidOperationException("Child index set incorrectly.");
					if (child.PreviousSibling != prev)
						throw new InvalidOperationException("Previous pointer set incorrectly.");
					if (prev != null && prev.NextSibling != child)
						throw new InvalidOperationException("Next pointer set incorrectly.");
					prev = child;

					(int childNodes, int childElements) = VerifyTree(root, child);
					nodeCount += childNodes;
					elementCount += childElements;
					childElementCount += (child is Element ? 1 : 0);
				}
				if (count > 0 && containerNode.FirstChild != containerNode.ChildNodes[0])
					throw new InvalidOperationException("First child set incorrectly.");
				if (count > 0 && containerNode.LastChild != containerNode.ChildNodes[^1])
					throw new InvalidOperationException("Last child set incorrectly.");
				if (containerNode.Count != count)
					throw new InvalidOperationException("Count set incorrectly.");
				if (containerNode._childElementCount != childElementCount)
					throw new InvalidOperationException("Child element count set incorrectly.");
			}

			if (nodeCount < elementCount)
				throw new InvalidOperationException("Subtree node count is less than element count.");
			if (nodeCount != node.SubtreeNodeCount)
				throw new InvalidOperationException("Subtree node count set incorrectly.");
			if (elementCount != node.SubtreeElementCount)
				throw new InvalidOperationException("Subtree element count set incorrectly.");

			return (nodeCount, elementCount);
		}
#endif

		#endregion

		#region IList<Node> implementation

		/// <summary>
		/// Append a child node to the end of this container.  If the node is already attached to the
		/// tree, it will be detached first (in O(n) time).  Otherwise, this runs in O(1) for small
		/// containers, O(lg n) for large containers.
		/// </summary>
		/// <param name="child">The child to append.</param>
		/// <exception cref="ArgumentNullException">Thrown if the child is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree.</exception>
		void ICollection<Node>.Add(Node node)
			=> AppendChild(node);

		/// <summary>
		/// Answer whether this collection's children contain the given node.
		/// </summary>
		/// <param name="node">The node to test to see if it is an immediate child of this parent.</param>
		/// <returns>True if the given node is an immediate child of this parent, false if it is not.</returns>
		bool ICollection<Node>.Contains(Node? node)
			=> node is not null && ReferenceEquals(node.Parent, this);

		/// <summary>
		/// Copy the child nodes of this node to the given array.
		/// </summary>
		/// <param name="array">The destination array to copy to.</param>
		/// <param name="arrayIndex">The starting index to copy at.</param>
		void ICollection<Node>.CopyTo(Node[] array, int arrayIndex)
		{
			foreach (Node node in Children)
				array[arrayIndex++] = node;
		}

		/// <summary>
		/// Remove this child from this parent.  Runs in O(n) time.
		/// </summary>
		/// <param name="child">The child to detach.</param>
		/// <returns>True if the child was removed, false if the given node was not a child of this parent.</returns>
		bool ICollection<Node>.Remove(Node? node)
		{
			if (node is null || !ReferenceEquals(node.Parent, this))
				return false;
			RemoveChild(node);
			return true;
		}

		/// <summary>
		/// Whether this container's collection of children can be modified.  Not all containers
		/// allow children to be added (an ImageElement, for example, is an Element, which is a
		/// ContainerNode, but the &lt;img&gt; element does not allow child nodes to exist inside it).
		/// </summary>
		public bool IsReadOnly
		{
			get => (RenderFlags & RenderFlags.ReadOnlyContainer) != 0;

			protected set
			{
				if (value)
					RenderFlags |= RenderFlags.ReadOnlyContainer;
				else
					RenderFlags &= ~RenderFlags.ReadOnlyContainer;
			}
		}

		/// <summary>
		/// Enumerate the children of this node, lazily.
		/// </summary>
		/// <returns>An enumerator that yields the children of this node.</returns>
		/// <remarks>Note: Modifying this container during enumeration will yield unpredictable
		/// results.  Do not modify this container while an enumeration is active.</remarks>
		public IEnumerator<Node> GetEnumerator()
			=> Children.GetEnumerator();

		/// <summary>
		/// Enumerate the children of this node, lazily.
		/// </summary>
		/// <returns>An enumerator that yields the children of this node.</returns>
		/// <remarks>Note: Modifying this container during enumeration will yield unpredictable
		/// results.  Do not modify this container while an enumeration is active.</remarks>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			=> Children.GetEnumerator();

		#endregion

		#region Throw helpers

		/// <summary>
		/// If this container node is marked as ReadOnly, throw an exception.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the container node
		/// cannot be modified.</exception>
		protected void ThrowIfReadOnly()
		{
			if ((RenderFlags & RenderFlags.ReadOnlyContainer) != 0)
				throw new InvalidOperationException("Container node's children are not modifiable.");
		}

		/// <summary>
		/// If the given child-to-be is not valid, throw an exception.
		/// </summary>
		/// <param name="child">The child to test.</param>
		/// <exception cref="ArgumentNullException">Thrown if the new child node is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the child is not a valid type of node to append
		/// (for example, element nodes may only contain TextNodes, CommentNodes, and other Elements).</exception>
		protected void ThrowIfInvalidChild(Node child)
		{
			if (child is null)
				throw new ArgumentNullException(nameof(child), "Child node must not be null.");

			if (!(child is Element
				|| child is TextNode
				|| child is CommentNode))
				throw new ArgumentException(nameof(child), "Every child of a ContainerNode must be an Element, a TextNode, or a CommentNode.");
		}

		/// <summary>
		/// If the given node is not a child of this node, throw an exception.
		/// </summary>
		/// <param name="child">The child to test.</param>
		/// <exception cref="HierarchyException">Thrown if the given node is not a child of this container.</exception>
		protected void ThrowIfWrongParent(Node child)
		{
			if (!ReferenceEquals(child.Parent, this))
				throw new HierarchyException("Reference node is not a child of this container.");
		}

		/// <summary>
		/// Test to see whether adding the given node as a child would create a cycle in the
		/// tree.  If so, throw an exception.
		/// </summary>
		/// <param name="child">The child to test.</param>
		/// <exception cref="HierarchyException">Thrown if this insertion would create a cycle in the tree.</exception>
		protected void ThrowIfCycle(Node child)
		{
			if (ReferenceEquals(child, this) || child.ContainsOrIs(this))
				throw new HierarchyException("Cannot create cycle in DOM tree.");
		}

		/// <summary>
		/// If the given index is not within the range of valid indices of child nodes of
		/// this container, throw an exception.
		/// </summary>
		/// <param name="index">The index to test.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not in range.</exception>
		protected void ThrowIfIndexOutOfRange(int index)
		{
			if (index < 0 || index >= (_children?.Count ?? 0))
				throw new ArgumentOutOfRangeException(nameof(index), "Child node index is out of range.");
		}

		#endregion
	}
}
