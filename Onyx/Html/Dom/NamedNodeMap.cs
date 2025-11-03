using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Onyx.Extensions;

namespace Onyx.Html.Dom
{
	public struct NamedNodeMap : IDictionary<string, Attribute>
	{
		private readonly Element _owner;

		private readonly Dictionary<string, Attribute>? _attributes => _owner.AttributesDict;

		public NamedNodeMap(Element owner)
		{
			_owner = owner;
		}

		public Attribute this[string key]
		{
			get => _attributes != null
				? _attributes[key.FastLowercase()]
				: throw new KeyNotFoundException($"Attribute '{key}' not found.");
			set
			{
				EnsureDict();
				key = key.FastLowercase();
				if (!_attributes!.TryGetValue(key, out Attribute? oldValue)
					|| !ReferenceEquals(value, oldValue))
				{
					_attributes[key] = value;
					_owner.OnAttrChange(key, value, oldValue?.Value);
				}
			}
		}

		public readonly Attribute? this[int index]
			=> throw new NotSupportedException();

		private void EnsureDict()
			=> _owner.AttributesDict ??= new Dictionary<string, Attribute>();

		public ICollection<string> Keys => throw new NotSupportedException();

		public ICollection<Attribute> Values => throw new NotSupportedException();

		public readonly int Count => _attributes?.Count ?? 0;

		public readonly bool IsReadOnly => false;

		public void Add(string key, Attribute value)
		{
			EnsureDict();
			key = key.FastLowercase();
			_attributes!.Add(key, value);
			_owner.OnAttrChange(key, value, null);
		}

		public void Add(KeyValuePair<string, Attribute> item)
			=> Add(item.Key.FastLowercase(), item.Value);

		public void Clear()
		{
			_owner.AttributesDict = null;
			_owner.OnAttrChange(null, null, null);
		}

		public readonly bool Contains(KeyValuePair<string, Attribute> item)
			=> _attributes?.Contains(item) ?? false;

		public readonly bool ContainsKey(string key)
			=> _attributes?.ContainsKey(key.FastLowercase()) ?? false;

		public readonly void CopyTo(KeyValuePair<string, Attribute>[] array, int arrayIndex)
		{
			if (_attributes != null)
				((ICollection<KeyValuePair<string, Attribute>>)_attributes).CopyTo(array, arrayIndex);
		}

		public readonly IEnumerator<KeyValuePair<string, Attribute>> GetEnumerator()
			=> _attributes?.OrderBy(a => a.Key).GetEnumerator()
				?? Enumerable.Empty<KeyValuePair<string, Attribute>>().GetEnumerator();

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

		public bool Remove(KeyValuePair<string, Attribute> item)
		{
			if (!TryGetValue(item.Key.FastLowercase(), out Attribute? attr)
				|| !ReferenceEquals(attr, item.Value))
				return false;
			return Remove(item.Key);
		}

		public readonly bool TryGetValue(string key, [MaybeNullWhen(false)] out Attribute value)
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

		public readonly Attribute? GetNamedItem(string name)
			=> TryGetValue(name.FastLowercase(), out Attribute? attr) ? attr : null;

		public Attribute? SetNamedItem(Attribute attr)
		{
			Attribute? oldValue = GetNamedItem(attr.Name);
			this[attr.Name] = attr;
			return oldValue;
		}

		public Attribute? RemoveNamedItem(string name)
		{
			Attribute? oldValue = GetNamedItem(name);
			Remove(name);
			return oldValue;
		}
	}
}
