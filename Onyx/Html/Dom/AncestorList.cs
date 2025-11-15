using System.Collections;
using System.Runtime.CompilerServices;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A collection of reverse-order ancestors of a given node.  This is used
	/// during ComputeDocumentPosition and similar methods to compare ancestor branches
	/// as efficiently as possible, but it's not really intended for general use.
	/// </summary>
	internal class AncestorList : IReadOnlyList<Node>
	{
		private Node[] _nodes = new Node[64];

		/// <summary>
		/// The number of nodes in this list.
		/// </summary>
		public int Count => _count;
		private int _count = 0;

		/// <summary>
		/// Insert a node at the *head* of the list.  Runs in amortized O(1) time.
		/// </summary>
		/// <param name="node">The node to insert at the start.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(Node node)
		{
			if (_count >= _nodes.Length)
				Grow();
			_nodes[_nodes.Length - ++_count] = node;
		}

		/// <summary>
		/// Retrieve a node at the given index, relative to the start of the list.
		/// Runs in theta(1) time.
		/// </summary>
		/// <param name="index">The index of the node to retrieve.</param>
		/// <returns>The node at that position.</returns>
		public Node this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _nodes[_nodes.Length - _count + index];
		}

		/// <summary>
		/// Remove all nodes in the list.  Runs in theta(1) time.
		/// </summary>
		public void Clear()
			=> _count = 0;

		/// <summary>
		/// Expand the list, in place.  Runs in O(n) time.
		/// </summary>
		private void Grow()
		{
			Node[] newNodes = new Node[_nodes.Length * 2];
			_nodes.CopyTo(newNodes, _nodes.Length);
			_nodes = newNodes;
		}

		/// <summary>
		/// Get an enumerator that can walk the list.  This enumerator is only valid
		/// as long as the list is not being modified.
		/// </summary>
		/// <returns>An enumerator that can lazily yield the list.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<Node> GetEnumerator()
			=> new AncestorListEnumerator(this);

		/// <summary>
		/// Get an enumerator that can walk the list.  This enumerator is only valid
		/// as long as the list is not being modified.
		/// </summary>
		/// <returns>An enumerator that can lazily yield the list.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> new AncestorListEnumerator(this);

		/// <summary>
		/// An enumerator that can lazily yield the list.
		/// </summary>
		private struct AncestorListEnumerator : IEnumerator<Node>
		{
			private readonly AncestorList _list;
			private int _index = -1;

			public AncestorListEnumerator(AncestorList list)
				=> _list = list;

			public Node Current => _list[_index];

			object IEnumerator.Current => _list[_index];

			public void Dispose() { }

			public bool MoveNext()
				=> ++_index < _list.Count;

			public void Reset()
				=> _index = -1;
		}
	}
}
