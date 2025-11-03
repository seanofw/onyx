using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Onyx
{
	/// <summary>
	/// A cache, with a cache-size limit by item count, with an LRU replacement policy, so
	/// that recently-used items stay in the cache, and infrequently-used items drop out.
	/// There are lots of cache classes in the world, but by including this in Onyx, we
	/// avoid taking a dependency on an external library (dependencies can cause plenty
	/// of trouble at linking/loading time).
	/// </summary>
	/// <typeparam name="K">The key type for each cache entry.</typeparam>
	/// <typeparam name="V">The value type for each cache entry.</typeparam>
	public class Cache<K, V> : IDictionary<K, V>
		where K : notnull
	{
		#region Private fields

		private struct ListEntry
		{
			public K Key;
			public V Value;
			public int Next;
			public int Prev;
		}

		private ListEntry[] _entries = new ListEntry[16];
		private int _head = -1, _tail = -1;
		private int _firstFree = 0;

		private readonly Dictionary<K, int> _lookup = new Dictionary<K, int>();

		#endregion

		#region Public properties

		public int Limit => _limit;
		private readonly int _limit;

		public int Count => _lookup.Count;

		public bool IsReadOnly => false;

		#endregion

		#region Construction

		public Cache(int limit)
		{
			_limit = limit;

			for (int i = 0; i < _entries.Length - 1; i++)
				_entries[i].Next = i + 1;

			_entries[_entries.Length - 1].Next = -1;
		}

		#endregion

		#region Dictionary-style API

		public V this[K key]
		{
			get
			{
				int index = _lookup[key];
				Raise(index);
				return _entries[index].Value;
			}

			set => AddOrUpdate(key, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ContainsKey(K key)
			=> _lookup.ContainsKey(key);

		public V GetOrAdd(K key, Func<K, V> factory)
		{
			if (Lookup(key, out int index))
				return _entries[index].Value;

			V value = factory(key);
			Add(key, value);
			return value;
		}

		public V GetOrAdd(K key, V value)
		{
			if (Lookup(key, out int index))
				return _entries[index].Value;

			Add(key, value);
			return value;
		}

		public void Add(K key, V value)
		{
			if (_firstFree < 0)
				Grow();

			int index = _firstFree;

			_lookup.Add(key, index);

			_firstFree = _entries[index].Next;

			AttachAtHead(index);

			_entries[index].Key = key;
			_entries[index].Value = value;

			if (_lookup.Count > _limit)
				Evict();
		}

		public void AddOrUpdate(K key, V newValue)
		{
			if (_lookup.TryGetValue(key, out int index))
			{
				Raise(index);
				_entries[index].Value = newValue;
				return;
			}

			Add(key, newValue);
		}

		public void AddOrUpdate(K key, Func<K, V> create, Func<K, V, V> update)
		{
			if (_lookup.TryGetValue(key, out int index))
			{
				Raise(index);
				_entries[index].Value = update(key, _entries[index].Value);
				return;
			}

			V value = create(key);
			Add(key, value);
		}

		public bool Remove(K key)
		{
			if (!_lookup.TryGetValue(key, out int index))
				return false;

			RemoveInternal(index, key);
			return true;
		}

		public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
		{
			if (!_lookup.TryGetValue(key, out int index))
			{
				value = default(V);
				return false;
			}

			Raise(index);
			value = _entries[index].Value;
			return true;
		}

		public V GetValueOrDefault(K key, V defaultValue = default!)
		{
			if (!_lookup.TryGetValue(key, out int index))
				return defaultValue;

			Raise(index);
			return _entries[index].Value;
		}

		public void Clear()
		{
			for (int i = 0; i < _entries.Length - 1; i++)
			{
				_entries[i].Next = i + 1;
				_entries[i].Key = default!;
				_entries[i].Value = default!;
			}

			_entries[_entries.Length - 1].Next = -1;
			_entries[_entries.Length - 1].Key = default!;
			_entries[_entries.Length - 1].Value = default!;

			_firstFree = 0;

			_head = _tail = -1;
		}

		#endregion

		#region Keys/Values collections

		private struct LightweightKeysCollection : ICollection<K>
		{
			private readonly Cache<K, V> _cache;

			public int Count => _cache.Count;

			public bool IsReadOnly => false;

			public LightweightKeysCollection(Cache<K, V> cache)
				=> _cache = cache;

			public bool Contains(K key)
				=> _cache._lookup.ContainsKey(key);

			public void CopyTo(K[] array, int arrayIndex)
			{
				for (int index = _cache._head; index >= 0; index = _cache._entries[index].Next)
					array[arrayIndex++] = _cache._entries[index].Key;
			}

			public IEnumerator<K> GetEnumerator()
			{
				for (int index = _cache._head; index >= 0; index = _cache._entries[index].Next)
					yield return _cache._entries[index].Key;
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();

			void ICollection<K>.Add(K item)
				=> throw new NotSupportedException();

			public void Clear()
				=> _cache.Clear();

			public bool Remove(K key)
				=> _cache.Remove(key);
		}

		private struct LightweightValuesCollection : ICollection<V>
		{
			private readonly Cache<K, V> _cache;

			public int Count => _cache.Count;

			public bool IsReadOnly => false;

			public LightweightValuesCollection(Cache<K, V> cache)
				=> _cache = cache;

			public bool Contains(V value)
				=> _cache.Any(pair => object.Equals(pair.Value, value));

			public void CopyTo(V[] array, int arrayIndex)
			{
				for (int index = _cache._head; index >= 0; index = _cache._entries[index].Next)
					array[arrayIndex++] = _cache._entries[index].Value;
			}

			public IEnumerator<V> GetEnumerator()
			{
				for (int index = _cache._head; index >= 0; index = _cache._entries[index].Next)
					yield return _cache._entries[index].Value;
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();

			void ICollection<V>.Add(V value)
				=> throw new NotSupportedException();

			public void Clear()
				=> _cache.Clear();

			public bool Remove(V value)
				=> throw new NotSupportedException();
		}

		public ICollection<K> Keys => new LightweightKeysCollection(this);

		public ICollection<V> Values => new LightweightValuesCollection(this);

		#endregion

		#region Cache mechanics

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool Lookup(K key, out int index)
		{
			if (_lookup.TryGetValue(key, out index))
			{
				Raise(index);
				return true;
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Raise(int index)
		{
			Detach(index);
			AttachAtHead(index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Evict()
		{
			if (_tail < 0)
				return;

			int index = _tail;
			K key = _entries[index].Key;

			RemoveInternal(index, key);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RemoveInternal(int index, K key)
		{
			_lookup.Remove(key);
			Detach(index);
			_entries[index].Key = default!;
			_entries[index].Value = default!;
		}

		#endregion

		#region Low-level list mechanics

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private void Grow()
		{
			ListEntry[] newEntries = new ListEntry[_entries.Length * 2];
			_entries.AsSpan().CopyTo(newEntries);

			for (int i = _entries.Length; i < newEntries.Length - 1; i++)
				newEntries[i].Next = i + 1;
			newEntries[newEntries.Length - 1].Next = _firstFree;
			_firstFree = _entries.Length;

			_entries = newEntries;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private void Detach(int entry)
		{
			if (entry != _head)
				_entries[_entries[entry].Prev].Next = _entries[_head].Next;
			else
				_head = _entries[_head].Next;

			if (entry != _tail)
				_entries[_entries[entry].Next].Prev = _entries[_tail].Prev;
			else
				_tail = _entries[_tail].Prev;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private void AttachAtHead(int entry)
		{
			_entries[entry].Prev = -1;
			_entries[entry].Next = _head;

			if (_head != -1)
				_entries[_head].Prev = entry;
			else
				_tail = entry;

			_head = entry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private void AttachAtTail(int entry)
		{
			_entries[entry].Prev = _tail;
			_entries[entry].Next = -1;

			if (_tail != -1)
				_entries[_tail].Next = entry;
			else
				_head = entry;

			_tail = entry;
		}

		#endregion

		#region Enumeration

		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			for (int index = _head; index >= 0; index = _entries[index].Next)
				yield return new KeyValuePair<K, V>(_entries[index].Key, _entries[index].Value);
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		#endregion

		#region ICollection<T> support

		void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
		{
			Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item)
		{
			if (!TryGetValue(item.Key, out V? value))
				return false;
			if (!object.Equals(value, item.Value))
				return false;
			return true;
		}

		void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
		{
			foreach (KeyValuePair<K, V> pair in this)
				array[arrayIndex++] = pair;
		}

		bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
		{
			if (!_lookup.TryGetValue(item.Key, out int index))
				return false;
			if (!object.Equals(_entries[index].Value, item.Value))
				return false;

			RemoveInternal(index, item.Key);
			return true;
		}

		#endregion
	}
}
