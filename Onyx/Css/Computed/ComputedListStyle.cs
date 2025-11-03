using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedListStyle : IDefaultInheritedStyle
	{
		public string? Uri { get; }
		public ListStylePosition Position { get; }
		public ListStyleType Type { get; }

		public static ComputedListStyle Default { get; } = new ComputedListStyle(
			null, ListStylePosition.Outside, ListStyleType.Disc);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedListStyle(string? uri, ListStylePosition position, ListStyleType type)
		{
			Uri = uri;
			Position = position;
			Type = type;
		}

		public ComputedListStyle WithUri(string? uri)
			=> new ComputedListStyle(uri, Position, Type);
		public ComputedListStyle WithPosition(ListStylePosition position)
			=> new ComputedListStyle(Uri, position, Type);
		public ComputedListStyle WithType(ListStyleType type)
			=> new ComputedListStyle(Uri, Position, type);
	}
}
