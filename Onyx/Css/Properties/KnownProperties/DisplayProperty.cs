using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class DisplayProperty : StyleProperty
	{
		public DisplayKind Display { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithDisplay(Display);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithDisplay(source.Display);

		public override string ToString()
			=> Display.ToString().Hyphenize();
	}
}
