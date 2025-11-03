using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class CounterIncrementProperty : StyleProperty
	{
		public IReadOnlyList<Counter> Counters
		{
			get => _counters;
			init => _counters = value is ImmutableArray<Counter> array ? array
				: value is null ? ImmutableArray<Counter>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<Counter> _counters = ImmutableArray<Counter>.Empty;

		public bool None { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithCounterIncrements(None ? [] : Counters);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithCounterIncrements(source.CounterIncrements);

		public override string ToString()
			=> string.Join(" ", Counters.Select(c => c.ToString()));

		public CounterIncrementProperty AddCounter(string name, int value)
			=> this with { Counters = _counters.Add(new Counter(name, value)) };
		public CounterIncrementProperty ApplyValue(int value)
			=> this with { Counters = _counters.SetItem(
				_counters.Length - 1, new Counter(_counters[^1].Name, value)) };
	}
}
