using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Onyx.Extensions;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A dictionary-like construct for holding the element's attributes.  We do not
	/// follow the JS DOM and have actual "Attr" objects inside a "NamedNodeMap," as it's
	/// very much unlike what C# and F# programmers are used to:  Instead, we offer a
	/// class that looks and feels like a dictionary (and actually *is* a dictionary under
	/// the hood) but that still can affect the element that owns it.
	/// 
	/// Note that the keys of this dictionary are *case-insensitive*, using Ordinal
	/// conversion rules for comparisons and lookups.  This is a little unusual for ordinary
	/// dictionaries, but it is the expected behavior for HTML.
	/// </summary>
	public class AttributeDictionary : IDictionary<string, string>
	{
		/// <summary>
		/// The element that owns this attribute dictionary.  Each element may have at most one.
		/// </summary>
		private readonly Element _owner;

		/// <summary>
		/// The actual attribute dictionary itself, a hash table under the hood.
		/// </summary>
		private Dictionary<string, string>? _attributes;

		/// <summary>
		/// All of the keys of this dictionary, as a live collection.
		/// </summary>
		public ICollection<string> Keys => new KeysCollection(this);

		/// <summary>
		/// All of the values of this dictionary, as a live collection.
		/// </summary>
		public ICollection<string> Values => new ValuesCollection(this);

		/// <summary>
		/// The total number of attributes defined in this dictionary.
		/// </summary>
		public int Count => _attributes?.Count ?? 0;

		/// <summary>
		/// Whether this attribute dictionary is read-only (it isn't).
		/// </summary>
		public bool IsReadOnly => false;

		/// <summary>
		/// Construct a new attribute dictionary.  This can only be done by the element that
		/// owns it, so it's marked 'internal'.
		/// </summary>
		/// <param name="owner">The element that owns this.</param>
		internal AttributeDictionary(Element owner)
		{
			_owner = owner;
			_attributes = new Dictionary<string, string>();
		}

		/// <summary>
		/// Construct a new attribute dictionary.  This can only be done by the element that
		/// owns it, so it's marked 'internal'.
		/// </summary>
		/// <param name="owner">The element that owns this.</param>
		/// <param name="attributes">The attributes to start out with.</param>
		internal AttributeDictionary(Element owner, Dictionary<string, string> attributes)
		{
			_owner = owner;
			_attributes = attributes;
		}

		/// <summary>
		/// Read or write an attribute value, by name.
		/// </summary>
		/// <param name="key">The name of the attribute value to read or write.</param>
		/// <returns>The value of the attribute.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the key is null.</exception>
		/// <exception cref="KeyNotFoundException">Thrown if the requested attribute does not exist.</exception>
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

		/// <summary>
		/// Ensure that the internal dictionary has been created by creating it if
		/// it doesn't exist.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureDict()
			=> _attributes ??= new Dictionary<string, string>();

		/// <summary>
		/// Add a new attribute.
		/// </summary>
		/// <param name="key">The key (name) of the attribute to add.</param>
		/// <param name="value">The value of the attribute to add.</param>
		/// <exception cref="ArgumentNullException">Thrown if the key is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the key already exists in the collection.</exception>
		public void Add(string key, string value)
		{
			EnsureDict();
			key = key.FastLowercase();
			_attributes!.Add(key, value ?? string.Empty);
			_owner.OnAttrChange(key, value, null);
		}

		/// <summary>
		/// Add a new attribute.
		/// </summary>
		/// <param name="item">The attribute tuple to add to the collection.</param>
		/// <exception cref="ArgumentNullException">Thrown if the item's Key is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the item's Key already exists in the collection.</exception>
		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
			=> Add(item.Key.FastLowercase(), item.Value);

		/// <summary>
		/// Add a new attribute.
		/// </summary>
		/// <param name="key">The key (name) of the attribute to add.</param>
		/// <param name="value">The value of the attribute to add.</param>
		/// <returns>True if the attribute was added, false if the key already existed.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the key is null.</exception>
		public bool TryAdd(string key, string value)
		{
			EnsureDict();
			key = key.FastLowercase();
			if (_attributes!.TryAdd(key, value ?? string.Empty))
			{
				_owner.OnAttrChange(key, value, null);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Remove all attributes.
		/// </summary>
		public void Clear()
		{
			_attributes?.Clear();
			_owner.OnAttrChange(null, null, null);
		}

		/// <summary>
		/// Determine whether the dictionary contains the given attribute.
		/// </summary>
		/// <param name="item">The attribute to test for.  Both the Key and Value will be compared.</param>
		/// <returns>True if the attribute exists with the given value, false if it does not.</returns>
		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
			=> _attributes?.Contains(new KeyValuePair<string, string>(
				item.Key.FastLowercase(), item.Value ?? string.Empty)) ?? false;

		/// <summary>
		/// Determine whether the dictionary contains an attribute with the given name.
		/// </summary>
		/// <param name="key">The name of the attribute to test for.</param>
		/// <returns>True if such an attribute exists, false if it does not.</returns>
		public bool ContainsKey(string key)
			=> _attributes?.ContainsKey(key.FastLowercase()) ?? false;

		/// <summary>
		/// Copy all of the attributes to the given array.
		/// </summary>
		/// <param name="array">The destination array to copy the attributes to.</param>
		/// <param name="arrayIndex">The starting index to copy to.</param>
		public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			if (_attributes != null)
				((ICollection<KeyValuePair<string, string>>)_attributes).CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Enumerate all of the attributes in the dictionary.
		/// </summary>
		/// <returns>An enumerator that will lazily produce all of the attributes in the dictionary.</returns>
		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
			=> _attributes?.OrderBy(a => a.Key).GetEnumerator()
				?? Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();

		/// <summary>
		/// Enumerate all of the attributes in the dictionary.
		/// </summary>
		/// <returns>An enumerator that will lazily produce all of the attributes in the dictionary.</returns>
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		/// <summary>
		/// Remove an attribute from the dictionary, by name.
		/// </summary>
		/// <param name="key">The name of the attribute to remove.</param>
		/// <returns>True if the attribute was removed, or false if no such attribute existed.</returns>
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

		/// <summary>
		/// Remove an attribute from the dictionary.
		/// </summary>
		/// <param name="item">The attribute to remove.  The Key and Value must match the attribute
		/// to remove, or this will indicate that the attribute is not found.</param>
		/// <returns>True if the attribute was removed, or false if no such attribute existed.</returns>
		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
		{
			if (!TryGetValue(item.Key.FastLowercase(), out string? value)
				|| value != (item.Value ?? string.Empty))
				return false;
			return Remove(item.Key);
		}

		/// <summary>
		/// Retrieve a value from the dictionary, safely.
		/// </summary>
		/// <param name="key">The key of the value to locate.</param>
		/// <param name="value">The resulting value in the dictionary (if any).</param>
		/// <returns>True if an attribute with the given name exists, false if no such
		/// attribute was found.</returns>
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
		{
			if (_attributes == null)
			{
				value = default!;
				return false;
			}

			return _attributes.TryGetValue(key.FastLowercase(), out value);
		}

		/// <summary>
		/// An ICollection{string} implementation that can produce and modify the
		/// collection via its keys (names).  This supports all ICollection{T} methods
		/// except for Add(key), as it's a nonsensical method for a dictionary.
		/// </summary>
		private struct KeysCollection : ICollection<string>
		{
			private readonly AttributeDictionary _owner;

			public int Count => _owner.Count;

			public bool IsReadOnly => false;

			public KeysCollection(AttributeDictionary owner)
				=> _owner = owner;

			public void Add(string item)
				=> throw new NotSupportedException();

			public void Clear()
				=> _owner.Clear();

			public bool Contains(string item)
				=> _owner._attributes?.ContainsKey(item.FastLowercase()) ?? false;

			public void CopyTo(string[] array, int arrayIndex)
			{
				foreach (string key in _owner.Keys)
					array[arrayIndex++] = key;
			}

			public bool Remove(string item)
				=> _owner._attributes?.Remove(item.FastLowercase()) ?? false;

			public IEnumerator<string> GetEnumerator()
				=> _owner._attributes?.Keys.GetEnumerator() ?? Enumerable.Empty<string>().GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}

		/// <summary>
		/// An ICollection{string} implementation that can produce and modify the
		/// collection via its values.  This supports all ICollection{T} methods
		/// except for Add(value), as it's a nonsensical method for a dictionary, and
		/// Remove(value), as there's not really a good use case that involves searching
		/// the values for a particular one to remove.
		/// </summary>
		private struct ValuesCollection : ICollection<string>
		{
			private readonly AttributeDictionary _owner;

			public int Count => _owner.Count;

			public bool IsReadOnly => false;

			public ValuesCollection(AttributeDictionary owner)
				=> _owner = owner;

			public void Add(string item)
				=> throw new NotSupportedException();

			public void Clear()
				=> _owner.Clear();

			public bool Contains(string item)
				=> _owner._attributes?.Any(p => p.Value == item) ?? false;

			public void CopyTo(string[] array, int arrayIndex)
			{
				foreach (string value in _owner.Values)
					array[arrayIndex++] = value;
			}

			public bool Remove(string item)
				=> throw new NotSupportedException();

			public IEnumerator<string> GetEnumerator()
				=> _owner._attributes?.Values.GetEnumerator() ?? Enumerable.Empty<string>().GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
				=> GetEnumerator();
		}
	}
}
