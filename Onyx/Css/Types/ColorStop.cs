using Onyx.Types;

namespace Onyx.Css.Types
{
	public record struct ColorStop
	{
		public Color32? Color { get; init; }
		public Measure Measure { get; init; }

		public ColorStop() { }

		public ColorStop(Color32? color, Measure measure)
		{
			Color = color;
			Measure = measure;
		}

		public override string ToString()
			=> Color.HasValue && Measure != default ? Color.Value.ToString() + " " + Measure.ToString()
				: Color.HasValue ? Color.Value.ToString()
				: Measure != default ? Measure.ToString()
				: string.Empty;
	}
}
