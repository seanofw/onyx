namespace Onyx.Rendering
{
	public readonly struct LineStyle
	{
		public uint Dashes { get; }		// Bitmask, where each bit represents a unit square of size 'Thickness'

		public static LineStyle Solid  { get; } = new LineStyle(dashes: 0xFFFFFFFFU);
		public static LineStyle Dashed { get; } = new LineStyle(dashes: 0x0F0F0F0FU);

		public LineStyle(uint dashes)
		{
			Dashes = dashes;
		}
	}
}
