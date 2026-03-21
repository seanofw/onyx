using Onyx.Css.Types;

namespace Onyx.Rendering
{
	/// <summary>
	/// The family name, size, stretch, style, and weight that identify a font request.
	/// Used as a cache key by the layout engine and as the argument to IRenderables.CreateFont().
	/// </summary>
	public readonly struct FontInfo : IEquatable<FontInfo>
	{
		/// <summary>The font family name, e.g. "Arial" or "Times New Roman".</summary>
		public string Name { get; }

		/// <summary>The font size in CSS pixels. Rounded to three decimal places.</summary>
		public double Size => (double)_size;
		private readonly float _size;

		/// <summary>
		/// The font stretch on a scale where 1.0 is 100% (normal), 0.625 is 62.5%
		/// (extra-condensed), and 2.0 is 200% (ultra-expanded). Rounded to three decimal
		/// places so that exact comparisons are reliable. Use the FontStretch constants
		/// for the CSS keyword values.
		/// </summary>
		public double Stretch => (double)_stretch;
		private readonly float _stretch;

		/// <summary>Normal, italic, or oblique.</summary>
		public FontStyle Style { get; }

		/// <summary>
		/// The font weight, where 400 is regular and 700 is bold. Values outside the
		/// CSS range of 1–1000 are clamped by the backend.
		/// </summary>
		public int Weight => (int)_weight;
		private readonly short _weight;

		/// <summary>
		/// Construct a new FontInfo.
		/// </summary>
		/// <param name="name">The font family name. Must not be null or empty.</param>
		/// <param name="size">The font size in CSS pixels. Must be positive. Rounded to three decimal places.</param>
		/// <param name="stretch">
		/// CSS font-stretch on a scale where 1.0 is normal. Rounded to three decimal places.
		/// Use FontStretch constants for keyword values. Defaults to FontStretch.Normal.
		/// </param>
		/// <param name="style">Normal, italic, or oblique. Defaults to Normal.</param>
		/// <param name="weight">CSS font weight. 400 is regular, 700 is bold. Defaults to 400.</param>
		public FontInfo(string name, double size, double stretch = FontStretch.Normal,
			FontStyle style = FontStyle.Normal, int weight = 400)
		{
			Name = name;
			_size = (float)Math.Round(size, 3);
			_stretch = (float)Math.Round(stretch, 3);
			Style = style;
			_weight = (short)weight;
		}

		/// <summary>Copy this FontInfo, replacing Name.</summary>
		public FontInfo WithName(string name)
			=> new FontInfo(name, Size, Stretch, Style, Weight);
		/// <summary>Copy this FontInfo, replacing Size.</summary>
		public FontInfo WithSize(double size)
			=> new FontInfo(Name, size, Stretch, Style, Weight);
		/// <summary>Copy this FontInfo, replacing Stretch.</summary>
		public FontInfo WithStretch(double stretch)
			=> new FontInfo(Name, Size, stretch, Style, Weight);
		/// <summary>Copy this FontInfo, replacing Style.</summary>
		public FontInfo WithStyle(FontStyle style)
			=> new FontInfo(Name, Size, Stretch, style, Weight);
		/// <summary>Copy this FontInfo, replacing Weight.</summary>
		public FontInfo WithWeight(int weight)
			=> new FontInfo(Name, Size, Stretch, Style, weight);

		public bool Equals(FontInfo other)
			=> Name == other.Name && Size == other.Size && Stretch == other.Stretch
				&& Style == other.Style && Weight == other.Weight;

		public override bool Equals(object? obj)
			=> obj is FontInfo other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = (hashCode * 65599) + Name.GetHashCode();
				hashCode = (hashCode * 65599) + Size.GetHashCode();
				hashCode = (hashCode * 65599) + Stretch.GetHashCode();
				hashCode = (hashCode * 65599) + (int)Style;
				hashCode = (hashCode * 65599) + (int)_weight;
				return hashCode;
			}
		}

		public static bool operator ==(FontInfo left, FontInfo right)
			=> left.Equals(right);
		public static bool operator !=(FontInfo left, FontInfo right)
			=> !left.Equals(right);
	}
}
