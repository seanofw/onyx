using Onyx.Css.Types;

namespace Onyx.Rendering
{
	public readonly struct FontInfo
	{
		public string Name { get; }
		public double Size { get; }
		public FontStyle Style { get; }
		public int Weight { get; }

		public FontInfo(string name, double size, FontStyle style = FontStyle.Normal, int weight = 400)
		{
			Name = name;
			Size = size;
			Style = style;
			Weight = weight;
		}
	}
}
