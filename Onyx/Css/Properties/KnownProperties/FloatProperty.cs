using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FloatProperty : StyleProperty
	{
		public FloatMode Float { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFloat(Float);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFloat(source.Float);

		public override string ToString()
			=> Float.ToString().Hyphenize();
	}
}
