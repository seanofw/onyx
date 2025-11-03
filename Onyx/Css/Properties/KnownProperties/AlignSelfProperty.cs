using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class AlignSelfProperty : StyleProperty
	{
		public AlignSelfKind AlignSelf { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithAlignSelf(AlignSelf);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithAlignSelf(source.AlignSelf);

		public override string ToString()
			=> AlignSelf.ToString().Hyphenize();
	}
}
