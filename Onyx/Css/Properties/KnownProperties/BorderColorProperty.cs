using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BorderColorProperty : StyleProperty
	{
		public IReadOnlyList<Color32> Colors
		{
			get => _colors;
			init => _colors = value is ImmutableArray<Color32> array ? array
				: value is null ? ImmutableArray<Color32>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<Color32> _colors = ImmutableArray<Color32>.Empty;

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
			=> string.Join(" ", Colors.Select(c => c.ToString()));

		public BorderColorProperty AddColor(Color32 color)
			=> this with { Colors = _colors.Add(color) };

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			Color32 topColor, rightColor, bottomColor, leftColor;

			switch (Colors.Count)
			{
				case 0:
					yield break;
				case 1:
					topColor = rightColor = bottomColor = leftColor = Colors[0];
					break;
				case 2:
					topColor = bottomColor = Colors[0];
					leftColor = rightColor = Colors[1];
					break;
				case 3:
					topColor = Colors[0];
					leftColor = rightColor = Colors[1];
					bottomColor = Colors[2];
					break;
				case 4:
				default:
					topColor = Colors[0];
					rightColor = Colors[1];
					bottomColor = Colors[2];
					leftColor = Colors[3];
					break;
			}

			yield return Derive<BorderTopColorProperty>() with
			{
				Kind = KnownPropertyKind.BorderTopColor,
				Color = topColor,
			};

			yield return Derive<BorderRightColorProperty>() with
			{
				Kind = KnownPropertyKind.BorderRightColor,
				Color = rightColor,
			};

			yield return Derive<BorderBottomColorProperty>() with
			{
				Kind = KnownPropertyKind.BorderBottomColor,
				Color = bottomColor,
			};

			yield return Derive<BorderLeftColorProperty>() with
			{
				Kind = KnownPropertyKind.BorderLeftColor,
				Color = leftColor,
			};
		}

		public override bool IsDecomposable => true;
	}
}
