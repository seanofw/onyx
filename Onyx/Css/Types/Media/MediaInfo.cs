namespace Onyx.Css.Types.Media
{
	/// <summary>
	/// Information about the current media, excluding its dimensions.
	/// </summary>
	public readonly struct MediaInfo
	{
		/// <summary>
		/// What type of media this is.
		/// </summary>
		public readonly MediaType Type { get; }

		/// <summary>
		/// How quickly the media can update (ideally 'fast'!).
		/// </summary>
		public readonly MediaUpdateMode UpdateMode { get; }

		/// <summary>
		/// Used to determine which of 'color', 'monochrome', or 'color-index' will return a value.
		/// The other media queries will return 0.
		/// </summary>
		public readonly MediaColorMode ColorMode { get; }

		/// <summary>
		/// If a truecolor display, display depth in bits per single component; typically 8.
		/// If a monochrome display, the number of bits of color depth in the brightness channel.
		/// If an indexed-color display, the number of palette entries.
		/// Used to satisfy the 'color', 'monochrome', and 'color-index' media queries.
		/// </summary>
		public readonly ushort ColorDepth { get; }

		/// <summary>
		/// How this media handles vertical overflow.
		/// </summary>
		public readonly MediaOverflowMode OverflowBlock { get; }

		/// <summary>
		/// How this media handles horizontal overflow.
		/// </summary>
		public readonly MediaOverflowMode OverflowInline { get; }

		/// <summary>
		/// Whether this media supports a pointer, and if so, how precise it is.
		/// </summary>
		public readonly MediaPointerKind PointerKind { get; }

		/// <summary>
		/// Whether this media supports pointer hover.
		/// </summary>
		public readonly MediaHoverKind HoverKind { get; }

		/// <summary>
		/// The 'color' media query value.
		/// </summary>
		public int Color => ColorMode == MediaColorMode.Truecolor ? ColorDepth : 0;

		/// <summary>
		/// The 'monochrome' media query value.
		/// </summary>
		public int Monochrome => ColorMode == MediaColorMode.Monochrome ? ColorDepth : 0;

		/// <summary>
		/// The 'color-index' media query value.
		/// </summary>
		public int ColorIndex => ColorMode == MediaColorMode.Paletted ? ColorDepth : 0;

		// 'color-gamut' media query is not currently supported.
		// 'resolution' media query is not currently supported.
		// 'scan' media query is not likely to ever be supported.
		// 'grid' media query always returns 0 in Onyx.

		public MediaInfo(MediaType type = default,
			MediaUpdateMode updateMode = default,
			MediaColorMode colorMode = default,
			int colorDepth = 0,
			MediaOverflowMode overflowBlock = default,
			MediaOverflowMode overflowInline = default,
			MediaPointerKind pointerKind = default,
			MediaHoverKind hoverKind = default)
		{
			Type = type;
			UpdateMode = updateMode;
			ColorMode = colorMode;
			ColorDepth = (colorDepth & 0xFFFF0000U) == 0
				? (ushort)colorDepth
				: throw new ArgumentOutOfRangeException("Color depth must be from 0 to 65535.");
			OverflowBlock = overflowBlock;
			OverflowInline = overflowInline;
			PointerKind = pointerKind;
			HoverKind = hoverKind;
		}

		public override string ToString()
			=> $"{Type}, {ColorMode}:{ColorDepth}, update:{UpdateMode}, overflow:{OverflowInline}/{OverflowBlock}, pointer:{PointerKind}/{HoverKind}";
	}
}
