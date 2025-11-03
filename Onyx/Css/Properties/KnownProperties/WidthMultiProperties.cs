using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class WidthMultiPropertyBase : StyleProperty
	{
		public IReadOnlyList<Width> Widths
		{
			get => _widths;
			init => _widths = value is ImmutableArray<Width> array ? array
				: value is null ? ImmutableArray<Width>.Empty
				: value.ToImmutableArray();
		}
		private protected readonly ImmutableArray<Width> _widths = ImmutableArray<Width>.Empty;

		public WidthMultiPropertyBase AddWidth(Width offset)
			=> this with { Widths = _widths.Add(offset) };

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		protected EdgeSizes ToEdgeSizes()
		{
			Width topWidth, rightWidth, bottomWidth, leftWidth;

			switch (Widths.Count)
			{
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

			return new EdgeSizes(topWidth, rightWidth, bottomWidth, leftWidth);
		}

		public override bool IsDecomposable => true;

		public override string ToString()
			=> string.Join(" ", Widths.Select(o => o.ToString()));
	}

	public sealed record class MarginProperty : WidthMultiPropertyBase
	{
		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			EdgeSizes edgeSizes = ToEdgeSizes();

			yield return Derive<MarginTopProperty>() with
			{
				Kind = KnownPropertyKind.MarginTop,
				Width = new Width(edgeSizes.Top, false),
			};
			yield return Derive<MarginRightProperty>() with
			{
				Kind = KnownPropertyKind.MarginRight,
				Width = new Width(edgeSizes.Right, false),
			};
			yield return Derive<MarginBottomProperty>() with
			{
				Kind = KnownPropertyKind.MarginBottom,
				Width = new Width(edgeSizes.Bottom, false),
			};
			yield return Derive<MarginLeftProperty>() with
			{
				Kind = KnownPropertyKind.MarginLeft,
				Width = new Width(edgeSizes.Left, false),
			};
		}
	}

	public record class PaddingProperty : WidthMultiPropertyBase
	{
		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			EdgeSizes edgeSizes = ToEdgeSizes();

			yield return Derive<PaddingTopProperty>() with
			{
				Kind = KnownPropertyKind.PaddingTop,
				Width = new Width(edgeSizes.Top, false),
			};
			yield return Derive<PaddingRightProperty>() with
			{
				Kind = KnownPropertyKind.PaddingRight,
				Width = new Width(edgeSizes.Right, false),
			};
			yield return Derive<PaddingBottomProperty>() with
			{
				Kind = KnownPropertyKind.PaddingBottom,
				Width = new Width(edgeSizes.Bottom, false),
			};
			yield return Derive<PaddingLeftProperty>() with
			{
				Kind = KnownPropertyKind.PaddingLeft,
				Width = new Width(edgeSizes.Left, false),
			};
		}
	}
}
