using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Onyx.Css.Properties;
using Onyx.Css.Types;
using Onyx.Types;

namespace Onyx.Css.Computed
{
	public class ComputedBackgroundStyle
	{
		public readonly Color32 Color;

		public IReadOnlyList<ComputedBackgroundLayer> Layers => _layers;
		private readonly ImmutableArray<ComputedBackgroundLayer> _layers;

		public IReadOnlyList<Shadow> BoxShadows => _boxShadows;
		private readonly ImmutableArray<Shadow> _boxShadows;

		public readonly double Opacity;

		public static ComputedBackgroundStyle Default { get; }
			= new ComputedBackgroundStyle(Color32.Transparent, null, null, 1.0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedBackgroundStyle(Color32 color, IEnumerable<ComputedBackgroundLayer>? layers,
			IEnumerable<Shadow>? boxShadows, double opacity)
		{
			Color = color;
			_layers = layers is ImmutableArray<ComputedBackgroundLayer> array ? array
				: layers is null ? ImmutableArray<ComputedBackgroundLayer>.Empty
				: layers.ToImmutableArray();
			_boxShadows = boxShadows is ImmutableArray<Shadow> array2 ? array2
				: boxShadows is null ? ImmutableArray<Shadow>.Empty
				: boxShadows.ToImmutableArray();
			Opacity = opacity;
		}

		public ComputedBackgroundStyle WithColor(Color32 color)
			=> new ComputedBackgroundStyle(color, _layers, BoxShadows, Opacity);
		public ComputedBackgroundStyle WithLayers(IEnumerable<ComputedBackgroundLayer>? layers)
			=> new ComputedBackgroundStyle(Color, layers, BoxShadows, Opacity);
		public ComputedBackgroundStyle WithBoxShadows(IEnumerable<Shadow>? boxShadows)
			=> new ComputedBackgroundStyle(Color, _layers, boxShadows, Opacity);
		public ComputedBackgroundStyle WithOpacity(double opacity)
			=> new ComputedBackgroundStyle(Color, _layers, BoxShadows, opacity);

		public UInt256 Diff(ComputedBackgroundStyle other)
		{
			if (this == other)
				return default;

			UInt256 bits = default;
			if (Color != other.Color)
				bits = bits.SetBit((int)KnownPropertyKind.BackgroundColor);
			if (Layers.Count != other.Layers.Count)
			{
				bits = bits
					.SetBit((int)KnownPropertyKind.BackgroundAttachment)
					.SetBit((int)KnownPropertyKind.BackgroundOrigin)
					.SetBit((int)KnownPropertyKind.BackgroundRepeat)
					.SetBit((int)KnownPropertyKind.BackgroundPosition)
					.SetBit((int)KnownPropertyKind.BackgroundSize)
					.SetBit((int)KnownPropertyKind.BackgroundImage);
			}
			else
			{
				for (int i = 0; i < Layers.Count; i++)
					bits |= Layers[i].Diff(other.Layers[i]);
			}
			if (BoxShadows.Count != other.BoxShadows.Count)
			{
				bits = bits.SetBit((int)KnownPropertyKind.BoxShadow);
			}
			else
			{
				for (int i = 0; i < BoxShadows.Count; i++)
					if (!BoxShadows[i].Equals(other.BoxShadows[i]))
						bits = bits.SetBit((int)KnownPropertyKind.BoxShadow);
			}
			if (Opacity != other.Opacity)
				bits = bits.SetBit((int)KnownPropertyKind.Opacity);
			return bits;
		}
	}
}
