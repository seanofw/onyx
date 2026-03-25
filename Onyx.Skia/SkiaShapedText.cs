using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public class SkiaShapedText : SkiaDisposable, IShapedText
	{
		private SKFont _font;
		private SKTextBlob _textBlob;

		internal SKTextBlob TextBlob => _textBlob;

		public TextMetrics TextMetrics { get; }

		internal SkiaShapedText(SKFont font, ReadOnlySpan<char> text)
		{
			_font = font;

			_textBlob = SKTextBlob.Create(text, _font)
				?? throw new InvalidOperationException("SKTextBlob.Create() failed to create a text blob");

			float advance = font.MeasureText(text, out SKRect bounds);

			TextMetrics = new TextMetrics(
				new Rect2d(bounds.Left, bounds.Top, bounds.Width, bounds.Height),
				new Vector2d(advance, 0)
			);
		}
	}
}
