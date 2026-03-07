namespace Onyx.Css.Types.Media
{
	/// <summary>
	/// This class describes the shape of display or print media.  It is separated
	/// from the rest of the media info because this can change live if a window is
	/// resized, while the rest of the media info generally never changes, so these
	/// structs are split into "may changes" and "never changes" bags.
	/// 
	/// Note that while 'display-width' and 'display-height' might actually be useful
	/// for Onyx developers, they're not included here because they're deprecated as
	/// of CSS Media Queries Level 4 for security reasons.  Which is a shame, but since
	/// we target web standards, we have no choice but to omit them too.
	/// </summary>
	public readonly struct MediaDimensions : IEquatable<MediaDimensions>
	{
		public Measure Width { get; }
		public Measure Height { get; }

		public double AspectRatio => Height.Units != Units.None && Height.Value != 0
			? Width.Value / Height.Value
			: 0.0;

		public MediaOrientation Orientation => AspectRatio > 1.0
			? MediaOrientation.Landscape
			: MediaOrientation.Portrait;

		public MediaDimensions(Measure width, Measure height)
		{
			if (width.Units != height.Units)
				throw new ArgumentException("Media width and height must use the same measurement.");

			Width = width;
			Height = height;
		}

		public override bool Equals(object? obj)
			=> obj is MediaDimensions other && Equals(other);

		public bool Equals(MediaDimensions other)
			=> Width == other.Width && Height == other.Height;

		public static bool operator ==(MediaDimensions a, MediaDimensions b)
			=> a.Equals(b);

		public static bool operator !=(MediaDimensions a, MediaDimensions b)
			=> !a.Equals(b);

		public override int GetHashCode()
			=> unchecked(Width.GetHashCode() * 65599 + Height.GetHashCode());

		public override string ToString()
			=> $"{Width} x {Height} ({Orientation}, {AspectRatio})";
	}
}
