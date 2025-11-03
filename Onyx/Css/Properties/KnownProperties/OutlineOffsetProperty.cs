using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OutlineOffsetProperty : StyleProperty
	{
		public Measure Offset { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOutlineOffset(Offset);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOutlineOffset(source.OutlineOffset);

		public override string ToString()
			=> Offset.ToString();
	}
}
