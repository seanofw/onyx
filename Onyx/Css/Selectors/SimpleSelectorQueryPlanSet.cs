namespace Onyx.Css.Selectors
{
	internal class SimpleSelectorQueryPlanSet
	{
		public SimpleSelectorQueryPlan? Self { get; }
		public SimpleSelectorQueryPlan? Children { get; }
		public SimpleSelectorQueryPlan? Descendants { get; }

		public static SimpleSelectorQueryPlanSet Empty { get; } = new SimpleSelectorQueryPlanSet();

		public SimpleSelectorQueryPlanSet(SimpleSelectorQueryPlan? self = null,
			SimpleSelectorQueryPlan? children = null, SimpleSelectorQueryPlan? descendants = null)
		{
			Self = self;
			Children = children;
			Descendants = descendants;
		}

		public SimpleSelectorQueryPlanSet WithSelf(SimpleSelectorQueryPlan? self)
			=> new SimpleSelectorQueryPlanSet(self, Children, Descendants);

		public SimpleSelectorQueryPlanSet WithChildren(SimpleSelectorQueryPlan? children)
			=> new SimpleSelectorQueryPlanSet(Self, children, Descendants);

		public SimpleSelectorQueryPlanSet WithDescendants(SimpleSelectorQueryPlan? descendants)
			=> new SimpleSelectorQueryPlanSet(Self, Children, descendants);
	}
}
