namespace Onyx.Rendering
{
	public readonly struct FontMetrics
	{
		public double LineHeight { get; }
		public double Ascent { get; }
		public double Descent { get; }

		public double UnderlineThickness { get; }
		public double UnderlinePosition { get; }
		public double StrikethroughPosition { get; }
		public double OverlinePosition { get; }

		public double EmWidth { get; }
		public double EnWidth { get; }
		public double ExHeight { get; }

		public FontMetrics(
			double lineHeight, double ascent, double descent,
			double underlineThickness, double underlinePosition, double strikethroughPosition, double overlinePosition,
			double emWidth, double enWidth, double exHeight)
		{
			LineHeight = lineHeight;
			Ascent = ascent;
			Descent = descent;

			UnderlineThickness = underlineThickness;
			UnderlinePosition = underlinePosition;
			StrikethroughPosition = strikethroughPosition;
			OverlinePosition = overlinePosition;

			EmWidth = emWidth;
			EnWidth = enWidth;
			ExHeight = exHeight;
		}
	}
}
