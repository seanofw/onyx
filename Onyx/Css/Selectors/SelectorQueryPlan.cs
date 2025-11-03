namespace Onyx.Css.Selectors
{
	internal class SelectorQueryPlan
	{
		public readonly SimpleSelectorQueryPlan InnerQueryPlan;

		public int ActualCost;

		public SelectorQueryPlan(SimpleSelectorQueryPlan innerQueryPlan)
		{
			InnerQueryPlan = innerQueryPlan;
			ActualCost = -1;
		}

		public override string ToString()
			=> InnerQueryPlan.ToString() + $", actual={ActualCost}";
	}
}
