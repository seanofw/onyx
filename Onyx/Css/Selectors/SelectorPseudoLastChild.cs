using System.Linq.Expressions;
using System.Reflection;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorPseudoLastChild : SelectorFilterPseudoClass
	{
		protected override string PseudoClassName => "last-child";

		public override Specificity Specificity => new Specificity(attributeCount: 1);

		public static SelectorPseudoLastChild Instance { get; } = new SelectorPseudoLastChild();

		private SelectorPseudoLastChild()
			: base(SelectorFilterKind.PseudoLastChild)
		{
		}

		private static readonly PropertyInfo _nextProperty =
			typeof(Element).GetProperty(nameof(Element.NextSibling), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> Expression.Equal(
				Expression.MakeMemberAccess(element, _nextProperty),
				Expression.Constant(null));

		public override bool IsMatch(Element element)
			=> element.NextSibling == null;
	}
}
