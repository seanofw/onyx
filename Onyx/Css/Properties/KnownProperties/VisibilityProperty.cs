using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class VisibilityProperty : StyleProperty
	{
		public VisibilityKind Visibility { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithVisibility(Visibility);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithVisibility(source.Visibility);

		public override string ToString()
			=> Visibility.ToString().Hyphenize();
	}
}
