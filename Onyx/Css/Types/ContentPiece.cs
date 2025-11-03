using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Onyx.Extensions;

namespace Onyx.Css.Types
{
	public readonly struct ContentPiece : IEquatable<ContentPiece>
	{
		public readonly ContentKind Kind;
		public readonly ListStyleType Style;
		public readonly QuoteKind QuoteKind;
		public readonly string? Text;
		public readonly string? Separator;

		public ContentPiece(ContentKind kind,
			ListStyleType style = default, QuoteKind quoteKind = default,
			string? text = null, string? separator = null)
		{
			Kind = kind;
			Style = style;
			QuoteKind = quoteKind;
			Text = text;
			Separator = separator;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is ContentPiece other && Equals(other);

		public bool Equals(ContentPiece other)
			=> Kind == other.Kind && Style == other.Style && QuoteKind == other.QuoteKind
				&& Text == other.Text && Separator == other.Separator;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + (int)Style;
				hashCode = hashCode * 65599 + (int)QuoteKind;
				hashCode = hashCode * 65599 + (Text?.GetHashCode() ?? 0);
				hashCode = hashCode * 65599 + (Separator?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(ContentPiece a, ContentPiece b)
			=> a.Equals(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(ContentPiece a, ContentPiece b)
			=> !a.Equals(b);

		public override string ToString()
			=> Kind switch
			{
				ContentKind.None => "none",
				ContentKind.Normal => "normal",
				ContentKind.String => "\"" + (Text ?? string.Empty).AddCSlashes() + "\"",
				ContentKind.Uri => "uri(\"" + (Text ?? string.Empty).AddCSlashes() + "\")",
				ContentKind.Counter => Style != default
					? "counter(" + (Text ?? string.Empty).AddCSlashes() + ", " + Style.ToString().Hyphenize() + ")"
					: "counter(" + (Text ?? string.Empty).AddCSlashes() + ")",
				ContentKind.Counters => Style != default
					? "counters(" + (Text ?? string.Empty).AddCSlashes() + ", " + (Separator ?? string.Empty).AddCSlashes() + ", " + Style.ToString().Hyphenize() + ")"
					: "counters(" + (Text ?? string.Empty).AddCSlashes() + ", " + (Separator ?? string.Empty).AddCSlashes() + ")",
				ContentKind.Quote => QuoteKind.ToString().Hyphenize(),
				ContentKind.Attr => "attr(" + Text + ")",

				_ => throw new InvalidOperationException("Unknown ContentKind " + Kind),
			};
	}
}
