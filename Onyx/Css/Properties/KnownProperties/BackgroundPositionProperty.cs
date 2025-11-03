using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BackgroundPositionProperty : StyleProperty
    {
		public IReadOnlyList<BackgroundPosition> Positions
		{
			get => _positions;
			init => _positions = value is ImmutableArray<BackgroundPosition> array ? array
				: value is null ? ImmutableArray<BackgroundPosition>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<BackgroundPosition> _positions = ImmutableArray<BackgroundPosition>.Empty;

		public static BackgroundPositionProperty Default { get; } =
			new BackgroundPositionProperty { Kind = KnownPropertyKind.BackgroundPosition };

		public BackgroundPositionProperty AddPosition(BackgroundPosition position)
			=> this with { Positions = _positions.Add(position) };

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundLayerBase>(
				style.BackgroundLayers, Positions.Count,
				(l, i) => l.WithPosition(Positions[i])));

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundAttachment>(
				source.BackgroundLayers, Math.Max(dest.BackgroundLayers.Count, source.BackgroundLayers.Count),
				(l, i) => l.WithPosition(i < source.BackgroundLayers.Count
					? source.BackgroundLayers[i].Position
					: default)));

		public override string ToString()
			=> string.Join(", ", Positions.Select(p => p.ToString()));
	}
}
