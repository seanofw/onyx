using System.Runtime.CompilerServices;
using Onyx.Html.Dom;

namespace Onyx.Html.Parsing
{
	/// <summary>
	/// A stack of nodes, used to track which elements have been opened (via start tags)
	/// but not yet closed (via end tags).
	/// </summary>
	internal ref struct NodeStack<T>
		where T : Node
	{
		#region Private state

		/// <summary>
		/// The actual stack itself.
		/// </summary>
		private T[] _nodeStack;

		#endregion

		#region Public properties

		/// <summary>
		/// The number of container nodes currently sitting on the stack.
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// The topmost node, which should never be null.
		/// </summary>
		public T CurrentNode { get; private set; }

		#endregion

		#region Construction

		/// <summary>
		/// Construct a new NodeStack.
		/// </summary>
		/// <param name="size">The initial capacity of the NodeStack.  This must
		/// not be zero or negative.  The NodeStack can grow more beyond this, but
		/// a sufficiently large value here can avoid reallocations.</param>
		public NodeStack(int size)
		{
			if (size < 1)
				throw new ArgumentOutOfRangeException(nameof(size));
			_nodeStack = new T[size];
			CurrentNode = null!;
			Count = 0;
		}

		#endregion

		#region Public API

		/// <summary>
		/// Access an entry in the NodeStack by position.
		/// </summary>
		/// <param name="index">The index of the entry to access, from oldest (0) to newest (Count-1).</param>
		/// <returns>The node at that level.</returns>
		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _nodeStack[index];
		}

		/// <summary>
		/// Push a new node onto the top of the stack.  This will updated CurrentNode
		/// to point at the new top of the stack.
		/// </summary>
		/// <param name="node">The node to push onto the stack.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PushNode(T node)
		{
			if (Count >= _nodeStack.Length)
			{
				T[] newStack = new T[_nodeStack.Length * 2];
				_nodeStack.AsSpan().CopyTo(newStack);
				_nodeStack = newStack;
			}
			_nodeStack[Count++] = node;
			CurrentNode = node;
		}

		/// <summary>
		/// Pop the topmost node from the stack and discard it.  This will updated CurrentNode
		/// to point at the new top of the stack.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PopNode()
		{
			CurrentNode = _nodeStack[--Count - 1];
		}

		/// <summary>
		/// Remove all nodes from the stack.  This does not deallocate the stack, but
		/// runs in O(1) time to quickly clear nodes.
		/// </summary>
		public void Clear()
		{
			Count = 0;
			CurrentNode = null!;
		}

		/// <summary>
		/// Search downward from the top of the stack for a matching node from a small
		/// set of nodes.  This returns the first matching node.
		/// </summary>
		/// <param name="searchFor">The set of node names (types) to search for.</param>
		/// <returns>The first matching ancestor, if any, or null if none is found.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T? FindAncestor(string[] searchFor)
		{
			for (int i = Count - 1; i >= 0; i--)
				if (searchFor.Contains(_nodeStack[i].NodeName))
					return _nodeStack[i];

			return null;
		}

		#endregion
	}
}
