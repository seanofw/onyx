using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BorderProperty : StyleProperty
	{
		public Measure BorderWidth { get; init; }
		public BorderStyle BorderStyle { get; init; }
		public Color32? BorderColor { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (BorderWidth.Units != default)
				pieces.Add(BorderWidth.ToString());
			if (BorderStyle != default)
				pieces.Add(BorderStyle.ToString().Hyphenize());
			if (BorderColor.HasValue)
				pieces.Add(BorderColor.Value.ToString());

			return string.Join(" ", pieces);
		}

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (BorderWidth.Units != default)
			{
				yield return Derive<BorderTopWidthProperty>()
					with { Kind = KnownPropertyKind.BorderTopWidth, Width = BorderWidth };
				yield return Derive<BorderRightWidthProperty>()
					with { Kind = KnownPropertyKind.BorderRightWidth, Width = BorderWidth };
				yield return Derive<BorderBottomWidthProperty>()
					with { Kind = KnownPropertyKind.BorderBottomWidth, Width = BorderWidth };
				yield return Derive<BorderLeftWidthProperty>()
					with { Kind = KnownPropertyKind.BorderLeftWidth, Width = BorderWidth };
			}

			if (BorderStyle != default)
			{
				yield return Derive<BorderTopStyleProperty>()
					with { Kind = KnownPropertyKind.BorderTopStyle, Style = BorderStyle };
				yield return Derive<BorderRightStyleProperty>()
					with { Kind = KnownPropertyKind.BorderRightStyle, Style = BorderStyle };
				yield return Derive<BorderBottomStyleProperty>()
					with { Kind = KnownPropertyKind.BorderBottomStyle, Style = BorderStyle };
				yield return Derive<BorderLeftStyleProperty>()
					with { Kind = KnownPropertyKind.BorderLeftStyle, Style = BorderStyle };
			}

			if (BorderColor.HasValue)
			{
				yield return Derive<BorderTopColorProperty>()
					with { Kind = KnownPropertyKind.BorderTopColor, Color = BorderColor.Value };
				yield return Derive<BorderRightColorProperty>()
					with { Kind = KnownPropertyKind.BorderRightColor, Color = BorderColor.Value };
				yield return Derive<BorderBottomColorProperty>()
					with { Kind = KnownPropertyKind.BorderBottomColor, Color = BorderColor.Value };
				yield return Derive<BorderLeftColorProperty>()
					with { Kind = KnownPropertyKind.BorderLeftColor, Color = BorderColor.Value };
			}
		}

		public override bool IsDecomposable => true;
	}
}
