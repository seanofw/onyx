using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BackgroundOriginProperty : StyleProperty
	{
		public IReadOnlyList<BackgroundOrigin> Origins
		{
			get => _origins;
			init => _origins = value is ImmutableArray<BackgroundOrigin> array ? array
				: value is null ? ImmutableArray<BackgroundOrigin>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<BackgroundOrigin> _origins = ImmutableArray<BackgroundOrigin>.Empty;

		public static BackgroundOriginProperty Default { get; } =
			new BackgroundOriginProperty { Kind = KnownPropertyKind.BackgroundOrigin };

		public BackgroundOriginProperty AddOrigin(BackgroundOrigin origin)
			=> this with { Origins = _origins.Add(origin) };

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundLayerBase>(
				style.BackgroundLayers, Origins.Count,
				(l, i) => l.WithOrigin(Origins[i])));

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundAttachment>(
				source.BackgroundLayers, Math.Max(dest.BackgroundLayers.Count, source.BackgroundLayers.Count),
				(l, i) => l.WithOrigin(i < source.BackgroundLayers.Count
					? source.BackgroundLayers[i].Origin
					: default)));

		public override string ToString()
			=> string.Join(", ", Origins.Select(o => o.ToString()));
	}
}
