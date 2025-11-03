using Onyx.Extensions;

namespace Onyx.Css.Types
{
	public sealed record class LinearGradient : GradientBase
	{
		public SideOrCorner SideOrCorner { get; init; }
		public Measure Angle { get; init; }

		public LinearGradient()
		{
			Kind = BackgroundKind.LinearGradient;
		}

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (SideOrCorner != default)
				pieces.Add(SideOrCorner.ToString().Hyphenize());
			else if (Angle != default)
				pieces.Add(Angle.ToString());

			foreach (ColorStop colorStop in ColorStops)
				pieces.Add(colorStop.ToString());

			return string.Join(", ", pieces);
		}
	}
}
