using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class BorderEdgeProperty : StyleProperty
	{
		public Measure Width { get; init; }
		public BorderStyle Style { get; init; }
		public Color32? Color { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (Width.Units != Units.None)
				pieces.Add(Width.ToString().Hyphenize());

			if (Style != BorderStyle.None)
				pieces.Add(Style.ToString().Hyphenize());

			if (Color.HasValue)
				pieces.Add(Color.Value.ToString());

			return string.Join(" ", pieces);
		}
	}

	public sealed record class BorderTopProperty : BorderEdgeProperty
	{
		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (Width.Units != Units.None)
				yield return Derive<BorderTopWidthProperty>()
					with { Kind = KnownPropertyKind.BorderTopWidth, Width = Width };

			if (Style != BorderStyle.None)
				yield return Derive<BorderTopStyleProperty>()
					with { Kind = KnownPropertyKind.BorderTopStyle, Style = Style };

			if (Color.HasValue)
				yield return Derive<BorderTopColorProperty>()
					with { Kind = KnownPropertyKind.BorderTopColor, Color = Color.Value };
		}

		public override bool IsDecomposable => true;
	}

	public sealed record class BorderRightProperty : BorderEdgeProperty
	{
		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (Width.Units != Units.None)
				yield return Derive<BorderRightWidthProperty>()
					with { Kind = KnownPropertyKind.BorderRightWidth, Width = Width };

			if (Style != BorderStyle.None)
				yield return Derive<BorderRightStyleProperty>()
					with { Kind = KnownPropertyKind.BorderRightStyle, Style = Style };

			if (Color.HasValue)
				yield return Derive<BorderRightColorProperty>()
					with { Kind = KnownPropertyKind.BorderRightColor, Color = Color.Value };
		}
	}

	public sealed record class BorderBottomProperty : BorderEdgeProperty
	{
		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (Width.Units != Units.None)
				yield return Derive<BorderBottomWidthProperty>()
					with { Kind = KnownPropertyKind.BorderBottomWidth, Width = Width };

			if (Style != BorderStyle.None)
				yield return Derive<BorderBottomStyleProperty>()
					with { Kind = KnownPropertyKind.BorderBottomStyle, Style = Style };

			if (Color.HasValue)
				yield return Derive<BorderBottomColorProperty>()
					with { Kind = KnownPropertyKind.BorderBottomColor, Color = Color.Value };
		}
	}

	public sealed record class BorderLeftProperty : BorderEdgeProperty
	{
		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (Width.Units != Units.None)
				yield return Derive<BorderLeftWidthProperty>()
					with { Kind = KnownPropertyKind.BorderLeftWidth, Width = Width };

			if (Style != BorderStyle.None)
				yield return Derive<BorderLeftStyleProperty>()
					with { Kind = KnownPropertyKind.BorderLeftStyle, Style = Style };

			if (Color.HasValue)
				yield return Derive<BorderLeftColorProperty>()
					with { Kind = KnownPropertyKind.BorderLeftColor, Color = Color.Value };
		}
	}
}
