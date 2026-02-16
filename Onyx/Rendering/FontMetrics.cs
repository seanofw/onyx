namespace Onyx.Rendering
{
	public readonly struct FontMetrics
	{
		public double LineHeight { get; }
		public double Ascent { get; }
		public double Descent { get; }

		public double OverlinePosition { get; }
		public double UnderlinePosition { get; }
		public double StrikethroughPosition { get; }
		public double UnderlineThickness { get; }
	}
}
