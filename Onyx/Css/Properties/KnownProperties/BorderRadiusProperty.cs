using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BorderRadiusProperty : StyleProperty
	{
		public IReadOnlyList<Measure> Radii
		{
			get => _radii;
			init => _radii = value is ImmutableArray<Measure> array ? array
				: value is null ? ImmutableArray<Measure>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<Measure> _radii = ImmutableArray<Measure>.Empty;

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
			=> string.Join(" ", Radii.Select(w => w.ToString()));

		public BorderRadiusProperty AddRadius(Measure radius)
			=> this with { Radii = _radii.Add(radius) };

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			Measure topLeft, topRight, bottomLeft, bottomRight;

			switch (Radii.Count)
			{
				case 0:
					yield break;
				case 1:
					topLeft = topRight = bottomLeft = bottomRight = Radii[0];
					break;
				case 2:
					topLeft = bottomRight = Radii[0];
					topRight = bottomLeft = Radii[1];
					break;
				case 3:
					topLeft = Radii[0];
					topRight = bottomLeft = Radii[1];
					bottomRight = Radii[2];
					break;
				case 4:
				default:
					topLeft = Radii[0];
					topRight = Radii[1];
					bottomRight = Radii[2];
					bottomLeft = Radii[3];
					break;
			}

			yield return Derive<BorderTopLeftRadiusProperty>() with
			{
				Kind = KnownPropertyKind.BorderTopLeftRadius,
				Radius = topLeft,
			};
			yield return Derive<BorderTopRightRadiusProperty>() with
			{
				Kind = KnownPropertyKind.BorderTopRightRadius,
				Radius = topRight,
			};
			yield return Derive<BorderBottomRightRadiusProperty>() with
			{
				Kind = KnownPropertyKind.BorderBottomRightRadius,
				Radius = bottomRight,
			};
			yield return Derive<BorderBottomLeftRadiusProperty>() with
			{
				Kind = KnownPropertyKind.BorderBottomLeftRadius,
				Radius = bottomLeft,
			};
		}

		public override bool IsDecomposable => true;
	}
}
