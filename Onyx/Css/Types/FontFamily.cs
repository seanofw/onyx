using System.Diagnostics.CodeAnalysis;
using Onyx.Extensions;

namespace Onyx.Css.Types
{
	public readonly struct FontFamily : IEquatable<FontFamily>
	{
		public readonly GenericFontFamily GenericFontFamily;
		public readonly string? Name;

		public FontFamily(string name)
			=> Name = !string.IsNullOrEmpty(name) ? name
				: throw new ArgumentNullException(nameof(name));

		public FontFamily(GenericFontFamily genericFontFamily)
			=> GenericFontFamily = genericFontFamily != default
				? genericFontFamily
				: throw new ArgumentNullException(nameof(genericFontFamily));

		public static implicit operator FontFamily(string name)
			=> new FontFamily(name);

		public static implicit operator FontFamily(GenericFontFamily genericFontFamily)
			=> new FontFamily(genericFontFamily);

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is FontFamily other && Equals(other);

		public bool Equals(FontFamily other)
			=> GenericFontFamily == other.GenericFontFamily && Name == other.Name;

		public override int GetHashCode()
			=> unchecked((Name?.GetHashCode() ?? 0) * 65599 + (int)GenericFontFamily);

		public static bool operator ==(FontFamily a, FontFamily b)
			=> a.Equals(b);

		public static bool operator !=(FontFamily a, FontFamily b)
			=> !a.Equals(b);

		public override string ToString()
			=> !string.IsNullOrEmpty(Name)
				? MaybeQuote(Name)
				: GenericFontFamily.ToString().Hyphenize();

		private static bool IsAsciiAlphaChar(char ch)
			=> ch >= 'A' && ch <= 'Z'
			|| ch >= 'a' && ch <= 'z';

		private static bool IsAllAsciiAlpha(string str)
		{
			foreach (char ch in str)
				if (!IsAsciiAlphaChar(ch))
					return false;
			return true;
		}

		private static string MaybeQuote(string str)
		{
			if (IsAllAsciiAlpha(str))
				return str;

			return "\"" + str.AddCSlashes() + "\"";
		}
	}
}
