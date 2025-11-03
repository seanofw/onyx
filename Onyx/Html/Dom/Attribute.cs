using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Extensions;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A single attribute in the DOM.  This is mutable, just like in the standard JS
	/// DOM, but it's designed like a simple C# tuple object instead of inheriting from
	/// Node like the JS Attr class needlessly does.  Mutating the value of this *does*
	/// raise events to its owning Element, though, if it has an owner.
	/// </summary>
	public class Attribute : IEquatable<Attribute>
	{
		/// <summary>
		/// The Element that this attribute is part of.
		/// </summary>
		public Element? Element { get; }

		/// <summary>
		/// The name of this attribute.  This will be forced to lowercase.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The value for this attribute.  This may be modified, and it may be set to
		/// null as well if the attribute has no value.
		/// </summary>
		public string? Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					string? oldValue = _value;
					_value = value;
					Element?.OnAttrChange(Name, this, oldValue);
				}
			}
		}
		private string? _value;

		/// <summary>
		/// The value, but guaranteed to always be a valid string instance.
		/// </summary>
		internal string SafeValue
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Value ?? string.Empty;
		}

		public Attribute(Element? element, string name, string? initialValue)
		{
			Element = element;
			Name = name.FastLowercase();
			_value = initialValue;
		}

		public override bool Equals(object? obj)
			=> obj is Attribute attribute && Equals(attribute);

		public bool Equals(Attribute? other)
			=> ReferenceEquals(other, this) ? true
				: ReferenceEquals(other, null) ? false
				: Name == other.Name && Value == other.Value;

		public override int GetHashCode()
			=> unchecked(Name.GetHashCode() * 65599 + (Value?.GetHashCode() ?? 0));

		public static bool operator ==(Attribute? a, Attribute? b)
			=> ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);

		public static bool operator !=(Attribute? a, Attribute? b)
			=> ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !a.Equals(b);

		public void ToString(StringBuilder dest)
		{
			Name?.HtmlEncodeTo(dest);
			dest.Append("=\"");
			Value?.HtmlEncodeTo(dest);
			dest.Append("\"");
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}
	}
}
