using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class ResizeProperty : StyleProperty
	{
		public ResizeKind Resize { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithResize(Resize);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithResize(source.Resize);

		public override string ToString()
			=> Resize.ToString().Hyphenize();
	}
}
