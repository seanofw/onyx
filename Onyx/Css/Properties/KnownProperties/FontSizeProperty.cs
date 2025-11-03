using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FontSizeProperty : StyleProperty
	{
		public Measure Measure { get; init; }
		public AbsoluteFontSize AbsoluteFontSize { get; init; }
		public RelativeFontSize RelativeFontSize { get; init; }

		public static FontSizeProperty Default = new FontSizeProperty();

		public override ComputedStyle Apply(ComputedStyle style)
		{
			Measure measure;

			if (AbsoluteFontSize != default)
			{
				measure = AbsoluteFontSize switch
				{
					AbsoluteFontSize.XxSmall => new Measure(Units.Points, 6),
					AbsoluteFontSize.XSmall => new Measure(Units.Points, 8),
					AbsoluteFontSize.Small => new Measure(Units.Points, 10),
					AbsoluteFontSize.Medium => new Measure(Units.Points, 12),
					AbsoluteFontSize.Large => new Measure(Units.Points, 16),
					AbsoluteFontSize.XLarge => new Measure(Units.Points, 20),
					AbsoluteFontSize.XxLarge => new Measure(Units.Points, 24),
					_ => throw new InvalidOperationException("Invalid AbsoluteFontSize: " + AbsoluteFontSize),
				};
			}
			else if (RelativeFontSize != default)
			{
				measure = RelativeFontSize switch
				{
					RelativeFontSize.Smaller => style.FontSize * 0.8,
					RelativeFontSize.Larger => style.FontSize * 1.25,
					_ => throw new InvalidOperationException("Invalid RelativeFontSize: " + RelativeFontSize),
				};
			}
			else
			{
				measure = Measure;
			}

			return style.WithFontSize(measure);
		}

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFontSize(source.FontSize);

		public override string ToString()
			=> AbsoluteFontSize != default ? AbsoluteFontSize.ToString().Hyphenize()
				: RelativeFontSize != default ? RelativeFontSize.ToString().Hyphenize()
				: Measure.ToString();
	}
}