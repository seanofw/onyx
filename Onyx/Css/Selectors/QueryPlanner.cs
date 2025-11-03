using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	internal delegate (IReadOnlyCollection<Node> Result, int CostReestimate) QueryPlanExecuteFunc(Node trueRoot, ElementLookupTables tables);

	internal static class QueryPlanner
	{
		public static IReadOnlyCollection<Node> ExecuteQuery(Selector selector,
			Node trueRoot, ElementLookupTables tables)
		{
			// If a cost shrinks below 2/3, or grows above 3/2, we assume that the
			// a prior query plan is no longer valid and recalculate.
			const int DiffDenom = 2;
			const int DiffNumer = 3;

			SelectorQueryPlan queryPlan = GetOrCreateQueryPlan(selector, trueRoot, tables);
			SimpleSelectorQueryPlan innerQueryPlan = queryPlan.InnerQueryPlan;

			(IReadOnlyCollection<Node> result, int costReestimate) = innerQueryPlan.ExecuteFunc(trueRoot, tables);

			int lastActualCost = queryPlan.ActualCost;
			if (lastActualCost < 0)
			{
				// No metric yet, so record how many elements we found this way.
				queryPlan.ActualCost = result.Count;
				lastActualCost = result.Count;
			}

			int lastEstimatedCost = innerQueryPlan.EstimatedCost;
			if (costReestimate * DiffNumer < lastEstimatedCost * DiffDenom
				|| costReestimate * DiffDenom > lastEstimatedCost * DiffNumer)
			{
				// Estimated cost is very different, so what was used to estimate the overall
				// query plan is no longer good because the document changed.  Reset the query
				// plan so it will get recomputed and hopefully run more efficiently next time.
				innerQueryPlan.EstimatedCost = costReestimate;
				ResetQueryPlan(selector, trueRoot, tables);
			}

			if (result.Count * DiffNumer < lastActualCost * DiffDenom
				|| result.Count * DiffDenom > lastActualCost * DiffNumer)
			{
				// Inner query plan might be valid independently, but the outer plan may be
				// built around the wrong simple selector after the document changed.  Reset
				// the outer plan, but let the metrics from the inner plan stay as-is.
				ResetQueryPlan(selector, trueRoot, tables);
			}

			return result;
		}

		public static SelectorQueryPlan GetOrCreateQueryPlan(Selector selector,
			Node trueRoot, ElementLookupTables tables)
		{
			if (!tables.SelectorQueryPlans.TryGetValue(selector, out SelectorQueryPlan? queryPlan))
				tables.SelectorQueryPlans[selector] = queryPlan = MakeQueryPlan(selector, trueRoot, tables);

			return queryPlan;
		}

		public static void ResetQueryPlan(Selector selector,
			Node trueRoot, ElementLookupTables tables)
		{
			tables.SelectorQueryPlans.Remove(selector);
		}

		private static SelectorQueryPlan MakeQueryPlan(Selector selector,
			Node trueRoot, ElementLookupTables tables)
		{
			SimpleSelectorQueryPlan? bestQueryPlan = null;

			// Walk the selector right-to-left, and look up the SimpleSelectorQueryPlan
			// for each simple selector.
			IReadOnlyList<SelectorComponent> path = selector.Path;
			Combinator lastCombinator = Combinator.Self;
			for (int i = path.Count - 1; i >= 0; i--)
			{
				// Look up or create the simple selector's plan set.
				SimpleSelector simpleSelector = path[i].SimpleSelector;
				if (!tables.SimpleSelectorQueryPlans.TryGetValue(simpleSelector, out SimpleSelectorQueryPlanSet? planSet))
				{
					planSet = MakeSimpleQueryPlan(simpleSelector, trueRoot, tables);
					tables.SimpleSelectorQueryPlans[simpleSelector] = planSet;
				}

				// Get the actual query plan for the simple selector in its current position.
				// We do not currently support query plans for simple selectors combined by + or ~
				// as those operators are much harder to efficiently support.
				SimpleSelectorQueryPlan? simpleQueryPlan = null;
				if (lastCombinator == Combinator.Self)
					simpleQueryPlan = planSet.Self;
				else if (lastCombinator == Combinator.Child)
					simpleQueryPlan = planSet.Children;
				else if (lastCombinator == Combinator.Descendant)
					simpleQueryPlan = planSet.Descendants;

				// If the estimated cost for this query plan beats the current estimated
				// cost for the best query plan we've found so far, this is the winning
				// query plan, so take it.
				if (simpleQueryPlan != null)
					if (bestQueryPlan == null || simpleQueryPlan.EstimatedCost < bestQueryPlan.EstimatedCost)
						bestQueryPlan = simpleQueryPlan;

				lastCombinator = path[i].Combinator;
			}

			return new SelectorQueryPlan(bestQueryPlan ?? MakeScanAllPlan(trueRoot.DescendantElementCount));
		}

		private static SimpleSelectorQueryPlan MakeScanAllPlan(int cost)
			=> new SimpleSelectorQueryPlan(SimpleSelectorQueryPlanKind.ScanAll, string.Empty, cost,
				(trueRoot, tables) => (trueRoot.GetDescendants(), trueRoot.DescendantElementCount));

		private static SimpleSelectorQueryPlan MakeGetElementsByElementTypePlan(SimpleSelector simpleSelector, string elementType, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.ElementType | SimpleSelectorQueryPlanKind.Self,
				elementType, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByElementType(elementType);
					return (baseSet, baseSet.Count);
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByElementTypeChildrenPlan(SimpleSelector simpleSelector, string elementType, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.ElementType | SimpleSelectorQueryPlanKind.Children,
				elementType, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByElementType(elementType);
					return (ChildrenOf(simpleSelector, baseSet), ChildrenCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByElementTypeDescendantsPlan(SimpleSelector simpleSelector, string elementType, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.ElementType | SimpleSelectorQueryPlanKind.Descendants,
				elementType, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByElementType(elementType);
					return (DescendantsOf(simpleSelector, baseSet), DescendantCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByIdPlan(SimpleSelector simpleSelector, string id, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Id | SimpleSelectorQueryPlanKind.Self,
				id, estimatedCost,
				(trueRoot, tables) => {
					var baseSet = tables.GetElementsById(id);
					return (baseSet, baseSet.Count);
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByIdChildrenPlan(SimpleSelector simpleSelector, string id, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Id | SimpleSelectorQueryPlanKind.Children,
				id, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsById(id);
					return (ChildrenOf(simpleSelector, baseSet), ChildrenCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByIdDescendantsPlan(SimpleSelector simpleSelector, string id, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Id | SimpleSelectorQueryPlanKind.Descendants,
				id, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsById(id);
					return (DescendantsOf(simpleSelector, baseSet), DescendantCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByClassnamePlan(SimpleSelector simpleSelector, string classname, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Classname | SimpleSelectorQueryPlanKind.Self,
				classname, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByClassname(classname);
					return (baseSet, baseSet.Count);
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByClassnameChildrenPlan(SimpleSelector simpleSelector, string classname, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Classname | SimpleSelectorQueryPlanKind.Children,
				classname, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByClassname(classname);
					return (ChildrenOf(simpleSelector, baseSet), ChildrenCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByClassnameDescendantsPlan(SimpleSelector simpleSelector, string classname, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Classname | SimpleSelectorQueryPlanKind.Descendants,
				classname, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByClassname(classname);
					return (DescendantsOf(simpleSelector, baseSet), DescendantCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByNamePlan(SimpleSelector simpleSelector, string name, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Name | SimpleSelectorQueryPlanKind.Self,
				name, estimatedCost,
				(trueRoot, tables) => {
					var baseSet = tables.GetElementsByName(name);
					return (baseSet, baseSet.Count);
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByNameChildrenPlan(SimpleSelector simpleSelector, string name, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Name | SimpleSelectorQueryPlanKind.Children,
				name, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByName(name);
					return (ChildrenOf(simpleSelector, baseSet), ChildrenCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByNameDescendantsPlan(SimpleSelector simpleSelector, string name, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.Name | SimpleSelectorQueryPlanKind.Descendants,
				name, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByName(name);
					return (DescendantsOf(simpleSelector, baseSet), DescendantCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByTypeAttributePlan(SimpleSelector simpleSelector, string type, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.TypeAttribute | SimpleSelectorQueryPlanKind.Self,
				type, estimatedCost,
				(trueRoot, tables) => {
					var baseSet = tables.GetElementsByTypeAttribute(type);
					return (baseSet, baseSet.Count);
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByTypeAttributeChildrenPlan(SimpleSelector simpleSelector, string type, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.TypeAttribute | SimpleSelectorQueryPlanKind.Children,
				type, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByTypeAttribute(type);
					return (ChildrenOf(simpleSelector, baseSet), ChildrenCost(baseSet));
				});

		private static SimpleSelectorQueryPlan MakeGetElementsByTypeAttributeDescendantsPlan(SimpleSelector simpleSelector, string type, int estimatedCost)
			=> new SimpleSelectorQueryPlan(
				SimpleSelectorQueryPlanKind.TypeAttribute | SimpleSelectorQueryPlanKind.Descendants,
				type, estimatedCost,
				(trueRoot, tables) => {
					IReadOnlyCollection<Node> baseSet = tables.GetElementsByTypeAttribute(type);
					return (DescendantsOf(simpleSelector, baseSet), DescendantCost(baseSet));
				});

		private static int ChildrenCost(IReadOnlyCollection<Node> currentSet)
		{
			int cost = 0;
			foreach (Node node in currentSet)
				cost += node.ChildElementCount;
			return cost;
		}

		private static IReadOnlyCollection<Node> ChildrenOf(SimpleSelector simpleSelector,
			IReadOnlyCollection<Node> baseSet)
		{
			List<Node> result = new List<Node>();
			foreach (Node node in baseSet)
			{
				if (node is Element element && simpleSelector.IsMatch(element))
				{
					foreach (Node child in node.ChildNodes)
					{
						if (child is Element)
							result.Add(node);
					}
				}
			}
			return result;
		}

		private static int DescendantCost(IReadOnlyCollection<Node> currentSet)
		{
			int cost = 0;
			foreach (Node node in currentSet)
				cost += node.DescendantElementCount;
			return cost;
		}

		private static IReadOnlyCollection<Node> DescendantsOf(SimpleSelector simpleSelector,
			IReadOnlyCollection<Node> baseSet)
		{
			List<Element> result = new List<Element>();
			foreach (Node node in baseSet)
			{
				if (node is Element element && simpleSelector.IsMatch(element))
					node.GetDescendants(result);
			}
			return result;
		}

		/// <summary>
		/// Make a complete query plan for a single simple selector, taking advantage
		/// of the current counts of elements and which things have which attributes.
		/// This emits one of sixteen different optimized techniques to retrieve elements
		/// matching that selector, and does so for that selector in three different
		/// usage positions:  As a leaf (self mode), when followed by a '>' (children mode),
		/// and when followed by space (descendant mode).  The resulting query plan is
		/// then returned.
		/// </summary>
		/// <param name="simpleSelector">The simple selector to generate a query plan for.</param>
		/// <param name="trueRoot">The true root of the DOM.</param>
		/// <param name="tables">The fast-lookup tables that provide efficient access to
		/// the DOM.</param>
		/// <returns>The resulting query-plan set for this simple selector.</returns>
		public static SimpleSelectorQueryPlanSet MakeSimpleQueryPlan(
			SimpleSelector simpleSelector, Node trueRoot, ElementLookupTables? tables = null)
		{
			SimpleSelectorQueryPlanSet? planSet =
				tables != null ? GenerateFastQueryPlan(simpleSelector, tables) : null;

			planSet ??= SimpleSelectorQueryPlanSet.Empty;

			if (planSet.Self == null)
				planSet = planSet.WithSelf(MakeScanAllPlan(trueRoot.DescendantElementCount));

			if (planSet.Children == null)
				planSet = planSet.WithChildren(MakeScanAllPlan(trueRoot.DescendantElementCount));

			if (planSet.Descendants == null)
				planSet = planSet.WithDescendants(MakeScanAllPlan(trueRoot.DescendantElementCount));

			return planSet;
		}

		/// <summary>
		/// Use the fast-lookup container to attempt to find the source of the
		/// best candidate set that can fast-match the given simple selector.  It's
		/// possible that none of them fast-match at all.  If found, generate a query
		/// plan for the simple selector that attempts to match it as efficiently as
		/// possible.
		/// </summary>
		/// <param name="tables">The fast-lookup tables that have
		/// direct mappings for element types, IDs, classes, names, and other
		/// common attributes.</param>
		/// <param name="simpleSelector">The simple selector to try.</param>
		private static SimpleSelectorQueryPlanSet? GenerateFastQueryPlan(
			SimpleSelector simpleSelector, ElementLookupTables tables)
		{
			SimpleSelectorQueryPlanSet? planSet = null;

			// First possibility: If this selector contains an element type, look up how many
			// of those there are.
			string elementName = simpleSelector.ElementName;
			if (!string.IsNullOrEmpty(elementName) && elementName != "*")
			{
				IReadOnlyCollection<Node> elementTypeSet = tables.GetElementsByElementType(elementName);
				planSet = UpdatePlanSet(planSet, elementTypeSet, elementName, simpleSelector,
					MakeGetElementsByElementTypePlan,
					MakeGetElementsByElementTypeChildrenPlan,
					MakeGetElementsByElementTypeDescendantsPlan);
			}

			foreach (SelectorFilter filter in simpleSelector.Filters)
			{
				// Second possibility: If this selector contains classnames,
				// those might be smaller sets than what we just found.
				if (filter is SelectorFilterClass byClass)
				{
					IReadOnlyCollection<Node> classnameSet = tables.GetElementsByClassname(byClass.Class);
					planSet = UpdatePlanSet(planSet, classnameSet, byClass.Class, simpleSelector,
						MakeGetElementsByClassnamePlan,
						MakeGetElementsByClassnameChildrenPlan,
						MakeGetElementsByClassnameDescendantsPlan);
				}

				// Third possibility: If this selector contains an #id, that's
				// highly likely to be a set of zero or one element.
				else if (filter is SelectorFilterId byId)
				{
					IReadOnlyCollection<Node> idSet = tables.GetElementsById(byId.Id);
					planSet = UpdatePlanSet(planSet, idSet, byId.Id, simpleSelector,
						MakeGetElementsByIdPlan,
						MakeGetElementsByIdChildrenPlan,
						MakeGetElementsByIdDescendantsPlan);
				}

				// Fourth possibility: If this selector contains [type=...] or [name=...], that's
				// likely smaller than all elements too.
				else if (filter is SelectorFilterAttrib byAttrib && byAttrib.Kind == SelectorFilterKind.AttribEq)
				{
					if (byAttrib.Name == "name")
					{
						IReadOnlyCollection<Node> attrSet = tables.GetElementsByName(byAttrib.Value);
						planSet = UpdatePlanSet(planSet, attrSet, byAttrib.Value, simpleSelector,
							MakeGetElementsByNamePlan,
							MakeGetElementsByNameChildrenPlan,
							MakeGetElementsByNameDescendantsPlan);
					}
					else if (byAttrib.Name == "type")
					{
						IReadOnlyCollection<Node> attrSet = tables.GetElementsByTypeAttribute(byAttrib.Value);
						planSet = UpdatePlanSet(planSet, attrSet, byAttrib.Value, simpleSelector,
							MakeGetElementsByTypeAttributePlan,
							MakeGetElementsByTypeAttributeChildrenPlan,
							MakeGetElementsByTypeAttributeDescendantsPlan);
					}
				}
			}

			return planSet;
		}

		private static SimpleSelectorQueryPlanSet? UpdatePlanSet(SimpleSelectorQueryPlanSet? planSet,
			IReadOnlyCollection<Node> currentSet, string arg, SimpleSelector simpleSelector,
			Func<SimpleSelector, string, int, SimpleSelectorQueryPlan> makeSelfPlan,
			Func<SimpleSelector, string, int, SimpleSelectorQueryPlan> makeChildrenPlan,
			Func<SimpleSelector, string, int, SimpleSelectorQueryPlan> makeDescendantsPlan)
		{
			planSet ??= SimpleSelectorQueryPlanSet.Empty;

			int estimatedSelfCost = currentSet.Count;
			if (planSet.Self == null || estimatedSelfCost < planSet.Self.EstimatedCost)
				planSet = planSet.WithSelf(makeSelfPlan(simpleSelector, arg, estimatedSelfCost));

			int estimatedChildrenCost = ChildrenCost(currentSet);
			if (planSet.Children == null || estimatedChildrenCost < planSet.Children.EstimatedCost)
				planSet = planSet.WithChildren(makeChildrenPlan(simpleSelector, arg,
					estimatedChildrenCost + estimatedSelfCost));

			int estimatedDescendantCost = DescendantCost(currentSet);
			if (planSet.Descendants == null || estimatedDescendantCost < planSet.Descendants.EstimatedCost)
				planSet = planSet.WithDescendants(makeDescendantsPlan(simpleSelector, arg,
					estimatedDescendantCost + estimatedSelfCost));

			return planSet;
		}
	}
}
