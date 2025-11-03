using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FlexWrapProperty : StyleProperty
	{
		public FlexWrap Wrap { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFlexWrap(Wrap);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFlexWrap(source.FlexWrap);

		public override string ToString()
			=> Wrap.ToString().Hyphenize();
	}
}
