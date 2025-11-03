using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class ContentProperty : StyleProperty
    {
		public IReadOnlyList<ContentPiece> Pieces
		{
			get => _pieces;
			init => _pieces = value is ImmutableList<ContentPiece> list ? list
				: value is null ? ImmutableList<ContentPiece>.Empty
				: value.ToImmutableList();
		}
		private readonly ImmutableList<ContentPiece> _pieces = ImmutableList<ContentPiece>.Empty;

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithContentPieces(Pieces);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithContentPieces(source.ContentPieces);

		public override string ToString()
			=> string.Join(" ", Pieces.Select(p => p.ToString()));

		public ContentProperty AddNone()
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.None)) };

		public ContentProperty AddNormal()
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.Normal)) };

		public ContentProperty AddQuoteKind(QuoteKind quoteKind)
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.Quote, quoteKind: quoteKind)) };

		public ContentProperty AddString(string str)
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.String, text: str)) };

		public ContentProperty AddUri(string uri)
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.Uri, text: uri)) };

		public ContentProperty AddCounter(string name, ListStyleType style = default)
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.Counter, text: name, style: style)) };

		public ContentProperty AddCounters(string name, string separator, ListStyleType style = default)
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.Counters, text: name, style: style, separator: separator)) };

		public ContentProperty AddAttr(string attr)
			=> this with { Pieces = _pieces.Add(new ContentPiece(ContentKind.Attr, text: attr)) };
	}
}
