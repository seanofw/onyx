using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class QuotesProperty : StyleProperty
	{
		public IReadOnlyList<string> Quotes
		{
			get => _quotes;
			init => _quotes = value is ImmutableArray<string> array ? array
				: value is null ? ImmutableArray<string>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<string> _quotes = ImmutableArray<string>.Empty;

		public bool None { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithQuotes(None ? null : Quotes);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithQuotes(source.Quotes);

		public override string ToString()
			=> None ? "none"
				: string.Join(" ", Quotes.Select(q => "\"" + q.ToString().AddCSlashes() + "\""));

		public QuotesProperty AddQuote(string quote)
			=> this with { Quotes = _quotes.Add(quote) };
	}
}
