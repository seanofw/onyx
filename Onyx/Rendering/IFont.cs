namespace Onyx.Rendering
{
	public interface IFont
	{
		FontInfo FontInfo { get; }

		TextMetrics MeasureText(ReadOnlySpan<char> text);

		FontMetrics FontMetrics { get; }
	}
}
