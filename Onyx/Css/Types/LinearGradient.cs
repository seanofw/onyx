using Onyx.Extensions;

namespace Onyx.Css.Types
{
	public sealed record class LinearGradient : GradientBase
	{
		public SideOrCorner SideOrCorner { get; init; }
		public Measure Angle { get; init; }
		public bool UsesTo { get; init; }

		/// <summary>
		/// The angle, converted to radians from whatever form it was in before,
		/// normalized to [0, 2pi).
		/// </summary>
		public double AngleInRadians
		{
			get
			{
				double angle;
				if (SideOrCorner != default)
				{
					angle = SideOrCorner switch
					{
						SideOrCorner.Left => 270 * (Math.PI / 180),
						SideOrCorner.Right => 90 * (Math.PI / 180),
						SideOrCorner.Top => 0 * (Math.PI / 180),
						SideOrCorner.Bottom => 180 * (Math.PI / 180),
						SideOrCorner.TopLeft => 315 * (Math.PI / 180),
						SideOrCorner.BottomLeft => 225 * (Math.PI / 180),
						SideOrCorner.TopRight => 45 * (Math.PI / 180),
						SideOrCorner.BottomRight => 135 * (Math.PI / 180),
						_ => 0,
					};
				}
				else
				{
					angle = Angle.Units switch
					{
						Units.Degrees => Angle.Value * (Math.PI / 180),
						Units.Radians => Angle.Value,
						Units.Grads => Angle.Value * (Math.PI / 200),
						_ => 0,
					};
				}

				if (!UsesTo)
					angle += Math.PI;

				angle %= 2.0 * Math.PI;

				if (angle < 0)
					angle += 2.0 * Math.PI;

				return angle;
			}
		}

		public LinearGradient()
		{
			Kind = BackgroundKind.LinearGradient;
		}

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (SideOrCorner != default)
			{
				pieces.Add((UsesTo ? "to " : string.Empty) + SideOrCorner.ToString().Hyphenize());
			}
			else if (Angle != default)
				pieces.Add(Angle.ToString());

			foreach (ColorStop colorStop in ColorStops)
				pieces.Add(colorStop.ToString());

			return string.Join(", ", pieces);
		}
	}
}
