using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class BackgroundRepeatProperty : StyleProperty
    {
		public IReadOnlyList<BackgroundRepeat> Repeats
		{
			get => _repeats;
			init => _repeats = value is ImmutableArray<BackgroundRepeat> array ? array
				: value is null ? ImmutableArray<BackgroundRepeat>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<BackgroundRepeat> _repeats = ImmutableArray<BackgroundRepeat>.Empty;

		public static BackgroundRepeatProperty Default { get; } =
			new BackgroundRepeatProperty { Kind = KnownPropertyKind.BackgroundRepeat };

		public BackgroundRepeatProperty AddRepeat(BackgroundRepeat repeat)
			=> this with { Repeats = _repeats.Add(repeat) };

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundLayerBase>(
				style.BackgroundLayers, Repeats.Count,
				(l, i) => l.WithRepeat(Repeats[i])));

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundAttachment>(
				source.BackgroundLayers, Math.Max(dest.BackgroundLayers.Count, source.BackgroundLayers.Count),
				(l, i) => l.WithRepeat(i < source.BackgroundLayers.Count
					? source.BackgroundLayers[i].Repeat
					: default)));

		public override string ToString()
			=> string.Join(", ", Repeats.Select(r => r.ToString().Hyphenize()));
	}
}
