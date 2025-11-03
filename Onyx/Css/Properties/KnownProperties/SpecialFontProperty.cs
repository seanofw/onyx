using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class SpecialFontProperty : StyleProperty
	{
		public SpecialFontKind SpecialFontKind { get; init; }

		public static SpecialFontProperty Default { get; } = new SpecialFontProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithSpecialFont(SpecialFontKind);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithSpecialFont(source.SpecialFont);

		public override string ToString()
			=> SpecialFontKind.ToString().Hyphenize();
	}
}
