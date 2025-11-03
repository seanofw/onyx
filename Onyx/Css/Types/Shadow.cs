namespace Onyx.Css.Types
{
	public readonly record struct Shadow : IEquatable<Shadow>
	{
		private readonly Color32 _color;
		private readonly float _xValue, _yValue, _blurValue, _spreadValue;
		private readonly Units _xUnits, _yUnits, _blurUnits, _spreadUnits;
		private readonly ShadowFlags _shadowFlags;

		public Color32? Color => (_shadowFlags & ShadowFlags.HasColor) != 0 ? _color : null;
		public bool Inset => (_shadowFlags & ShadowFlags.Inset) != 0;
		public Measure OffsetX => new Measure(_xUnits, _xValue);
		public Measure OffsetY => new Measure(_yUnits, _yValue);
		public Measure Blur => new Measure(_blurUnits, _blurValue);
		public Measure Spread => new Measure(_spreadUnits, _spreadValue);

		public static Shadow Default { get; } = new Shadow(
			Measure.Zero, Measure.Zero, Measure.Zero, Measure.Zero,
			null, false);

		public Shadow(Measure offsetX, Measure offsetY, Measure blur, Measure spread, Color32? color, bool inset)
		{
			_xUnits = offsetX.Units;
			_xValue = (float)offsetX.Value;

			_yUnits = offsetY.Units;
			_yValue = (float)offsetY.Value;

			_blurUnits = blur.Units;
			_blurValue = (float)blur.Value;

			_spreadUnits = spread.Units;
			_spreadValue = (float)spread.Value;

			_color = color.GetValueOrDefault();
			_shadowFlags =
				  (color.HasValue ? ShadowFlags.HasColor : 0)
				| (inset ? ShadowFlags.Inset : 0);
		}

		public Shadow WithOffsetX(Measure offsetX)
			=> new Shadow(offsetX, OffsetY, Blur, Spread, Color, Inset);
		public Shadow WithOffsetY(Measure offsetY)
			=> new Shadow(OffsetX, offsetY, Blur, Spread, Color, Inset);
		public Shadow WithBlur(Measure blur)
			=> new Shadow(OffsetX, OffsetY, blur, Spread, Color, Inset);
		public Shadow WithSpread(Measure spread)
			=> new Shadow(OffsetX, OffsetY, Blur, spread, Color, Inset);
		public Shadow WithColor(Color32? color)
			=> new Shadow(OffsetX, OffsetY, Blur, Spread, color, Inset);
		public Shadow WithInset(bool inset)
			=> new Shadow(OffsetX, OffsetY, Blur, Spread, Color, inset);

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
