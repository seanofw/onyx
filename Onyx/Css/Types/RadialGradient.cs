using Onyx.Extensions;

namespace Onyx.Css.Types
{
	public sealed record class RadialGradient : GradientBase
	{
		public RadialShape RadialShape { get; init; }
		public RadialExtent RadialExtent { get; init; }
		public Measure Measure { get; init; }
		public Measure Measure2 { get; init; }
		public Measure PositionX { get; init; }
		public Measure PositionY { get; init; }

		public RadialGradient()
		{
			Kind = BackgroundKind.RadialGradient;
		}

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (RadialShape != default)
				pieces.Add(RadialShape.ToString().Hyphenize());
			else if (Measure != default && Measure2 != default)
				pieces.Add("circle");
			else
				pieces.Add("ellipse");

			if (RadialExtent != default)
				pieces.Add(RadialExtent.ToString().Hyphenize());
			else if (Measure != default)
			{
				pieces.Add(Measure.ToString());
				if (Measure2 != default)
					pieces.Add(Measure2.ToString());
			}
			else
				pieces.Add("farthest-corner");

			if (PositionX != default || PositionY != default)
			{
				pieces.Add("at");
				pieces.Add(PositionX.ToString());
				pieces.Add(PositionY.ToString());
			}

			string prefix = string.Join(" ", pieces);
			pieces.Clear();

			pieces.Add(prefix);

			foreach (ColorStop colorStop in ColorStops)
					pieces.Add(colorStop.ToString());

			return string.Join(", ", pieces);
		}
	}
}
