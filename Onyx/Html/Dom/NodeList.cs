using System.Collections;
using System.Collections.Immutable;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// This is the internal representation of a "node list," and is used to represent
	/// the DOM.  It avoids allocating for empty lists; uses a fixed-size array for small
	/// lists; and switches to an AVL tree for efficient mutation of big lists.  The
	/// cutoff point is at 8 nodes: Above that, we switch to the AVL tree, but below that
	/// we prefer a simple array.
	/// </summary>
	/// <typeparam name="T">The type of the nodes in this node list.</typeparam>
	internal class NodeList<T> : IList<T>, IReadOnlyList<T>
		where T : Node
	{
		public const int SmallListLimit = 8;

		private ImmutableList<T>? _bigList;
		private T[]? _smallList;

		public bool IsReadOnly => false;

		public int Count { get; private set; }

		private ArgumentNullException NullNode
			=> new ArgumentNullException("node", "Cannot insert null into a NodeList");

		public T this[int index]
		{
			get
			{
				if (index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				if (_smallList != null)
					return _smallList[index];
				else if (_bigList != null)
					return _bigList[index];
				else
					throw new ArgumentOutOfRangeException(nameof(index));
			}

			set
			{
				if (index >= Count)
					throw new ArgumentOutOfRangeException(nameof(index));

				if (_smallList != null)
					_smallList[index] = value ?? throw NullNode;
				else if (_bigList != null)
					_bigList = _bigList.SetItem(index, value);
				else
					throw new ArgumentOutOfRangeException(nameof(index));
			}
		}

		public NodeList()
		{
		}

		public NodeList(IEnumerable<T> nodes)
		{
			AddRange(nodes);
		}

		public void AddRange(IEnumerable<T> nodes)
		{
			if (nodes is IReadOnlyCollection<T> collection
				&& collection.Count + Count >= SmallListLimit)
			{
				// Pre-switch to a big list, because we're going to be adding a lot of items.
				if (_smallList != null)
				{
					_bigList = ImmutableList<T>.Empty.AddRange(_smallList.AsSpan().Slice(0, Count).ToArray());
					_smallList = null;
				}
				else if (_bigList == null)
					_bigList = ImmutableList<T>.Empty;
			}

			foreach (T node in nodes)
				Add(node);
		}

		public void Add(T node)
		{
			if (_smallList != null)
			{
				if (Count < _smallList.Length)
				{
					_smallList[Count++] = node ?? throw NullNode;
				}
				else
				{
					_bigList = ImmutableList<T>.Empty.AddRange(_smallList).Add(node);
					_smallList = null;
					Count++;
				}
			}
			else if (_bigList != null)
			{
				_bigList = _bigList.Add(node ?? throw NullNode);
				Count++;
			}
			else
			{
				_smallList = new T[SmallListLimit];
				_smallList[0] = node ?? throw NullNode;
				Count++;
			}
		}

		public int IndexOf(T node)
		{
			if (_smallList != null)
				return ((IList<T>)_smallList).IndexOf(node);
			else if (_bigList != null)
				return _bigList.IndexOf(node);
			else
				return -1;
		}

		public void Insert(int index, T node)
		{
			if (node is null)
				throw NullNode;
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than or equal to 0 and less than or equal to the current count of nodes.");

			if (index == Count)
			{
				Add(node);
				return;
			}

			if (_smallList != null)
			{
				if (Count + 1 >= _smallList.Length)
				{
					// Small list is full, so reallocate it as a big list, then insert into the big list.
					_bigList = ImmutableList<T>.Empty.AddRange(_smallList).Insert(index, node);
					_smallList = null;
				}
				else
				{
					// Shuffle the elements over to make room.
					_smallList.AsSpan().Slice(index, Count - index).CopyTo(_smallList.AsSpan().Slice(index + 1));
					_smallList[index] = node;
				}
				Count++;
			}
			else if (_bigList != null)
			{
				_bigList = _bigList.Insert(index, node);
				Count++;
			}
			else throw new InvalidOperationException("NodeList contains inconsistent state and is damaged.");
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= Count)
				throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than or equal to 0 and less than to the current count of nodes.");

			if (_smallList != null)
			{
				if (index + 1 < Count)
				{
					_smallList.AsSpan().Slice(index + 1).CopyTo(_smallList.AsSpan().Slice(index));
					_smallList[Count - 1] = null!;
				}
				Count--;
			}
			else if (_bigList != null)
			{
				_bigList = _bigList.RemoveAt(index);
				if (--Count <= SmallListLimit / 2)
				{
					// Clearly we've shrunk a lot from whatever "big" was, so replace the
					// big list with a small list.
					_smallList = _bigList.ToArray();
					_bigList = null;
				}
			}
			else throw new InvalidOperationException("NodeList contains inconsistent state and is damaged.");
		}

		public void RemoveRange(int start, int count)
		{
			if (start < 0 || start >= Count)
				throw new ArgumentOutOfRangeException(nameof(start), "Start must be greater than or equal to 0 and less than to the current count of nodes.");
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), "Count of nodes to remove must not be negative.");
			if (start + count > Count)
				throw new ArgumentOutOfRangeException(nameof(count), "Cannot remove nodes outside of the list.");

			if (_smallList != null)
			{
				if (start + count < Count)
				{
					_smallList.AsSpan().Slice(start + count).CopyTo(_smallList.AsSpan().Slice(start));
					for (int i = Count - 1; i >= Count - count; i--)
						_smallList[i] = null!;
				}
				Count -= count;
			}
			else if (_bigList != null)
			{
				_bigList = _bigList.RemoveRange(start, count);
				Count -= count;
				if (Count <= SmallListLimit / 2)
				{
					// Clearly we've shrunk a lot from whatever "big" was, so replace the
					// big list with a small list.
					_smallList = _bigList.ToArray();
					_bigList = null;
				}
			}
			else throw new InvalidOperationException("NodeList contains inconsistent state and is damaged.");
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index < 0)
				return false;
			RemoveAt(index);
			return true;
		}

		public void Clear()
		{
			_smallList = null;
			_bigList = null;
		}

		public bool Contains(T item)
			=> IndexOf(item) >= 0;

		public void CopyTo(T[] array, int arrayIndex)
		{
			if (_smallList != null)
				_smallList.AsSpan().CopyTo(array.AsSpan().Slice(arrayIndex));
			else if (_bigList != null)
				_bigList.CopyTo(array, arrayIndex);
		}

		private struct ArraySliceEnumerator : IEnumerator<T>, IEnumerator
		{
			private readonly T[] _array;
			private int _count;
			private int _index;

			public ArraySliceEnumerator(T[] array, int count)
			{
				_array = array;
				_count = count;
				_index = -1;
			}

			public T Current => _array[_index];

			object IEnumerator.Current => Current;

			public void Dispose()
			{
			}

			public bool MoveNext()
				=> ++_index < _count;

			public void Reset()
				=> _index = -1;
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (_smallList != null)
				return new ArraySliceEnumerator(_smallList, Count);
			else if (_bigList != null)
				return _bigList.GetEnumerator();
			else
				return ((IEnumerable<T>)Array.Empty<T>()).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
