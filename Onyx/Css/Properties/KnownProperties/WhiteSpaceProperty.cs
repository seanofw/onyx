using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class WhiteSpaceProperty : StyleProperty
	{
		public WhiteSpaceKind WhiteSpace { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithWhiteSpace(WhiteSpace);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithWhiteSpace(source.WhiteSpace);

		public override string ToString()
			=> WhiteSpace.ToString().Hyphenize();
	}
}
