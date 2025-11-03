namespace Onyx.Css.Types
{
	public record struct ColorStop
	{
		public Color32? Color { get; init; }
		public Measure Measure { get; init; }

		public override string ToString()
			=> Color.HasValue && Measure != default ? Color.Value.ToString() + " " + Measure.ToString()
				: Color.HasValue ? Color.Value.ToString()
				: Measure != default ? Measure.ToString()
				: string.Empty;
	}
}
