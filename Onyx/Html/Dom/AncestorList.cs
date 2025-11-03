using System.Collections;
using System.Runtime.CompilerServices;

namespace Onyx.Html.Dom
{
	internal class AncestorList : IReadOnlyList<Node>
	{
		private Node[] _nodes = new Node[64];

		public int Count => _count;
		private int _count = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Insert(Node node)
		{
			if (_count >= _nodes.Length)
				Grow();
			_nodes[_nodes.Length - ++_count] = node;
		}

		public Node this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _nodes[_nodes.Length - _count + index];
		}

		public void Clear()
			=> _count = 0;

		private void Grow()
		{
			Node[] newNodes = new Node[_nodes.Length * 2];
			_nodes.CopyTo(newNodes, _nodes.Length);
			_nodes = newNodes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<Node> GetEnumerator()
			=> new AncestorListEnumerator(this);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
			=> new AncestorListEnumerator(this);

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
