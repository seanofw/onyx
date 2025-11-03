
namespace Onyx.Extensions
{
	internal static class DictionaryExtensions
	{
		public static bool AddToDictionaryOfHashSet<K, V>(this Dictionary<K, HashSet<V>> dict, K key, V value)
			where K : notnull
		{
			if (!dict.TryGetValue(key, out HashSet<V>? set))
				dict.Add(key, set = new HashSet<V>());

			return set.Add(value);
		}

		public static bool RemoveFromDictionaryOfHashSet<K, V>(this Dictionary<K, HashSet<V>> dict, K key, V value)
			where K : notnull
		{
			if (!dict.TryGetValue(key, out HashSet<V>? set))
				return false;

			bool result = set.Remove(value);

			if (set.Count == 0)
				dict.Remove(key);

			return result;
		}

		public static void AddToDictionaryOfList<K, V>(this Dictionary<K, List<V>> dict, K key, V value)
			where K : notnull
		{
			if (!dict.TryGetValue(key, out List<V>? list))
				dict.Add(key, list = new List<V>());

			list.Add(value);
		}

		public static bool RemoveFromDictionaryOfList<K, V>(this Dictionary<K, List<V>> dict, K key, V value)
			where K : notnull
		{
			if (!dict.TryGetValue(key, out List<V>? list))
				return false;

			bool result = list.Remove(value);

			if (list.Count == 0)
				dict.Remove(key);

			return result;
		}

		public static void AddToDictionaryOfListWithUniq<K, V>(
			this Dictionary<K, List<V>> dict, K key, V value)
			where K : notnull
		{
			if (!dict.TryGetValue(key, out List<V>? list))
				dict.Add(key, list = new List<V>());

			if (list.Count == 0 || !object.Equals(list[^1], value))
				list.Add(value);
		}
	}
}
