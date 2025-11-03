using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class ListStylePositionProperty : StyleProperty
	{
		public ListStylePosition Position { get; init; }

		public static ListStylePositionProperty Default { get; } = new ListStylePositionProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithListStylePosition(Position);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithListStylePosition(source.ListStylePosition);

		public override string ToString()
			=> Position.ToString().Hyphenize();
	}
}
