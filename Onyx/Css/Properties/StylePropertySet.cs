using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Onyx.Extensions;

namespace Onyx.Css.Properties
{
	public class StylePropertySet : IReadOnlyList<StyleProperty>
	{
		public ImmutableArray<StyleProperty> StyleProperties { get; }

		public IReadOnlyDictionary<KnownPropertyKind, StyleProperty> PropertiesByKind
			=> _propertiesByKind ??= StyleProperties
				.Where(p => p.Kind != KnownPropertyKind.Unknown)
				.ToDictionary(p => p.Kind);
		private Dictionary<KnownPropertyKind, StyleProperty>? _propertiesByKind;

		public static StylePropertySet Empty { get; } = new StylePropertySet();

		public int Count => StyleProperties.Length;

		public StyleProperty this[int index] => StyleProperties[index];

		public StylePropertySet(IEnumerable<StyleProperty>? styleProperties = null)
		{
			StyleProperties = styleProperties is ImmutableArray<StyleProperty> list ? list
				: styleProperties is null ? ImmutableArray<StyleProperty>.Empty
				: styleProperties.ToImmutableArray();
		}

		public StylePropertySet Add(StyleProperty styleProperty)
			=> new StylePropertySet(StyleProperties.Add(styleProperty));

		public bool TryGet(KnownPropertyKind kind, [MaybeNullWhen(false)] out StyleProperty styleProperty)
			=> PropertiesByKind.TryGetValue(kind, out styleProperty);

		public StylePropertySet RemoveAt(int index)
			=> new StylePropertySet(StyleProperties.RemoveAt(index));

		public IEnumerator<StyleProperty> GetEnumerator()
			=> ((IReadOnlyList<StyleProperty>)StyleProperties).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
			=> ((IReadOnlyList<StyleProperty>)StyleProperties).GetEnumerator();

		public override string ToString()
			=> string.Join(", ", StyleProperties.Select(p => p.Kind.ToString().Hyphenize()));
	}
}
