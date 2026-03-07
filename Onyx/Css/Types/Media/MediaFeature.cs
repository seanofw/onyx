namespace Onyx.Css.Types.Media
{
	public enum MediaFeature : uint
	{
		Unknown = 0,

		Width,
		Height,
		AspectRatio,
		Orientation,

		Resolution,
		Scan,
		Grid,
		Update,
		OverflowBlock,
		OverflowInline,

		Color,
		ColorIndex,
		Monochrome,
		ColorGamut,

		Pointer,
		Hover,
		AnyPointer,
		AnyHover,

		_Last_,

		Min = 0x10000,
		Max = 0x20000,

		MinWidth = Width | Min,
		MaxWidth = Width | Max,
		MinHeight = Height | Min,
		MaxHeight = Height | Max,
		MinAspectRatio = AspectRatio | Min,
		MaxAspectRatio = AspectRatio | Max,

		MinResolution = Resolution | Min,
		MaxResolution = Resolution | Max,

		MinColor = Color | Min,
		MaxColor = Color | Max,
		MinColorIndex = ColorIndex | Min,
		MaxColorIndex = ColorIndex | Max,
		MinMonochrome = Monochrome | Min,
		MaxMonochrome = Monochrome | Max,
		MinColorGamut = ColorGamut | Min,
		MaxColorGamut = ColorGamut | Max,
	}
}
