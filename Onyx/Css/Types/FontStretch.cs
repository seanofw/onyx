namespace Onyx.Css.Types
{
	/// <summary>
	/// Predefined constants for the CSS font-stretch property, on a scale where 1.0
	/// is 100% (normal). 0.625 is 62.5% (extra-condensed), 2.0 is 200% (ultra-expanded).
	/// Arbitrary values in the CSS range of 0.5–2.0 are also valid.
	/// </summary>
	public static class FontStretch
	{
		/// <summary>0.5 (50%) — the most condensed keyword value.</summary>
		public const double UltraCondensed = 0.5;
		/// <summary>0.625 (62.5%)</summary>
		public const double ExtraCondensed = 0.625;
		/// <summary>0.75 (75%)</summary>
		public const double Condensed = 0.75;
		/// <summary>0.875 (87.5%)</summary>
		public const double SemiCondensed = 0.875;
		/// <summary>1.0 (100%) — the default stretch.</summary>
		public const double Normal = 1.0;
		/// <summary>1.125 (112.5%)</summary>
		public const double SemiExpanded = 1.125;
		/// <summary>1.25 (125%)</summary>
		public const double Expanded = 1.25;
		/// <summary>1.5 (150%)</summary>
		public const double ExtraExpanded = 1.5;
		/// <summary>2.0 (200%) — the most expanded keyword value.</summary>
		public const double UltraExpanded = 2.0;
	}
}
