using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedFontStyle : IDefaultInheritedStyle
	{
		public Color32 Color { get; }
		public IReadOnlyList<FontFamily> Families => _families;
		private readonly ImmutableArray<FontFamily> _families;
		public SpecialFontKind SpecialFont { get; }
		public Measure Size { get; }
		public FontStyle Style { get; }
		public int Weight { get; }
		public FontVariant Variant { get; }
		public IReadOnlyList<Shadow> TextShadows => _textShadows;
		private readonly ImmutableArray<Shadow> _textShadows;

		public static ComputedFontStyle Default { get; } = new ComputedFontStyle(
			["serif"], default, new Measure(Units.Pixels, 14), FontStyle.Normal, 400, FontVariant.Normal,
			Color32.Black, null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedFontStyle(IEnumerable<FontFamily>? families, SpecialFontKind specialFont,
			Measure size, FontStyle style, int weight, FontVariant variant,
			Color32 color, IEnumerable<Shadow>? textShadows)
		{
			Color = color;

			_families = families is ImmutableArray<FontFamily> array ? array
					: families is null ? ImmutableArray<FontFamily>.Empty
					: families.ToImmutableArray();
			SpecialFont = specialFont;
			Size = size;
			Style = style;
			Weight = weight;
			Variant = variant;

			_textShadows = textShadows is ImmutableArray<Shadow> array2 ? array2
				: textShadows is null ? ImmutableArray<Shadow>.Empty
				: textShadows.ToImmutableArray();
		}

		public ComputedFontStyle WithFamilies(IEnumerable<FontFamily>? families)
			=> new ComputedFontStyle(families, SpecialFont, Size, Style, Weight, Variant, Color, TextShadows);
		public ComputedFontStyle WithSpecialFont(SpecialFontKind specialFont)
			=> new ComputedFontStyle(Families, specialFont, Size, Style, Weight, Variant, Color, TextShadows);
		public ComputedFontStyle WithSize(Measure size)
			=> new ComputedFontStyle(Families, SpecialFont, size, Style, Weight, Variant, Color, TextShadows);
		public ComputedFontStyle WithStyle(FontStyle style)
			=> new ComputedFontStyle(Families, SpecialFont, Size, style, Weight, Variant, Color, TextShadows);
		public ComputedFontStyle WithWeight(int weight)
			=> new ComputedFontStyle(Families, SpecialFont, Size, Style, weight, Variant, Color, TextShadows);
		public ComputedFontStyle WithVariant(FontVariant variant)
			=> new ComputedFontStyle(Families, SpecialFont, Size, Style, Weight, variant, Color, TextShadows);
		public ComputedFontStyle WithColor(Color32 color)
			=> new ComputedFontStyle(Families, SpecialFont, Size, Style, Weight, Variant, color, TextShadows);
		public ComputedFontStyle WithTextShadows(IEnumerable<Shadow>? textShadows)
			=> new ComputedFontStyle(Families, SpecialFont, Size, Style, Weight, Variant, Color, textShadows);
	}
}