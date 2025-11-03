using Onyx.Css.Computed;
using Onyx.Css.Types;
using System.Collections.Immutable;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BackgroundImageProperty : StyleProperty
	{
		public IReadOnlyList<BackgroundLayerBase> BackgroundLayers
		{
			get => _backgroundLayers;
			init => _backgroundLayers = value is ImmutableArray<BackgroundLayerBase> array ? array
				: value is null ? ImmutableArray<BackgroundLayerBase>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<BackgroundLayerBase> _backgroundLayers = ImmutableArray<BackgroundLayerBase>.Empty;

		public BackgroundImageProperty AddNone()
			=> this with { BackgroundLayers = _backgroundLayers.Add(BackgroundLayerNone.Instance) };
		public BackgroundImageProperty Add(string url)
			=> this with { BackgroundLayers = _backgroundLayers.Add(new BackgroundImage { Url = url }) };
		public BackgroundImageProperty Add(GradientBase gradient)
			=> this with { BackgroundLayers = _backgroundLayers.Add(gradient) };

		public static BackgroundImageProperty Default { get; } =
			new BackgroundImageProperty { Kind = KnownPropertyKind.BackgroundImage };

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundLayerBase>(
				style.BackgroundLayers, BackgroundLayers.Count,
				(l, i) => l.WithLayer(BackgroundLayers[i])));

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundAttachment>(
				source.BackgroundLayers, Math.Max(dest.BackgroundLayers.Count, source.BackgroundLayers.Count),
				(l, i) => l.WithLayer(i < source.BackgroundLayers.Count
					? source.BackgroundLayers[i].Layer
					: default)));

		public override string ToString()
			=> string.Join(", ", BackgroundLayers.Select(l => l.ToString()));
	}
}
