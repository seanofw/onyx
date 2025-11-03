using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class TextDecorationProperty : StyleProperty
	{
		public TextDecorationLineKind? TextDecoration { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> TextDecoration.HasValue ? style.WithTextDecoration(TextDecoration.Value) : style;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithTextDecoration(source.TextDecoration);

		public override string ToString()
		{
			if (!TextDecoration.HasValue)
				return string.Empty;

			if (TextDecoration.Value == default)
				return "none";

			List<string> pieces = new List<string>();

			if ((TextDecoration.Value & Types.TextDecorationLineKind.Underline) != 0)
				pieces.Add(Types.TextDecorationLineKind.Underline.ToString().Hyphenize());

			if ((TextDecoration.Value & Types.TextDecorationLineKind.Overline) != 0)
				pieces.Add(Types.TextDecorationLineKind.Overline.ToString().Hyphenize());

			if ((TextDecoration.Value & Types.TextDecorationLineKind.LineThrough) != 0)
				pieces.Add(Types.TextDecorationLineKind.LineThrough.ToString().Hyphenize());

			if ((TextDecoration.Value & Types.TextDecorationLineKind.Blink) != 0)
				pieces.Add(Types.TextDecorationLineKind.Blink.ToString().Hyphenize());

			return string.Join(" ", pieces);
		}
	}
}
