using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OutlineStyleProperty : StyleProperty
	{
		public BorderStyle Style { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOutlineStyle(Style);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOutlineStyle(source.OutlineStyle);

		public override string ToString()
			=> Style.ToString().Hyphenize();
	}
}
