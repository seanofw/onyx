namespace Onyx.Css.Selectors
{
	internal class SimpleSelectorQueryPlan
	{
		public readonly SimpleSelectorQueryPlanKind Kind;
		public readonly string Name;
		public readonly QueryPlanExecuteFunc ExecuteFunc;

		public int EstimatedCost;

		public SimpleSelectorQueryPlan(SimpleSelectorQueryPlanKind kind, string name, int estimatedCost,
			QueryPlanExecuteFunc executeFunc)
		{
			Kind = kind;
			Name = name;
			EstimatedCost = estimatedCost;
			ExecuteFunc = executeFunc;
		}

		public override string ToString()
			=> $"{SourceDescription}, take {TraversalDescription}, est={EstimatedCost}";

		public string SourceDescription
			=> (Kind & SimpleSelectorQueryPlanKind.SourceMask) switch
			{
				SimpleSelectorQueryPlanKind.ScanAll => $"Scan all",
				SimpleSelectorQueryPlanKind.Id => $"Start at '#{Name}'",
				SimpleSelectorQueryPlanKind.Classname => $"Start at '.{Name}'",
				SimpleSelectorQueryPlanKind.Name => $"Start at '[name=\"{Name}\"]'",
				SimpleSelectorQueryPlanKind.TypeAttribute => $"Start at '[type=\"{Name}\"]'",
				_ => "<unknown start>",
			};

		public string TraversalDescription
			=> (Kind & SimpleSelectorQueryPlanKind.TraversalMask) switch
			{
				SimpleSelectorQueryPlanKind.Self => "self",
				SimpleSelectorQueryPlanKind.Children => "children",
				SimpleSelectorQueryPlanKind.Descendants => "descendants",
				_ => "<unknown traversal",
			};
	}
}
