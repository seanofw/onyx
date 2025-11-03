using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BorderStyleProperty : StyleProperty
	{
		public IReadOnlyList<BorderStyle> Styles
		{
			get => _styles;
			init => _styles = value is ImmutableArray<BorderStyle> array ? array
				: value is null ? ImmutableArray<BorderStyle>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<BorderStyle> _styles = ImmutableArray<BorderStyle>.Empty;

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
			=> string.Join(" ", Styles.Select(s => s.ToString().Hyphenize()));

		public BorderStyleProperty AddStyle(BorderStyle style)
			=> this with { Styles = _styles.Add(style) };

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			BorderStyle topStyle, rightStyle, bottomStyle, leftStyle;

			switch (Styles.Count)
			{
				case 0:
					yield break;
				case 1:
					topStyle = rightStyle = bottomStyle = leftStyle = Styles[0];
					break;
				case 2:
					topStyle = bottomStyle = Styles[0];
					leftStyle = rightStyle = Styles[1];
					break;
				case 3:
					topStyle = Styles[0];
					leftStyle = rightStyle = Styles[1];
					bottomStyle = Styles[2];
					break;
				case 4:
				default:
					topStyle = Styles[0];
					rightStyle = Styles[1];
					bottomStyle = Styles[2];
					leftStyle = Styles[3];
					break;
			}

			yield return Derive<BorderTopStyleProperty>() with
			{
				Kind = KnownPropertyKind.BorderTopStyle,
				Style = topStyle,
			};

			yield return Derive<BorderRightStyleProperty>() with
			{
				Kind = KnownPropertyKind.BorderRightStyle,
				Style = rightStyle,
			};

			yield return Derive<BorderBottomStyleProperty>() with
			{
				Kind = KnownPropertyKind.BorderBottomStyle,
				Style = bottomStyle,
			};

			yield return Derive<BorderLeftStyleProperty>() with
			{
				Kind = KnownPropertyKind.BorderLeftStyle,
				Style = leftStyle,
			};
		}

		public override bool IsDecomposable => true;
	}
}
