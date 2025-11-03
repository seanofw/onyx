using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class CaptionSideProperty : StyleProperty
	{
		public CaptionSide CaptionSide { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithCaptionSide(CaptionSide);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithCaptionSide(source.CaptionSide);

		public override string ToString()
			=> CaptionSide.ToString().Hyphenize();
	}
}
