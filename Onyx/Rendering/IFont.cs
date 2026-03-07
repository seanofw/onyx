namespace Onyx.Rendering
{
	public interface IFont : IDisposable
	{
		FontInfo FontInfo { get; }

		TextMetrics MeasureText(ReadOnlySpan<char> text);

		FontMetrics FontMetrics { get; }
	}
}
