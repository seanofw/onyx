using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BorderWidthProperty : StyleProperty
	{
		public IReadOnlyList<Measure> Widths
		{
			get => _widths;
			init => _widths = value is ImmutableArray<Measure> array ? array
				: value is null ? ImmutableArray<Measure>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<Measure> _widths = ImmutableArray<Measure>.Empty;

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
			=> string.Join(" ", Widths.Select(w => w.ToString()));

		public BorderWidthProperty AddWidth(Measure width)
			=> this with { Widths = _widths.Add(width) };

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			Measure topWidth, rightWidth, bottomWidth, leftWidth;

			switch (Widths.Count)
			{
				case 0:
					yield break;
				case 1:
					topWidth = rightWidth = bottomWidth = leftWidth = Widths[0];
					break;
				case 2:
					topWidth = bottomWidth = Widths[0];
					leftWidth = rightWidth = Widths[1];
					break;
				case 3:
					topWidth = Widths[0];
					leftWidth = rightWidth = Widths[1];
					bottomWidth = Widths[2];
					break;
				case 4:
				default:
					topWidth = Widths[0];
					rightWidth = Widths[1];
					bottomWidth = Widths[2];
					leftWidth = Widths[3];
					break;
			}

			yield return Derive<BorderTopWidthProperty>() with
			{
				Kind = KnownPropertyKind.BorderTopWidth,
				Width = topWidth,
			};

			yield return Derive<BorderRightWidthProperty>() with
			{
				Kind = KnownPropertyKind.BorderRightWidth,
				Width = rightWidth,
			};

			yield return Derive<BorderBottomWidthProperty>() with
			{
				Kind = KnownPropertyKind.BorderBottomWidth,
				Width = bottomWidth,
			};

			yield return Derive<BorderLeftWidthProperty>() with
			{
				Kind = KnownPropertyKind.BorderLeftWidth,
				Width = leftWidth,
			};
		}
	}
}
