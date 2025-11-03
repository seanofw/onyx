using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class JustifyContentProperty : StyleProperty
	{
		public JustifyContentKind JustifyContent { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithJustifyContent(JustifyContent);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithJustifyContent(source.JustifyContent);

		public override string ToString()
			=> JustifyContent.ToString().Hyphenize();
	}
}
