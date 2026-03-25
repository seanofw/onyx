using System.Runtime.CompilerServices;

namespace Onyx.Html.Parsing
{
	/// <summary>
	/// A stack of nodes, used to track which elements have been opened (via start tags)
	/// but not yet closed (via end tags).
	/// </summary>
	internal ref struct NodeStack<T>
	{
		#region Private state

		/// <summary>
		/// The shape of each entry in the node stack.
		/// </summary>
		public struct Entry
		{
			public readonly T Node;
			public readonly string Name;
			public readonly SourceLocation SourceLocation;
			public readonly int Depth;

			public bool IsNull => Name == null!;

			public Entry(T node, string name, SourceLocation sourceLocation, int depth)
			{
				Node = node;
				Name = name;
				SourceLocation = sourceLocation;
				Depth = depth;
			}
		}

		/// <summary>
		/// The actual stack itself.
		/// </summary>
		private Entry[] _nodeStack;

		#endregion

		#region Public properties

		/// <summary>
		/// The number of container nodes currently sitting on the stack.
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// The topmost entry, which should never be null.
		/// </summary>
		public Entry Current { get; private set; }

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
			_nodeStack = new Entry[size];
			Current = default!;
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
			get => _nodeStack[index].Node;
		}

		/// <summary>
		/// Push a new node onto the top of the stack.  This will updated CurrentNode
		/// to point at the new top of the stack.
		/// </summary>
		/// <param name="node">The node to push onto the stack.</param>
		/// <param name="name">The name of that node.</param>
		/// <param name="sourceLocation">The source location where that node is found.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PushNode(T node, string name, SourceLocation sourceLocation)
		{
			if (Count >= _nodeStack.Length)
			{
				Entry[] newStack = new Entry[_nodeStack.Length * 2];
				_nodeStack.AsSpan().CopyTo(newStack);
				_nodeStack = newStack;
			}
			Current = _nodeStack[Count] = new Entry(node, name, sourceLocation, Count);
			Count++;
		}

		/// <summary>
		/// Pop the topmost node from the stack and discard it.  This will updated CurrentNode
		/// to point at the new top of the stack.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PopNode()
		{
			Current = _nodeStack[--Count - 1];
		}

		/// <summary>
		/// Remove all nodes from the stack.  This does not deallocate the stack, but
		/// runs in O(1) time to quickly clear nodes.
		/// </summary>
		public void Clear()
		{
			Count = 0;
			Current = default!;
		}

		/// <summary>
		/// Search downward from the top of the stack for a matching node from a small
		/// set of nodes.  This returns the first matching node.
		/// </summary>
		/// <param name="searchFor">The set of node names (types) to search for.</param>
		/// <returns>The first matching ancestor, if any, or null if none is found.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Entry FindAncestor(string[] searchFor)
		{
			for (int i = Count - 1; i >= 0; i--)
				if (searchFor.Contains(_nodeStack[i].Name))
					return _nodeStack[i];

			return default;
		}

		#endregion
	}
}
