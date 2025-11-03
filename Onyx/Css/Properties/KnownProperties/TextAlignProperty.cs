using Onyx.Css.Computed;
using Onyx.Extensions;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class TextAlignProperty : StyleProperty
	{
		public TextAlign TextAlign { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithTextAlign(TextAlign);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithTextAlign(source.TextAlign);

		public override string ToString()
			=> TextAlign.ToString().Hyphenize();
	}
}
