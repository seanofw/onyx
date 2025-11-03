using System.Globalization;
using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FontWeightProperty : StyleProperty
	{
		public FontWeightName Name { get; init; }
		public int Amount { get; init; }

		public static FontWeightProperty Default = new FontWeightProperty();

		public override ComputedStyle Apply(ComputedStyle style)
		{
			int weight;

			if (Name != default)
			{
				weight = Name switch
				{
					FontWeightName.Normal => 400,
					FontWeightName.Bold => 700,
					FontWeightName.Bolder => Math.Min(style.FontWeight + 100, 900),
					FontWeightName.Lighter => Math.Max(style.FontWeight - 100, 100),
					_ => throw new InvalidOperationException("Invalid FontWeightName: " + Name),
				};
			}
			else weight = Amount;

			return style.WithFontWeight(weight);
		}

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFontWeight(source.FontWeight);

		public override string ToString()
			=> Name != default ? Name.ToString().Hyphenize()
				: Amount.ToString(CultureInfo.InvariantCulture);
	}
}