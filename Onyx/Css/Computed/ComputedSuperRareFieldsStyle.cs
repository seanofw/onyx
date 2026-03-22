using System.Collections.Immutable;
using Onyx.Css.Properties;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	/// <summary>
	/// Fields that are almost never used.
	/// </summary>
	public class ComputedSuperRareFieldsStyle
	{
		public CssRect? Clip { get; }

		public IReadOnlyList<ContentPiece> ContentPieces => _contentPieces;
		private readonly ImmutableArray<ContentPiece> _contentPieces;

		public Measure VerticalAlignLength { get; }

		public IReadOnlyList<Counter> CounterIncrements => _counterIncrements;
		private readonly ImmutableArray<Counter> _counterIncrements;

		public IReadOnlyList<Counter> CounterResets => _counterResets;
		private readonly ImmutableArray<Counter> _counterResets;

		public static ComputedSuperRareFieldsStyle Default { get; }
			= new ComputedSuperRareFieldsStyle(default, default, default, default, default);

		public ComputedSuperRareFieldsStyle(CssRect? clip, IEnumerable<ContentPiece>? contentPieces,
			Measure verticalAlignLength,
			IEnumerable<Counter>? counterIncrements, IEnumerable<Counter>? counterResets)
		{
			Clip = clip;

			_contentPieces = contentPieces is ImmutableArray<ContentPiece> array ? array
				: contentPieces is null ? ImmutableArray<ContentPiece>.Empty
				: contentPieces.ToImmutableArray();

			VerticalAlignLength = verticalAlignLength;

			_counterIncrements = counterIncrements is ImmutableArray<Counter> array2 ? array2
				: counterIncrements is null ? ImmutableArray<Counter>.Empty
				: counterIncrements.ToImmutableArray();

			_counterResets = counterResets is ImmutableArray<Counter> array3 ? array3
				: counterIncrements is null ? ImmutableArray<Counter>.Empty
				: counterIncrements.ToImmutableArray();
		}

		public ComputedSuperRareFieldsStyle WithClip(CssRect? clip)
			=> new ComputedSuperRareFieldsStyle(clip, ContentPieces, VerticalAlignLength, CounterIncrements, CounterResets);
		public ComputedSuperRareFieldsStyle WithContent(IEnumerable<ContentPiece>? contentPieces)
			=> new ComputedSuperRareFieldsStyle(Clip, contentPieces, VerticalAlignLength, CounterIncrements, CounterResets);
		public ComputedSuperRareFieldsStyle WithVerticalAlignLength(Measure verticalAlignLength)
			=> new ComputedSuperRareFieldsStyle(Clip, ContentPieces, verticalAlignLength, CounterIncrements, CounterResets);
		public ComputedSuperRareFieldsStyle WithCounterIncrements(IEnumerable<Counter>? counterIncrements)
			=> new ComputedSuperRareFieldsStyle(Clip, ContentPieces, VerticalAlignLength, counterIncrements, CounterResets);
		public ComputedSuperRareFieldsStyle WithCounterResets(IEnumerable<Counter>? counterResets)
			=> new ComputedSuperRareFieldsStyle(Clip, ContentPieces, VerticalAlignLength, CounterIncrements, counterResets);

		public UInt256 Diff(ComputedSuperRareFieldsStyle other)
		{
			if (this == other)
				return default;

			UInt256 bits = default;

			if (Clip != other.Clip)
				bits = bits.SetBit((int)KnownPropertyKind.Clip);

			if (ContentPieces.Count != other.ContentPieces.Count)
			{
				bits = bits.SetBit((int)KnownPropertyKind.Content);
			}
			else
			{
				for (int i = 0; i < ContentPieces.Count; i++)
					if (ContentPieces[i] != other.ContentPieces[i])
						bits = bits.SetBit((int)KnownPropertyKind.Content);
			}

			if (VerticalAlignLength != other.VerticalAlignLength)
				bits = bits.SetBit((int)KnownPropertyKind.VerticalAlign);

			if (CounterIncrements.Count != other.CounterIncrements.Count)
			{
				bits = bits.SetBit((int)KnownPropertyKind.CounterIncrement);
			}
			else
			{
				for (int i = 0; i < CounterIncrements.Count; i++)
					if (CounterIncrements[i] != other.CounterIncrements[i])
						bits = bits.SetBit((int)KnownPropertyKind.CounterIncrement);
			}

			if (CounterResets.Count != other.CounterResets.Count)
			{
				bits = bits.SetBit((int)KnownPropertyKind.CounterReset);
			}
			else
			{
				for (int i = 0; i < CounterResets.Count; i++)
					if (CounterResets[i] != other.CounterResets[i])
						bits = bits.SetBit((int)KnownPropertyKind.CounterReset);
			}

			return bits;
		}
	}
}
