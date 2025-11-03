using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;
using System.Collections.Immutable;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class CursorProperty : StyleProperty
	{
		public IReadOnlyList<CustomCursor> CustomCursors
		{
			get => _customCursors;
			init => _customCursors = value is ImmutableArray<CustomCursor> array ? array
				: value is null ? ImmutableArray<CustomCursor>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<CustomCursor> _customCursors = ImmutableArray<CustomCursor>.Empty;

		public CursorKind CursorKind { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithCursor(CursorKind, CustomCursors);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithCursor(source.CursorKind, source.CustomCursors);

		public override string ToString()
			=> CursorKind != default ? CursorKind.ToString().Hyphenize()
				: string.Join(" ", CustomCursors.Select(c => c.ToString()));

		public CursorProperty AddCustomCursor(CustomCursor customCursor)
			=> this with { CustomCursors = _customCursors.Add(customCursor) };
	}
}
