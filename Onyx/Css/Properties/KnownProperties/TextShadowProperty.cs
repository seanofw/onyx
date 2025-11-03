using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class TextShadowProperty : StyleProperty
	{
		public IReadOnlyList<Shadow> Shadows
		{
			get => _shadows;
			init => _shadows = value is ImmutableArray<Shadow> array ? array
				: value is null ? ImmutableArray<Shadow>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<Shadow> _shadows = ImmutableArray<Shadow>.Empty;

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithTextShadows(Shadows);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithTextShadows(source.TextShadows);

		public override string ToString()
			=> string.Join(", ", Shadows.Select(s => s.ToString()));

		public TextShadowProperty AddShadow(Shadow shadow)
			=> this with { Shadows = _shadows.Add(shadow) };
	}
}
