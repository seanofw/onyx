using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class ClearProperty : StyleProperty
	{
		public ClearMode ClearMode { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithClear(ClearMode);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithClear(source.Clear);

		public override string ToString()
			=> ClearMode.ToString().Hyphenize();
	}
}
