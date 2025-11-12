using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Onyx.Extensions;

namespace Onyx.Html.Dom
{
	public struct NamedNodeMap : IDictionary<string, string>
	{
		private readonly Element _owner;

		private readonly Dictionary<string, string>? _attributes => _owner.AttributesDict;

		public NamedNodeMap(Element owner)
		{
			_owner = owner;
		}

		public string this[string key]
		{
			get => _attributes != null
				? _attributes[key.FastLowercase()]
				: throw new KeyNotFoundException($"Attribute '{key}' not found.");
			set
			{
				EnsureDict();
				key = key.FastLowercase();
				if (!_attributes!.TryGetValue(key, out string? oldValue)
					|| !ReferenceEquals(value, oldValue))
				{
					_attributes[key] = value;
					_owner.OnAttrChange(key, value, oldValue);
				}
			}
		}

		public readonly Attribute? this[int index]
			=> throw new NotSupportedException();

		private void EnsureDict()
			=> _owner.AttributesDict ??= new Dictionary<string, string>();

		public ICollection<string> Keys => throw new NotSupportedException();

		public ICollection<string> Values => throw new NotSupportedException();

		public readonly int Count => _attributes?.Count ?? 0;

		public readonly bool IsReadOnly => false;

		public void Add(string key, string value)
		{
			EnsureDict();
			key = key.FastLowercase();
			_attributes!.Add(key, value);
			_owner.OnAttrChange(key, value, null);
		}

		public void Add(KeyValuePair<string, string> item)
			=> Add(item.Key.FastLowercase(), item.Value);

		public void Clear()
		{
			_owner.AttributesDict = null;
			_owner.OnAttrChange(null, null, null);
		}

		public readonly bool Contains(KeyValuePair<string, string> item)
			=> _attributes?.Contains(item) ?? false;

		public readonly bool ContainsKey(string key)
			=> _attributes?.ContainsKey(key.FastLowercase()) ?? false;

		public readonly void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			if (_attributes != null)
				((ICollection<KeyValuePair<string, string>>)_attributes).CopyTo(array, arrayIndex);
		}

		public readonly IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			=> _attributes?.OrderBy(a => a.Key).GetEnumerator()
				?? Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();

		public bool Remove(string key)
		{
			key = key.FastLowercase();
			if (_attributes != null && _attributes.Remove(key))
			{
				_owner.OnAttrChange(key, null, null);
				return true;
			}
			return false;
		}

		public bool Remove(KeyValuePair<string, string> item)
		{
			if (!TryGetValue(item.Key.FastLowercase(), out string? value)
				|| !ReferenceEquals(value, item.Value))
				return false;
			return Remove(item.Key);
		}

		public readonly bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
		{
			if (_attributes == null)
			{
				value = default!;
				return false;
			}

			return _attributes.TryGetValue(key.FastLowercase(), out value);
		}

		readonly IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
	}
}
