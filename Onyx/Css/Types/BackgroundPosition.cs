namespace Onyx.Css.Types
{
	public record struct BackgroundPosition
	{
		public Measure X { get; init; }
		public Measure Y { get; init; }

		public static BackgroundPosition Default { get; }
			= new BackgroundPosition { X = Measure.Zero, Y = Measure.Zero };

		public override string ToString()
			=> X.ToString() + " " + Y.ToString();
	}
}
