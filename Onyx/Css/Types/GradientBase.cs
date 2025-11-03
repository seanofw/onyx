using System.Collections.Immutable;

namespace Onyx.Css.Types
{
	public record class GradientBase : BackgroundLayerBase
	{
		public bool Repeat { get; init; }

		public IReadOnlyList<ColorStop> ColorStops
		{
			get => _colorStops;
			init => _colorStops = value is ImmutableArray<ColorStop> array ? array
				: value is null ? ImmutableArray<ColorStop>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<ColorStop> _colorStops = ImmutableArray<ColorStop>.Empty;

		public GradientBase AddColorStop(ColorStop colorStop)
			=> this with { ColorStops = _colorStops.Add(colorStop) };
	}
}
