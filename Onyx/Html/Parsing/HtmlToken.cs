using System.Collections.Immutable;

namespace Onyx.Html.Parsing
{
	/// <summary>
	/// A single HTML token read by the lexer:  This represents either a chunk of
	/// text, a comment, a start tag (with attributes), or an end tag.  This is immutable.
	/// </summary>
	public class HtmlToken : IEquatable<HtmlToken>
	{
		#region Properties

		/// <summary>
		/// What kind of token this is (plain text, start tag, end tag, comment, etc.).
		/// </summary>
		public HtmlTokenKind Kind { get; }

		/// <summary>
		/// The text of this token:  The actual text content, if plain text or a comment;
		/// or the tag name, if a start tag or end tag.  Any HTML entities within this text
		/// will have been decoded, but otherwise this will be the original text as given.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// The attributes for this start tag, if this is a start tag.  These are in
		/// exactly the order found in the source, and not deduplicated.  All HTML entities
		/// within both the keys and the values will have been decoded.
		/// </summary>
		public IReadOnlyList<KeyValuePair<string, string?>>? Attributes { get; }

		/// <summary>
		/// The source location in the input where this tag was found:  Filename, line, column, length, etc.
		/// </summary>
		public SourceLocation SourceLocation { get; }

		#endregion

		#region Construction and With* methods

		/// <summary>
		/// Construct an HTML token.
		/// </summary>
		/// <param name="kind">What kind of token this is (plain text, start tag, end tag, comment, etc.).</param>
		/// <param name="text">The text of this token:  The actual text content, if plain text
		/// or a comment; or the tag name, if a start tag or end tag.  Any HTML entities within
		/// this text should already have been decoded.</param>
		/// <param name="attributes">The attributes for this start tag, if this is a start tag.
		/// These are in exactly the order found in the source, and not deduplicated.  Any HTML
		/// entities within both the keys and the values should already have been decoded.</param>
		/// <param name="sourceLocation">The source location in the input where this tag was found:
		/// Filename, line, column, length, etc.</param>
		public HtmlToken(HtmlTokenKind kind, string text,
			IEnumerable<KeyValuePair<string, string?>>? attributes,
			SourceLocation sourceLocation)
		{
			Kind = kind;
			Text = text;
			Attributes = attributes is ImmutableArray<KeyValuePair<string, string?>> immutable ? immutable
				: attributes?.ToImmutableArray() ?? ImmutableArray<KeyValuePair<string, string?>>.Empty;
			SourceLocation = sourceLocation;
		}

		/// <summary>
		/// Copy this object, replacing one property.
		/// </summary>
		public HtmlToken WithKind(HtmlTokenKind kind)
			=> new HtmlToken(kind, Text, Attributes, SourceLocation);

		/// <summary>
		/// Copy this object, replacing one property.
		/// </summary>
		public HtmlToken WithText(string text)
			=> new HtmlToken(Kind, text, Attributes, SourceLocation);

		/// <summary>
		/// Copy this object, replacing one property.
		/// </summary>
		public HtmlToken WithAttributes(IEnumerable<KeyValuePair<string, string?>>? attributes)
			=> new HtmlToken(Kind, Text, attributes, SourceLocation);

		/// <summary>
		/// Copy this object, replacing one property.
		/// </summary>
		public HtmlToken WithSourceLocation(SourceLocation sourceLocation)
			=> new HtmlToken(Kind, Text, Attributes, sourceLocation);

		#endregion

		#region Equality and hash codes

		/// <summary>
		/// Compare this token against another object for (deep) equality.
		/// </summary>
		/// <param name="obj">The other object to compare against.</param>
		/// <returns>True if all of their properties have equal values (and all attributes are
		/// the same key/value pairs, in the same order); false if they do not.</returns>
		public override bool Equals(object? obj)
			=> obj is HtmlToken other && Equals(other);

		/// <summary>
		/// Compare this token against another for (deep) equality.
		/// </summary>
		/// <param name="other">The other token to compare against.</param>
		/// <returns>True if all of their properties have equal values (and all attributes are
		/// the same key/value pairs, in the same order); false if they do not.</returns>
		public bool Equals(HtmlToken? other)
			=> ReferenceEquals(this, other) ? true
				: ReferenceEquals(other, null) ? false
				: Kind == other.Kind
					&& Text == other.Text
					&& CompareAttributes(Attributes, other.Attributes)
					&& SourceLocation == other.SourceLocation;

		/// <summary>
		/// Deep-compare two sequences of attributes for equality.
		/// </summary>
		/// <param name="a">The first sequence of attributes to compare.</param>
		/// <param name="b">The second sequence of attributes to compare.</param>
		/// <returns>True if they are equal, false if they are not.</returns>
		private static bool CompareAttributes(IReadOnlyList<KeyValuePair<string, string?>>? a,
			IReadOnlyList<KeyValuePair<string, string?>>? b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;

			if (a.Count != b.Count)
				return false;

			for (int i = 0; i < a.Count; i++)
			{
				if (a[i].Key != b[i].Key) return false;
				if (a[i].Value != b[i].Value) return false;
			}
			return true;
		}

		/// <summary>
		/// Generate a hash code that exactly describes this token.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + Text.GetHashCode();
				int count = Attributes != null ? Attributes.Count : 0;
				hashCode = hashCode * 65599 + count;
				if (Attributes != null)
				{
					for (int i = 0; i < count; i++)
					{
						hashCode = hashCode * 65599 + Attributes[i].Key.GetHashCode();
						hashCode = hashCode * 65599 + (Attributes[i].Value?.GetHashCode() ?? 0);
					}
				}
				hashCode = hashCode * 65599 + (SourceLocation?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		/// <summary>
		/// Compare this token against another for (deep) equality.
		/// </summary>
		/// <param name="a">The first token to compare.</param>
		/// <param name="b">The other token to compare against.</param>
		/// <returns>True if all of their properties have equal values (and all attributes are
		/// the same key/value pairs, in the same order); false if they do not.</returns>
		public static bool operator ==(HtmlToken? a, HtmlToken? b)
			=> ReferenceEquals(a, null) ? ReferenceEquals(b, null) : Equals(a, b);

		/// <summary>
		/// Compare this token against another for (deep) equality.
		/// </summary>
		/// <param name="a">The first token to compare.</param>
		/// <param name="b">The other token to compare against.</param>
		/// <returns>False if all of their properties have equal values (and all attributes are
		/// the same key/value pairs, in the same order); true if they do not.</returns>
		public static bool operator !=(HtmlToken? a, HtmlToken? b)
			=> ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !Equals(a, b);

		#endregion

		#region Stringification

		/// <summary>
		/// Convert this to a string, purely for debugging purposes.  This emits useful
		/// output in the debugger, but it is *not* equivalent output to the source text
		/// from which this token was extracted.
		/// </summary>
		/// <returns>The token, as a string.</returns>
		public override string ToString()
			=> $"{Kind} (\"{Text}\") {SourceLocation}";

		#endregion
	}
}