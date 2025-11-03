using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class DirectionProperty : StyleProperty
	{
		public WritingDirection Direction { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithDirection(Direction);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithDirection(source.Direction);

		public override string ToString()
			=> Direction.ToString().Hyphenize();
	}
}
