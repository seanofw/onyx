using System.Globalization;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FlexProperty : StyleProperty
	{
		public double? Grow { get; init; }
		public double? Shrink { get; init; }
		public Measure Measure { get; init; }
		public bool Auto { get; init; }
		public bool Content { get; init; }
		public bool None { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
		{
			if (None)
				return "none";

			List<string> pieces = new List<string>();

			if (Grow.HasValue)
				pieces.Add(Grow.Value.ToString(CultureInfo.InvariantCulture));

			if (Shrink.HasValue)
				pieces.Add(Shrink.Value.ToString(CultureInfo.InvariantCulture));

			if (Content)
				pieces.Add("content");
			else if (Auto)
				pieces.Add("auto");
			else if (Measure.Units != default)
				pieces.Add(Measure.ToString());

			return string.Join(" ", pieces);
		}

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (None)
				yield break;

			if (Grow.HasValue)
				yield return Derive<FlexGrowProperty>() with
				{
					Kind = KnownPropertyKind.FlexGrow,
					Grow = Grow.Value,
				};

			if (Shrink.HasValue)
				yield return Derive<FlexShrinkProperty>() with
				{
					Kind = KnownPropertyKind.FlexShrink,
					Shrink = Shrink.Value,
				};

			if (Content)
				yield return Derive<FlexBasisProperty>() with
				{
					Kind = KnownPropertyKind.FlexBasis,
					Content = true,
				};
			else if (Auto)
				yield return Derive<FlexBasisProperty>() with
				{
					Kind = KnownPropertyKind.FlexBasis,
					Auto = true,
				};
			else if (Measure.Units != default)
				yield return Derive<FlexBasisProperty>() with
				{
					Kind = KnownPropertyKind.FlexBasis,
					Measure = Measure,
				};
		}

		public override bool IsDecomposable => true;
	}
}
