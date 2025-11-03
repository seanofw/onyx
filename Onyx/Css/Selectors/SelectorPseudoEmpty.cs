using System.Linq.Expressions;
using System.Reflection;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorPseudoEmpty : SelectorFilterPseudoClass
	{
		protected override string PseudoClassName => "empty";

		public override Specificity Specificity => new Specificity(attributeCount: 1);

		public static SelectorPseudoEmpty Instance { get; } = new SelectorPseudoEmpty();

		private SelectorPseudoEmpty()
			: base(SelectorFilterKind.PseudoEmpty)
		{
		}

		private static readonly PropertyInfo _countProperty =
			typeof(Element).GetProperty(nameof(Element.Count), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> Expression.Equal(
				Expression.MakeMemberAccess(element, _countProperty),
				Expression.Constant(0));

		public override bool IsMatch(Element element)
			=> element.Count == 0;
	}
}
