using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BackgroundSizeProperty : StyleProperty
	{
		public IReadOnlyList<BackgroundSize> Sizes
		{
			get => _sizes;
			init => _sizes = value is ImmutableArray<BackgroundSize> array ? array
				: value is null ? ImmutableArray<BackgroundSize>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<BackgroundSize> _sizes = ImmutableArray<BackgroundSize>.Empty;

		public static BackgroundSizeProperty Default { get; } =
			new BackgroundSizeProperty { Kind = KnownPropertyKind.BackgroundSize };

		public BackgroundSizeProperty AddSize(BackgroundSize size)
			=> this with { Sizes = _sizes.Add(size) };

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundLayerBase>(
				style.BackgroundLayers, Sizes.Count,
				(l, i) => l.WithSize(Sizes[i])));

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundAttachment>(
				source.BackgroundLayers, Math.Max(dest.BackgroundLayers.Count, source.BackgroundLayers.Count),
				(l, i) => l.WithSize(i < source.BackgroundLayers.Count
					? source.BackgroundLayers[i].Size
					: default)));

		public override string ToString()
			=> string.Join(", ", Sizes.Select(s => s.ToString()));
	}
}
