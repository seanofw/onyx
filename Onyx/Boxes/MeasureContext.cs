using Onyx.Rendering;
using Onyx.Types;

namespace Onyx.Boxes
{
	public class MeasureContext
	{
		private IRenderables _renderables;

		private bool _hasFont;
		private IFont? _font;

		public Size2d PercentSize { get; }
		public FontInfo FontInfo { get; }

		public MeasureContext(IRenderables renderables, Size2d percentSize, FontInfo fontInfo)
		{
			_renderables = renderables;

			PercentSize = percentSize;
			FontInfo = fontInfo;
		}

		public MeasureContext WithPercentSize(Size2d percentSize)
			=> new MeasureContext(_renderables, percentSize, FontInfo);
		public MeasureContext WithFontInfo(FontInfo fontInfo)
			=> new MeasureContext(_renderables, PercentSize, fontInfo);

		public IFont? GetCurrentFont()
		{
			if (!_hasFont)
			{
				_font = _renderables.CreateFont(FontInfo);
				_hasFont = true;
			}
			return _font;
		}
	}
}
