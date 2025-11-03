using Onyx.Css;
using Onyx.Css.Computed;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A "container node" is a base class for all nodes that can potentially contain
	/// children (including Element and Document).  This implements all of the necessary
	/// DOM-like mechanics and events for a Node to really be part of a tree.
	/// </summary>
	public abstract class ContainerNode : Node
	{
		#region Tree references

		/// <summary>
		/// The children of this container, as an immutable list.  (Use methods on this
		/// node to modify it.)
		/// </summary>
		public override IReadOnlyList<Node> ChildNodes => _children ?? (IReadOnlyList<Node>)Array.Empty<Node>();

		/// <summary>
		/// The children of this container, as an immutable list.  (Use methods on this
		/// node to modify it.)
		/// </summary>
		public IReadOnlyList<Node> Children => _children ?? (IReadOnlyList<Node>)Array.Empty<Node>();

		/// <summary>
		/// The actual internal storage of the children, as a list whose internal storage
		/// shape will vary depending on how many children there are.
		/// </summary>
		private NodeList<Node>? _children;

		/// <summary>
		/// The first child of this container, as a direct reference.
		/// </summary>
		public override Node? FirstChild => _firstChild;
		private Node? _firstChild;

		/// <summary>
		/// The last child of this container, as a direct reference.
		/// </summary>
		public override Node? LastChild => _lastChild;
		private Node? _lastChild;

		/// <summary>
		/// The number of child nodes contained by this container.  (This is children,
		/// not descendants.)
		/// </summary>
		public int Count => _children?.Count ?? 0;

		#endregion

		#region High-level collection-modification functions

		/// <summary>
		/// Access this container's children as an ordinary C# list.
		/// </summary>
		/// <param name="index">The index of the child to retrieve or replace.  This must
		/// be a valid index within the collection.</param>
		/// <returns>The current child at the given index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the index is not
		/// within the range of children of this container.</exception>
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
		/// Append a child node to the end of this container.  If the node is already attached to the
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
			if (child is null)
				throw new ArgumentNullException(nameof(child), "Child node must not be null.");

			if (!(child is Element
				|| child is TextNode
				|| child is CommentNode))
				throw new ArgumentException(nameof(child), "Every child of a ContainerNode must be an Element, a TextNode, or a CommentNode.");

			if (ReferenceEquals(child, this) || child.ContainsOrIs(this))
				throw new HierarchyException("Cannot create cycle in DOM tree.");

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

			AttachAtEnd(child);
			SetRoot(child, Root);
			child.Index = _children.Count - 1;

			ChildElementCount += (child is Element ? +1 : 0);
			UpdateSubtreeCount(child.SubtreeNodeCount, child.SubtreeElementCount);

			Node.OnAttached(this, child);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// Return whether this has child nodes (O(1) lookup).
		/// </summary>
		/// <returns>True if this has child nodes, false if it does not.</returns>
		public override bool HasChildNodes()
			=> _children?.Count != 0;

		/// <summary>
		/// Insert the given node before another reference node that is known to already be a child
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

			if (newNode is null)
				throw new ArgumentNullException(nameof(newNode), "Child node must not be null.");

			if (!(newNode is Element
				|| newNode is TextNode
				|| newNode is CommentNode))
				throw new ArgumentException(nameof(newNode), "Every child of a ContainerNode must be an Element, a TextNode, or a CommentNode.");

			if (ReferenceEquals(newNode, this) || newNode.ContainsOrIs(this))
				throw new HierarchyException("Cannot create cycle in DOM tree.");

			if (!ContainsOrIs(referenceNode))
				throw new HierarchyException("Reference node is not a child of this container.");

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

			if (newNode is null)
				throw new ArgumentNullException(nameof(newNode), "Child node must not be null.");

			if (!(newNode is Element
				|| newNode is TextNode
				|| newNode is CommentNode))
				throw new ArgumentException(nameof(newNode), "Every child of a ContainerNode must be an Element, a TextNode, or a CommentNode.");

			if (ReferenceEquals(newNode, this) || newNode.ContainsOrIs(this))
				throw new HierarchyException("Cannot create cycle in DOM tree.");

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

			AttachBefore(newNode, referenceNode);
			SetRoot(newNode, Root);
			ChildElementCount += (newNode is Element ? +1 : 0);
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
		/// Remove this child from this parent.  Runs in O(n) time.
		/// </summary>
		/// <param name="child">The child to detach.</param>
		/// <exception cref="ArgumentNullException">Thrown if the child node is null.</exception>
		/// <exception cref="HierarchyException">Thrown if the given node is not a child of this container.</exception>
		public override void RemoveChild(Node child)
		{
			if (child is null)
				throw new ArgumentNullException(nameof(child), "Child node must not be null.");

			if (!ReferenceEquals(child.Parent, this))
				throw new HierarchyException("Node does not have this container node as its parent.");

			Node.OnDetaching(this, child);

			if (_children != null)
			{
				_children.Remove(child);

				if (_children.Count == 0)
					_children = null;
			}

			Detach(child);
			SetRoot(child, null);
			UpdateIndexesOnAndAfter(child.Index);
			ChildElementCount += (child is Element ? -1 : 0);
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
			if (index < 0 || index >= (_children?.Count ?? 0))
				throw new ArgumentOutOfRangeException(nameof(index), "Child node index is out of range.");

			Node child = _children![index];

			Node.OnDetaching(this, child);

			_children.RemoveAt(index);

			if (_children.Count == 0)
				_children = null;

			UpdateIndexesOnAndAfter(index);
			ChildElementCount += (child is Element ? -1 : 0);
			UpdateSubtreeCount(child.SubtreeNodeCount, child.SubtreeElementCount);
			child.Index = -1;

			Node.OnDetached(this, child);

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// Remove the given reference node from this container's children, and insert the given
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

			if (newNode is null)
				throw new ArgumentNullException(nameof(newNode), "Child node must not be null.");

			if (!(newNode is Element
				|| newNode is TextNode
				|| newNode is CommentNode))
				throw new ArgumentException(nameof(newNode), "Every child of a ContainerNode must be an Element, a TextNode, or a CommentNode.");

			if (ReferenceEquals(newNode, this) || newNode.ContainsOrIs(this))
				throw new HierarchyException("Cannot create cycle in DOM tree.");

			if (!ContainsOrIs(referenceNode))
				throw new HierarchyException("Reference node is not a child of this container.");

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
			if (newNode is null)
				throw new ArgumentNullException(nameof(newNode), "Child node must not be null.");

			if (!(newNode is Element
				|| newNode is TextNode
				|| newNode is CommentNode))
				throw new ArgumentException(nameof(newNode), "Every child of a ContainerNode must be an Element, a TextNode, or a CommentNode.");

			if (ReferenceEquals(newNode, this) || newNode.ContainsOrIs(this))
				throw new HierarchyException("Cannot create cycle in DOM tree.");

			if (_children == null)
				throw new ArgumentOutOfRangeException(nameof(index), "Index is not within the range of valid nodes.");

			if (newNode.Parent is not null)
				newNode.Parent.RemoveChild(newNode);

			ReplaceChildFastAndUnsafe(newNode, _children[index]);
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
			AttachBefore(newNode, referenceNode);
			Detach(referenceNode);
			SetRoot(newNode, Root);
			newNode.Index = referenceNode.Index;
			SetRoot(referenceNode, null);
			ChildElementCount += (referenceNode is Element ? -1 : 0) + (newNode is Element ? +1 : 0);
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
		/// Normalize text nodes in this container and in all deeper subtrees:
		/// Any adjacent text nodes are joined, and any empty text nodes are removed.
		/// </summary>
		public override void Normalize()
		{
			if (_children == null)
				return;

			int dest = 0;
			for (int src = 0; src < _children.Count; )
			{
				Node child = _children[src], next;
				if (child.NodeType == NodeType.Text)
				{
					if (string.IsNullOrEmpty(child.Value))
					{
						Detach(child);
						SetRoot(child, null);
						child.Index = -1;
						src++;
					}
					else if (src + 1 < _children.Count && (next = _children[src + 1]).NodeType == NodeType.Text)
					{
						child.Value += next.Value;
						Detach(next);
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

			for (int i = _children.Count - 1; i >= 0; i--)
			{
				Detach(_children[i]);
				SetRoot(_children[i], null);
				_children[i].Index = -1;
			}

			ChildElementCount = 0;
			UpdateSubtreeCount(-(SubtreeNodeCount - 1),
				this is Element ? -(SubtreeElementCount - 1) : -SubtreeElementCount);

			_children.Clear();

#if DEBUG
			VerifyTree(Root);
#endif
		}

		/// <summary>
		/// Find the index of the child in this container.
		/// </summary>
		/// <param name="child">The child to search for.</param>
		/// <returns>The index of the child, or -1 if the child was not found.</returns>
		public int IndexOf(Node child)
			=> child.Parent == this ? child.Index : -1;

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
			for (Node? node = this; node != null; node = node.Parent)
			{
				node.SubtreeElementCount += elementDelta;
				node.SubtreeNodeCount += nodeDelta;
			}
		}

		/// <summary>
		/// Attach the given child at the end of the container.  This *only* updates
		/// the relative pointers, and does *not* update the _children list.  This does
		/// not perform any safety or correctness checks, and runs in O(1) time.
		/// </summary>
		/// <param name="child">The child to append, which must not be null.</param>
		private void AttachAtEnd(Node child)
		{
			child.Parent = this;

			if ((child.Previous = _lastChild) != null)
				child.Previous.Next = child;
			else
				_firstChild = child;

			_lastChild = child;

			child.Next = null;
		}

		/// <summary>
		/// Attach the given child before the given existing child of the container.  This
		/// *only* updates the relative pointers, and does *not* update the _children list.
		/// This does not perform any safety or correctness checks, and runs in O(1) time.
		/// </summary>
		/// <param name="newChild">The child to append, which must not be null.</param>
		/// <param name="existingChild">The existing child to insert the new child before,
		/// which must not be null.</param>
		private void AttachBefore(Node newChild, Node existingChild)
		{
			newChild.Parent = this;

			newChild.Next = existingChild;
			if (newChild.Next != null)
				newChild.Next.Previous = newChild;
			else
				_lastChild = newChild;

			newChild.Previous = existingChild.Previous;
			if (newChild.Previous != null)
				newChild.Previous.Next = newChild;
			else
				_firstChild = newChild;
		}

		/// <summary>
		/// Detach the given child from the container.  This *only* updates the relative
		/// pointers, and does *not* update the _children list.  This does not perform any
		/// safety or correctness checks, and runs in O(1) time.
		/// </summary>
		/// <param name="child">The child to detach.  This must not be null.</param>
		private void Detach(Node child)
		{
			if (child.Next != null)
				child.Next.Previous = child.Previous;
			else
				_lastChild = child.Previous;

			if (child.Previous != null)
				child.Previous.Next = child.Next;
			else
				_firstChild = child.Next;

			child.Parent = null;
			child.Next = null;
			child.Previous = null;
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
					if (child.Previous != prev)
						throw new InvalidOperationException("Previous pointer set incorrectly.");
					if (prev != null && prev.Next != child)
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
				if (containerNode.ChildElementCount != childElementCount)
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
	}
}
