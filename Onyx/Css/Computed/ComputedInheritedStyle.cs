using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	/// <summary>
	/// CSS properties that are inherited by default, which is divided into roughly
	/// five smaller bags of properties:
	/// * Text styles
	/// * Font styles
	/// * Table styles
	/// * List styles
	/// * Miscellany
	/// * Visibility (separated out because it changes more frequently than
	///   many of the others and is more important for rendering too).
	/// </summary>
	public class ComputedInheritedStyle : IDefaultInheritedStyle
	{
		public ComputedTextStyle Text { get; }
		public ComputedFontStyle Font { get; }
		public ComputedTableStyle Table { get; }
		public ComputedListStyle List { get; }
		public ComputedMiscInheritedStyle Misc { get; }
		public VisibilityKind Visibility { get; }

		public static ComputedInheritedStyle Default { get; }
			= new ComputedInheritedStyle(ComputedTextStyle.Default, ComputedFontStyle.Default,
				ComputedTableStyle.Default, ComputedListStyle.Default,
				ComputedMiscInheritedStyle.Default, VisibilityKind.Visible);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedInheritedStyle(ComputedTextStyle text, ComputedFontStyle font,
			ComputedTableStyle table, ComputedListStyle list,
			ComputedMiscInheritedStyle misc, VisibilityKind visibility)
		{
			Text = text;
			Font = font;
			Table = table;
			List = list;
			Misc = misc;
			Visibility = visibility;
		}

		public ComputedInheritedStyle WithText(ComputedTextStyle text)
			=> new ComputedInheritedStyle(text, Font, Table, List, Misc, Visibility);
		public ComputedInheritedStyle WithFont(ComputedFontStyle font)
			=> new ComputedInheritedStyle(Text, font, Table, List, Misc, Visibility);
		public ComputedInheritedStyle WithTable(ComputedTableStyle table)
			=> new ComputedInheritedStyle(Text, Font, table, List, Misc, Visibility);
		public ComputedInheritedStyle WithList(ComputedListStyle list)
			=> new ComputedInheritedStyle(Text, Font, Table, list, Misc, Visibility);
		public ComputedInheritedStyle WithMisc(ComputedMiscInheritedStyle misc)
			=> new ComputedInheritedStyle(Text, Font, Table, List, misc, Visibility);
		public ComputedInheritedStyle WithVisibility(VisibilityKind visibility)
			=> new ComputedInheritedStyle(Text, Font, Table, List, Misc, visibility);
	}
}
