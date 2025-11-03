using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class OverflowProperty : StyleProperty
	{
		public OverflowKind Overflow { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
			=> Overflow.ToString().Hyphenize();

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			yield return Derive<OverflowXProperty>() with
			{
				Kind = KnownPropertyKind.OverflowX,
				OverflowX = Overflow,
			};
			yield return Derive<OverflowYProperty>() with
			{
				Kind = KnownPropertyKind.OverflowY,
				OverflowY = Overflow,
			};
		}

		public override bool IsDecomposable => true;
	}
}
