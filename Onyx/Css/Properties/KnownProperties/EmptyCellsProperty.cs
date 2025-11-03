using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class EmptyCellsProperty : StyleProperty
	{
		public EmptyCellsMode EmptyCells { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithEmptyCells(EmptyCells);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithEmptyCells(source.EmptyCells);

		public override string ToString()
			=> EmptyCells.ToString().Hyphenize();
	}
}
