using System.Runtime.CompilerServices;

namespace Onyx.Css.Computed
{
	public class ComputedRareFieldsStyle
	{
		public ComputedFlexStyle Flex { get; }
		public ComputedPageBreakStyle PageBreak { get; }
		public ComputedOutlineStyle Outline { get; }
		public ComputedSuperRareFieldsStyle SuperRare { get; }

		public int ZIndex { get; }

		public static ComputedRareFieldsStyle Default { get; } =
			new ComputedRareFieldsStyle(ComputedFlexStyle.Default, ComputedPageBreakStyle.Default,
				ComputedOutlineStyle.Default, ComputedSuperRareFieldsStyle.Default, zindex: 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedRareFieldsStyle(ComputedFlexStyle flex, ComputedPageBreakStyle pageBreak,
			ComputedOutlineStyle outline, ComputedSuperRareFieldsStyle superRare, int zindex)
		{
			Flex = flex;
			PageBreak = pageBreak;
			Outline = outline;
			SuperRare = superRare;
			ZIndex = zindex;
		}

		public ComputedRareFieldsStyle WithFlex(ComputedFlexStyle flex)
			=> new ComputedRareFieldsStyle(flex, PageBreak, Outline, SuperRare, ZIndex);
		public ComputedRareFieldsStyle WithPageBreak(ComputedPageBreakStyle pageBreak)
			=> new ComputedRareFieldsStyle(Flex, pageBreak, Outline, SuperRare, ZIndex);
		public ComputedRareFieldsStyle WithOutline(ComputedOutlineStyle outline)
			=> new ComputedRareFieldsStyle(Flex, PageBreak, outline, SuperRare, ZIndex);
		public ComputedRareFieldsStyle WithSuperRare(ComputedSuperRareFieldsStyle superRare)
			=> new ComputedRareFieldsStyle(Flex, PageBreak, Outline, superRare, ZIndex);
		public ComputedRareFieldsStyle WithZIndex(int zindex)
			=> new ComputedRareFieldsStyle(Flex, PageBreak, Outline, SuperRare, zindex);
	}
}
