using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FlexFlowProperty : StyleProperty
	{
		public FlexDirection Direction { get; init; }
		public FlexWrap Wrap { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (Direction != default)
				pieces.Add(Direction.ToString().Hyphenize());
			if (Wrap != default)
				pieces.Add(Wrap.ToString().Hyphenize());

			return string.Join(" ", pieces);
		}

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			yield return Derive<FlexDirectionProperty>() with
			{
				Kind = KnownPropertyKind.FlexDirection,
				Direction = Direction
			};
			yield return Derive<FlexWrapProperty>() with
			{
				Kind = KnownPropertyKind.FlexWrap,
				Wrap = Wrap
			};
		}

		public override bool IsDecomposable => true;
	}
}
