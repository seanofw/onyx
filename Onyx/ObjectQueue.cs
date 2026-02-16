using System.Collections;

namespace Onyx
{
	/// <summary>
	/// An ordered queue of items where each item will be processed in the
	/// order given.  This is like Queue{T}, but this will not double-add entries,
	/// and entries may be removed early.
	/// </summary>
	/// <typeparam name="T">The type of each item in the queue.</typeparam>
	public class ObjectQueue<T> : IReadOnlyCollection<T>
		where T : notnull
	{
		/// <summary>
		/// The queue is made out of nodes allocated from an array.
		/// </summary>
		private struct Node
		{
			public T? Item;
			public int Older;
			public int Newer;
		}

		/// <summary>
		/// The actual queue is a linked list stored inside an array.
		/// </summary>
		private Node[] _queue = new Node[16];

		/// <summary>
		/// Index of the first free node in the queue array, if any.
		/// </summary>
		private int _firstFree = -1;

		/// <summary>
		/// The first node in the queue array that is to be dequeued.
		/// </summary>
		private int _queueOldest = -1;

		/// <summary>
		/// The tail of the queue, the last enqueued entry.
		/// </summary>
		private int _queueNewest = -1;

		/// <summary>
		/// A dictionary mapping items to array positions.
		/// </summary>
		private readonly Dictionary<T, int> _dict = new Dictionary<T, int>();

		/// <summary>
		/// The number of entries currently in the queue.
		/// </summary>
		public int Count => _dict.Count;

		/// <summary>
		/// Construct a new queue.
		/// </summary>
		public ObjectQueue()
		{
			for (int i = 0; i < _queue.Length; i++)
				Free(i);
		}

		/// <summary>
		/// Enqueue the given object for processing.  If the object is already in the
		/// queue, it will not be moved from its restyle position.
		/// </summary>
		/// <param name="item">The item to process.</param>
		public void Enqueue(T item)
		{
			int index = Alloc();

			AttachAtTail(item, index);

			_dict[item] = index;
		}

		/// <summary>
		/// Remove the given item from the queue.  Even though it was previously
		/// enqueued, it is now assumed to have already been given a valid style.
		/// </summary>
		/// <param name="item">The item that no longer needs to be restyled.</param>
		public bool Remove(T item)
		{
			if (!_dict.TryGetValue(item, out int index))
				return false;

			if (index < 0 || index >= _queue.Length)
				throw new InvalidOperationException("Internal error in object queue.");

			Detach(index);

			Free(index);

			return true;
		}

		/// <summary>
		/// Attach a new entry at the tail of the queue.
		/// </summary>
		/// <param name="item">The item to put in the allocated entry.</param>
		/// <param name="index">The index of the allocated entry.</param>
		private void AttachAtTail(T item, int index)
		{
			_queue[index].Item = item;
			_queue[index].Older = -1;
			_queue[index].Newer = _queueNewest;

			if (_queueNewest >= 0)
				_queue[_queueNewest].Older = index;
			else
				_queueOldest = index;

			_queueNewest = index;
		}

		/// <summary>
		/// Detach an entry in the queue.
		/// </summary>
		/// <param name="index">The index of the entry to detach.</param>
		private void Detach(int index)
		{
			int older = _queue[index].Older;
			int newer = _queue[index].Newer;

			if (older >= 0)
				_queue[older].Newer = newer;
			else
				_queueOldest = newer;

			if (newer >= 0)
				_queue[newer].Older = older;
			else
				_queueNewest = older;
		}

		/// <summary>
		/// Allocate an unused entry from the array.  If the array is full, grow the array.
		/// </summary>
		/// <returns>The index of the allocated entry.</returns>
		private int Alloc()
		{
			if (_firstFree < 0)
			{
				int oldLength = _queue.Length;
				Node[] newQueue = new Node[oldLength * 2];
				Array.Copy(_queue, newQueue, oldLength);

				_queue = newQueue;

				for (int i = oldLength; i < oldLength * 2; i++)
					Free(i);
			}

			int index = _firstFree;
			_firstFree = _queue[index].Older;

			return index;
		}

		/// <summary>
		/// Blank out the given entry and then add it to the free list.
		/// </summary>
		/// <param name="index">The index of the entry to free.</param>
		private void Free(int index)
		{
			_queue[index].Older = _firstFree;
			_queue[index].Newer = -1;
			_queue[index].Item = default;

			_firstFree = index;
		}

		/// <summary>
		/// Remove the next item from the queue so it can be restyled.
		/// </summary>
		/// <returns>The next item to restyle.</returns>
		public T? TryDequeue()
		{
			if (_queueOldest < 0)
				return default;

			int index = _queueOldest;

			T? item = _queue[index].Item;
			Detach(index);
			Free(index);

			return item;
		}

		/// <summary>
		/// Enumerate the items currently in the queue, in order of oldest to newest.
		/// </summary>
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = _queueNewest; i >= 0; i = _queue[i].Newer)
				yield return _queue[i].Item!;
		}

		/// <summary>
		/// Enumerate the items currently in the queue.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
