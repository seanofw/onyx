using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OutlineProperty : StyleProperty
	{
		public BorderStyle Style { get; init; }
		public Measure Width { get; init; }
		public Color32? Color { get; init; }
		public bool Invert { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (Invert)
				pieces.Add("invert");
			else if (Color.HasValue)
				pieces.Add(Color.Value.ToString());

			if (Style != default)
				pieces.Add(Style.ToString().Hyphenize());

			if (Width.Units != default)
				pieces.Add(Width.ToString());

			return string.Join(" ", pieces);
		}

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (Invert)
				yield return Derive<OutlineColorProperty>() with
				{
					Kind = KnownPropertyKind.OutlineColor,
					Invert = true,
				};
			else if (Color.HasValue)
				yield return Derive<OutlineColorProperty>() with
				{
					Kind = KnownPropertyKind.OutlineColor,
					Color = Color.Value,
				};

			if (Style != default)
				yield return Derive<OutlineStyleProperty>() with
				{
					Kind = KnownPropertyKind.OutlineStyle,
					Style = Style,
				};

			if (Width.Units != default)
				yield return Derive<OutlineWidthProperty>() with
				{
					Kind = KnownPropertyKind.OutlineWidth,
					Width = Width,
				};
		}

		public override bool IsDecomposable => true;
	}
}
