using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class FontFamilyProperty : StyleProperty
	{
		public IReadOnlyList<FontFamily> Families
		{
			get => _families;
			init => _families = value is ImmutableArray<FontFamily> array ? array
				: value is null ? ImmutableArray<FontFamily>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<FontFamily> _families = ImmutableArray<FontFamily>.Empty;

		public static FontFamilyProperty Default { get; } = new FontFamilyProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithFontFamilies(Families);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithFontFamilies(source.FontFamilies);

		public override string ToString()
			=> string.Join(", ", Families.Select(n => n.ToString()));

		public FontFamilyProperty AddName(string name)
			=> this with { Families = _families.Add(name) };

		public FontFamilyProperty AddGenericFontFamily(GenericFontFamily kind)
			=> this with { Families = _families.Add(kind) };
	}
}