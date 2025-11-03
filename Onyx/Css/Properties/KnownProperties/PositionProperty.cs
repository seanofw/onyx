using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class PositionProperty : StyleProperty
	{
		public PositionKind Position { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithPosition(Position);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithPosition(source.Position);

		public override string ToString()
			=> Position.ToString().Hyphenize();
	}
}
