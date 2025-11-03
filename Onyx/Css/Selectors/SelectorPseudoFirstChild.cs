using System.Linq.Expressions;
using System.Reflection;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorPseudoFirstChild : SelectorFilterPseudoClass
	{
		protected override string PseudoClassName => "first-child";

		public override Specificity Specificity => new Specificity(attributeCount: 1);

		public static SelectorPseudoFirstChild Instance { get; } = new SelectorPseudoFirstChild();

		private SelectorPseudoFirstChild()
			: base(SelectorFilterKind.PseudoFirstChild)
		{
		}

		private static readonly PropertyInfo _previousProperty =
			typeof(Element).GetProperty(nameof(Element.PreviousSibling), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> Expression.Equal(
				Expression.MakeMemberAccess(element, _previousProperty),
				Expression.Constant(null));

		public override bool IsMatch(Element element)
			=> element.PreviousSibling == null;
	}
}
