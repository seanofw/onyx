namespace Onyx.Css.Types
{
	public record struct BackgroundSize
	{
		public BackgroundSizeKind Kind { get; init; }
		public bool AutoX { get; init; }
		public bool AutoY { get; init; }
		public Measure X { get; init; }
		public Measure Y { get; init; }

		public static BackgroundSize Default { get; }
			= new BackgroundSize { AutoX = true, AutoY = true };

		public override string ToString()
			=> Kind switch
			{
				BackgroundSizeKind.Cover => "cover",
				BackgroundSizeKind.Contain => "contain",

				_ => (AutoX ? "auto" : X.ToString())
					+ (AutoY || Y != default
						? " " + (AutoY ? "auto" : Y.ToString())
						: string.Empty),
			};
	}

	// TODO: FIXME: We use this annoying little stub class during parsing;
	// but the real version is the struct.
	internal record class BackgroundSizeClass
	{
		public BackgroundSizeKind Kind { get; init; }
		public bool AutoX { get; init; }
		public bool AutoY { get; init; }
		public Measure X { get; init; }
		public Measure Y { get; init; }

		public static implicit operator BackgroundSize(BackgroundSizeClass c)
			=> new BackgroundSize
			{
				Kind = c.Kind,
				AutoX = c.AutoX,
				AutoY = c.AutoY,
				X = c.X,
				Y = c.Y,
			};
	}
}
