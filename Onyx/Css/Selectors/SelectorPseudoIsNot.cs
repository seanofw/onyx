using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorPseudoIsNot : SelectorFilter
	{
		public CompoundSelector? Child { get; }
		public bool Not { get; }

		public override Specificity Specificity => _specificity ??= CalculateSpecificity();
		private Specificity? _specificity;

		private Specificity CalculateSpecificity()
			=> Child != null
				? new Specificity(attributeCount: 1) + Child.Specificity
				: new Specificity(attributeCount: 1);

		public SelectorPseudoIsNot(bool not, CompoundSelector? child)
			: base(not ? SelectorFilterKind.PseudoIs : SelectorFilterKind.PseudoNot)
		{
			not = Not;
			Child = child;
		}

		private static readonly MethodInfo _isMatchMethod =
			typeof(CompoundSelector).GetMethod(nameof(CompoundSelector.IsMatch), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
		{
			Expression expression;
			if (Child == null)
				expression = Expression.Constant(false);
			else
				expression = Expression.Call(
					Expression.Constant(Child, typeof(CompoundSelector)),
					_isMatchMethod,
					element);

			if (Not)
				expression = Expression.Not(expression);

			return expression;
		}

		public override bool IsMatch(Element element)
			=> (Child?.IsMatch(element) ?? false) ^ Not;

		public override bool Equals(SelectorFilter? filter)
			=> ReferenceEquals(this, filter) ? true
				: ReferenceEquals(filter, null) ? false
				: filter is SelectorPseudoIsNot other
					&& Kind == other.Kind
					&& Not == other.Not
					&& Child == other.Child;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + (Not ? 1 : 0);
				hashCode = hashCode * 65599 + (Child?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		public override void ToString(StringBuilder dest)
		{
			dest.Append(Not ? ":not(" : ":is(");
			Child?.ToString(dest);
			dest.Append(")");
		}
	}
}
