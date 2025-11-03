namespace Onyx.Css.Types
{
	public class ShadowClass
	{
		public Measure OffsetX { get; }
		public Measure OffsetY { get; }
		public Measure Blur { get; }
		public Measure Spread { get; }
		public Color32? Color { get; }
		public bool Inset { get; }

		public static ShadowClass Default { get; } = new ShadowClass();

		public ShadowClass()
		{
			OffsetX = Measure.Zero;
			OffsetY = Measure.Zero;
			Blur = Measure.Zero;
			Spread = Measure.Zero;

			Color = null;
			Inset = false;
		}

		public ShadowClass(Measure offsetX, Measure offsetY, Measure blur, Measure spread, Color32? color, bool inset)
		{
			OffsetX = offsetX;
			OffsetY = offsetY;
			Blur = blur;
			Spread = spread;

			Color = color;
			Inset = inset;
		}

		public static implicit operator Shadow(ShadowClass shadow)
			=> new Shadow(shadow.OffsetX, shadow.OffsetY, shadow.Blur, shadow.Spread, shadow.Color, shadow.Inset);

		public ShadowClass WithOffsetX(Measure offsetX)
			=> new ShadowClass(offsetX, OffsetY, Blur, Spread, Color, Inset);
		public ShadowClass WithOffsetY(Measure offsetY)
			=> new ShadowClass(OffsetX, offsetY, Blur, Spread, Color, Inset);
		public ShadowClass WithBlur(Measure blur)
			=> new ShadowClass(OffsetX, OffsetY, blur, Spread, Color, Inset);
		public ShadowClass WithSpread(Measure spread)
			=> new ShadowClass(OffsetX, OffsetY, Blur, spread, Color, Inset);
		public ShadowClass WithColor(Color32? color)
			=> new ShadowClass(OffsetX, OffsetY, Blur, Spread, color, Inset);
		public ShadowClass WithInset(bool inset)
			=> new ShadowClass(OffsetX, OffsetY, Blur, Spread, Color, inset);

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (OffsetX.Units != Units.None)
				pieces.Add(OffsetX.ToString());
			if (OffsetY.Units != Units.None)
				pieces.Add(OffsetY.ToString());
			if (Blur.Units != Units.None)
				pieces.Add(Blur.ToString());
			if (Spread.Units != Units.None)
				pieces.Add(Spread.ToString());

			if (Color.HasValue)
				pieces.Add(Color.Value.ToString());

			return string.Join(" ", pieces);
		}
	}
}
