using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class TextTransformProperty : StyleProperty
	{
		public TextTransform TextTransform { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithTextTransform(TextTransform);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithTextTransform(source.TextTransform);

		public override string ToString()
			=> TextTransform.ToString().Hyphenize();
	}
}
