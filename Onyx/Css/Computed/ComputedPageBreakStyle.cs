using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public readonly struct ComputedPageBreakStyle
	{
		public readonly PageBreakOption BreakBefore;
		public readonly PageBreakOption BreakAfter;
		public readonly PageBreakInsideOption BreakInside;

		public static ComputedPageBreakStyle Default { get; } = new ComputedPageBreakStyle(
			PageBreakOption.Auto, PageBreakOption.Auto, PageBreakInsideOption.Auto);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedPageBreakStyle(PageBreakOption breakBefore, PageBreakOption breakAfter, PageBreakInsideOption breakInside)
		{
			BreakBefore = breakBefore;
			BreakAfter = breakAfter;
			BreakInside = breakInside;
		}

		public ComputedPageBreakStyle WithBreakBefore(PageBreakOption breakBefore)
			=> new ComputedPageBreakStyle(breakBefore, BreakAfter, BreakInside);
		public ComputedPageBreakStyle WithBreakAfter(PageBreakOption breakAfter)
			=> new ComputedPageBreakStyle(BreakBefore, breakAfter, BreakInside);
		public ComputedPageBreakStyle WithBreakInside(PageBreakInsideOption breakInside)
			=> new ComputedPageBreakStyle(BreakBefore, BreakAfter, breakInside);
	}
}
