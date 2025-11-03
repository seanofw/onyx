using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class UnicodeBidiProperty : StyleProperty
	{
		public UnicodeBidi UnicodeBidi { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithUnicodeBidi(UnicodeBidi);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithUnicodeBidi(source.UnicodeBidi);

		public override string ToString()
			=> UnicodeBidi.ToString().Hyphenize();
	}
}
