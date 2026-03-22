using System.Globalization;
using Onyx.Extensions;

namespace Onyx.Css.Types
{
	public class CustomCursor : IEquatable<CustomCursor>
	{
		public string Uri { get; }
		public double HotspotX { get; }
		public double HotspotY { get; }

		public static CustomCursor Default { get; } = new CustomCursor();

		public CustomCursor()
		{
			Uri = string.Empty;
			HotspotX = 0;
			HotspotY = 0;
		}

		public CustomCursor(string uri, double hotspotX, double hotspotY)
		{
			Uri = uri;
			HotspotX = hotspotX;
			HotspotY = hotspotY;
		}

		public CustomCursor WithUri(string uri)
			=> new CustomCursor(uri, HotspotX, HotspotY);
		public CustomCursor WithHotspotX(double hotspotX)
			=> new CustomCursor(Uri, hotspotX, HotspotY);
		public CustomCursor WithHotspotY(double hotspotY)
			=> new CustomCursor(Uri, HotspotX, hotspotY);

		public override bool Equals(object? obj)
			=> obj is CustomCursor other && Equals(other);
		public bool Equals(CustomCursor? other)
			=> ReferenceEquals(other, this) ? true
				: ReferenceEquals(other, null) ? false
				: Uri == other.Uri && HotspotX == other.HotspotX && HotspotY == other.HotspotY;
		public override int GetHashCode()
			=> unchecked(((Uri.GetHashCode() * 65599) + HotspotX.GetHashCode()) * 65599 + HotspotY.GetHashCode());
		public static bool operator ==(CustomCursor? left, CustomCursor? right)
			=> ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
		public static bool operator !=(CustomCursor? left, CustomCursor? right)
			=> ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : !left.Equals(right);

		public override string ToString()
			=> "url(\"" + Uri.ToString().AddCSlashes()
				+ "\") " + (HotspotX != 0 && HotspotY != 0
					? HotspotX.ToString(CultureInfo.InvariantCulture)
						+ " " + HotspotY.ToString(CultureInfo.InvariantCulture)
					: string.Empty);
	}
}
