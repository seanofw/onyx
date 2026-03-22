
using Onyx.Css.Properties;
using Onyx.Css.Types;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Onyx.Css.Computed
{
	/// <summary>
	/// Oddities that are inherited, so they can't just go on the Misc object, but
	/// that don't really have anything in common with anything.
	/// </summary>
	public class ComputedMiscInheritedStyle
	{
		public IReadOnlyList<CustomCursor> CustomCursors => _customCursors;
		private readonly ImmutableArray<CustomCursor> _customCursors;
		public CursorKind CursorKind { get; }

		public int Widows { get; }
		public int Orphans { get; }

		public IReadOnlyList<string> Quotes => _quotes;
		private readonly ImmutableArray<string> _quotes;

		public static ComputedMiscInheritedStyle Default { get; }
			= new ComputedMiscInheritedStyle(cursorKind: CursorKind.Default, customCursors: null,
				widows: 2, orphans: 2, quotes: null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedMiscInheritedStyle(CursorKind cursorKind, IEnumerable<CustomCursor>? customCursors,
			int widows, int orphans, IEnumerable<string>? quotes)
		{
			_customCursors = customCursors is ImmutableArray<CustomCursor> array ? array
				: customCursors is null ? ImmutableArray<CustomCursor>.Empty
				: customCursors.ToImmutableArray();

			_quotes = quotes is ImmutableArray<string> array3 ? array3
				: quotes is null ? ImmutableArray<string>.Empty
				: quotes.ToImmutableArray();
		}

		public ComputedMiscInheritedStyle WithCursor(CursorKind cursorKind, IEnumerable<CustomCursor>? customCursors = null)
			=> new ComputedMiscInheritedStyle(cursorKind, customCursors, Widows, Orphans, Quotes);
		public ComputedMiscInheritedStyle WithWidows(int widows)
			=> new ComputedMiscInheritedStyle(CursorKind, CustomCursors, widows, Orphans, Quotes);
		public ComputedMiscInheritedStyle WithOrphans(int orphans)
			=> new ComputedMiscInheritedStyle(CursorKind, CustomCursors, Widows, orphans, Quotes);
		public ComputedMiscInheritedStyle WithQuotes(IEnumerable<string>? quotes)
			=> new ComputedMiscInheritedStyle(CursorKind, CustomCursors, Widows, Orphans, quotes);

		public UInt256 Diff(ComputedMiscInheritedStyle other)
		{
			if (this == other)
				return default;

			UInt256 bits = default;

			if (CursorKind != other.CursorKind)
				bits = bits.SetBit((int)KnownPropertyKind.Cursor);

			if (CustomCursors.Count != other.CustomCursors.Count)
				bits = bits.SetBit((int)KnownPropertyKind.Cursor);
			else
			{
				for (int i = 0; i < CustomCursors.Count; i++)
					if (CustomCursors[i] != other.CustomCursors[i])
						bits = bits.SetBit((int)KnownPropertyKind.Cursor);
			}

			if (Widows != other.Widows)
				bits = bits.SetBit((int)KnownPropertyKind.Widows);
			if (Orphans != other.Orphans)
				bits = bits.SetBit((int)KnownPropertyKind.Orphans);

			if (Quotes.Count != other.Quotes.Count)
				bits = bits.SetBit((int)KnownPropertyKind.Quotes);
			else
			{
				for (int i = 0; i < Quotes.Count; i++)
					if (Quotes[i] != other.Quotes[i])
						bits = bits.SetBit((int)KnownPropertyKind.Quotes);
			}

			return bits;
		}
	}
}
