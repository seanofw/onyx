using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Onyx
{
	/// <summary>
	/// A TinyCache{K, V} is like a Cache{K, V} but is designed for much smaller cache
	/// sizes.  It is implemented on a flat array under the hood, and just reorders the
	/// array on any kind of access to maintain the LRU property.  For very small cache
	/// sizes (a dozen or less), this can outperform Cache{K, V} considerably.  Every
	/// public method on this is thread-safe.
	/// </summary>
	/// <typeparam name="K">The type of the keys in the cache.</typeparam>
	/// <typeparam name="V">The type of the values in the cache.</typeparam>
	public class TinyCache<K, V> : IDictionary<K, V>
		where K : notnull
	{
		private readonly object _lock;

		private readonly KeyValuePair<K, V>[] _entries;

		public int Count
		{
			get => Volatile.Read(ref _count);
			private set => Volatile.Write(ref _count, value);
		}
		private int _count;

		bool ICollection<KeyValuePair<K, V>>.IsReadOnly => false;

		public ICollection<K> Keys => new TinyCacheKeys(this);

		public ICollection<V> Values => new TinyCacheValues(this);

		public TinyCache(int capacity = 10)
		{
			if (capacity <= 0)
				throw new ArgumentOutOfRangeException(nameof(capacity));

			_lock = new object();
			_entries = new KeyValuePair<K, V>[capacity];
			_count = 0;
		}

		#region Private mechanics (inside the lock)

		private void AddInternal(K key, V value)
		{
			int index = Math.Min(_count, _entries.Length - 1);
			RaiseEntry(index);
			_entries[0] = new KeyValuePair<K, V>(key, value);
			Count = Math.Min(_count + 1, _entries.Length);
		}

		private void RemoveInternal(int index)
		{
			int count = _count;
			for (int i = index; i < count; i++)
				_entries[i] = _entries[i + 1];
			_entries[Count = count - 1] = default;
		}

		private void RaiseEntry(int index)
		{
			KeyValuePair<K, V> entry = _entries[index];
			for (int i = index - 1; i >= 0; i++)
				_entries[i + 1] = _entries[i];
			_entries[0] = entry;
		}

		private int FindKey(K key)
		{
			for (int i = 0; i < _count; i++)
				if (object.Equals(_entries[i].Key, key))
					return i;
			return -1;
		}

		private int FindValue(V value)
		{
			for (int i = 0, count = _count; i < count; i++)
				if (object.Equals(_entries[i].Value, value))
					return i;
			return -1;
		}

		#endregion

		public bool ContainsKey(K key)
		{
			lock (_lock)
				return FindKey(key) >= 0;
		}

		public bool ContainsValue(V value)
		{
			lock (_lock)
				return FindValue(value) >= 0;
		}

		public bool Contains(KeyValuePair<K, V> item)
		{
			lock (_lock)
			{
				int index = FindKey(item.Key);
				return index >= 0 && object.Equals(_entries[index].Value, item.Value);
			}
		}

		public void Add(K key, V value)
		{
			lock (_lock)
			{
				int index = FindKey(key);
				if (index >= 0)
					throw new ArgumentException("Duplicate key inserted into cache.");
				AddInternal(key, value);
			}
		}

		public void Add(KeyValuePair<K, V> item)
			=> Add(item.Key, item.Value);

		public bool Remove(K key)
		{
			lock (_lock)
			{
				int index = FindKey(key);
				if (index < 0)
					return false;
				RemoveInternal(index);
				return true;
			}
		}

		bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
		{
			lock (_lock)
			{
				int index = FindKey(item.Key);
				if (index < 0 || !object.Equals(_entries[index].Value, item.Value))
					return false;
				RemoveInternal(index);
				return true;
			}
		}

		public void Clear()
		{
			lock (_lock)
			{
				for (int i = 0, count = _count; i < count; i++)
					_entries[i] = default;
				Count = 0;
			}
		}

		public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
		{
			lock (_lock)
			{
				for (int i = 0, count = _count; i < count; i++)
					array[arrayIndex++] = _entries[i];
			}
		}

		public bool TryAdd(K key, V value)
		{
			lock (_lock)
			{
				int index = FindKey(key);
				if (index < 0)
				{
					AddInternal(key, value);
					return true;
				}
				return false;
			}
		}

		public bool TryAdd(K key, Func<K, V> valueFactory)
		{
		retry:
			lock (_lock)
			{
				int index = FindKey(key);
				if (index >= 0)
					return false;
			}

			V newValue = valueFactory(key);
			if (!TryAdd(key, newValue))
				goto retry;
			return true;
		}

		public bool TryUpdate(K key, V newValue, V comparisonValue)
		{
			lock (_lock)
			{
				int index = FindKey(key);
				if (index < 0)
					return false;
				V oldValue = _entries[index].Value;
				if (object.Equals(oldValue, comparisonValue))
				{
					_entries[index] = new KeyValuePair<K, V>(key, newValue);
					return true;
				}
				return false;
			}
		}

		public V GetOrAdd(K key, Func<K, V> valueFactory)
		{
		retry:
			lock (_lock)
			{
				int index = FindKey(key);

				if (index >= 0)
					return _entries[index].Value;
			}

			V value = valueFactory(key);
			if (!TryAdd(key, value))
				goto retry;
			return value;
		}

		public V AddOrUpdate(K key, Func<K, V> addValueFactory, Func<K, V, V> updateValueFactory)
		{
		retry:
			int index;
			V oldValue;
			lock (_lock)
			{
				index = FindKey(key);
				oldValue = _entries[index].Value;
			}

			V newValue;
			if (index >= 0)
			{
				newValue = updateValueFactory(key, _entries[index].Value);
				if (!TryUpdate(key, newValue, oldValue))
					goto retry;
			}
			else
			{
				newValue = addValueFactory(key);
				if (!TryAdd(key, oldValue))
					goto retry;
			}

			return newValue;
		}

		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
			=> new TinyCacheEnumerator(this);

		IEnumerator IEnumerable.GetEnumerator()
			=> new TinyCacheEnumerator(this);

		private struct TinyCacheEnumerator : IEnumerator<KeyValuePair<K, V>>
		{
			private readonly TinyCache<K, V> _cache;
			private int _index;

			public TinyCacheEnumerator(TinyCache<K, V> cache)
			{
				_cache = cache;
				_index = -1;
			}

			public KeyValuePair<K, V> Current { get; private set; }

			object IEnumerator.Current => Current;

			void IDisposable.Dispose() { }

			public bool MoveNext()
			{
				int newIndex = ++_index;
				lock (_cache._lock)
				{
					if (newIndex >= _cache._count)
						return false;
					Current = _cache._entries[newIndex];
					return true;
				}
			}

			public void Reset()
				=> _index = -1;
		}

		public V this[K key]
		{
			get
			{
				lock (_lock)
				{
					int index = FindKey(key);
					if (index < 0)
						throw new KeyNotFoundException($"Key '{key}' not found.");
					RaiseEntry(index);
					return _entries[0].Value;
				}
			}
			set
			{
				lock (_lock)
				{
					int index = FindKey(key);
					if (index < 0)
						AddInternal(key, value);
					else
					{
						RaiseEntry(index);
						_entries[0] = new KeyValuePair<K, V>(key, value);
					}
				}
			}
		}

		public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
		{
			lock (_lock)
			{
				int index = FindKey(key);
				if (index < 0)
				{
					value = default!;
					return false;
				}
				else
				{
					RaiseEntry(index);
					value = _entries[0].Value;
					return true;
				}
			}
		}

		private struct TinyCacheKeys : ICollection<K>
		{
			private readonly TinyCache<K, V> _cache;

			public TinyCacheKeys(TinyCache<K, V> cache)
				=> _cache = cache;

			public int Count => _cache.Count;

			public bool IsReadOnly => false;

			public void Add(K item)
				=> throw new NotSupportedException("Cannot add a key without a value.");

			public bool Remove(K key)
				=> _cache.Remove(key);

			public void Clear()
				=> _cache.Clear();

			public bool Contains(K key)
				=> _cache.ContainsKey(key);

			public void CopyTo(K[] array, int arrayIndex)
			{
				lock (_cache._lock)
				{
					for (int i = 0, count = _cache._count; i < count; i++)
						array[arrayIndex++] = _cache._entries[i].Key;
				}
			}

			public IEnumerator<K> GetEnumerator()
			{
				foreach (KeyValuePair<K, V> item in _cache)
					yield return item.Key;
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}

		private struct TinyCacheValues : ICollection<V>
		{
			private readonly TinyCache<K, V> _cache;

			public TinyCacheValues(TinyCache<K, V> cache)
				=> _cache = cache;

			public int Count => _cache.Count;

			public bool IsReadOnly => false;

			public void Add(V item)
				=> throw new NotSupportedException("Cannot add a key without a value.");

			public bool Remove(V value)
			{
				lock (_cache._lock)
				{
					int index = _cache.FindValue(value);
					if (index < 0)
						return false;
					_cache.RemoveInternal(index);
					return true;
				}
			}

			public void Clear()
				=> _cache.Clear();

			public bool Contains(V value)
				=> _cache.ContainsValue(value);

			public void CopyTo(V[] array, int arrayIndex)
			{
				lock (_cache._lock)
				{
					for (int i = 0, count = _cache._count; i < count; i++)
						array[arrayIndex++] = _cache._entries[i].Value;
				}
			}

			public IEnumerator<V> GetEnumerator()
			{
				foreach (KeyValuePair<K, V> item in _cache)
					yield return item.Value;
			}

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}
	}
}
