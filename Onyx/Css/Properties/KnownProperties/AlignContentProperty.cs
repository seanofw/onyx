using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class AlignContentProperty : StyleProperty
	{
		public AlignContentKind AlignContent { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithAlignContent(AlignContent);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithAlignContent(source.AlignContent);

		public override string ToString()
			=> AlignContent.ToString().Hyphenize();
	}
}
