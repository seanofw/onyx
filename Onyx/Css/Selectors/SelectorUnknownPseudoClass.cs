using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorUnknownPseudoClass : SelectorFilter
	{
		public bool IsElement { get; }
		public string Name { get; }
		public string? Value { get; }

		public override Specificity Specificity => new Specificity(attributeCount: 1);

		public SelectorUnknownPseudoClass(bool isElement, string name, string? value)
			: base(value == null ? SelectorFilterKind.PseudoUnknown : SelectorFilterKind.PseudoUnknownFunc)
		{
			IsElement = isElement;
			Name = name;
			Value = value;
		}

		private static readonly MethodInfo _hasPseudoElementMethod =
			typeof(Element).GetMethod(nameof(Element.HasPseudoElement), BindingFlags.Instance | BindingFlags.Public)!;
		private static readonly MethodInfo _hasPseudoClassMethod =
			typeof(Element).GetMethod(nameof(Element.HasPseudoClass), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> IsElement
				? Expression.Call(element, _hasPseudoElementMethod, Expression.Constant(Name), Expression.Constant(Value))
				: Expression.Call(element, _hasPseudoClassMethod, Expression.Constant(Name), Expression.Constant(Value));

		public override bool IsMatch(Element element)
			=> IsElement
				? element.HasPseudoElement(Name, Value)
				: element.HasPseudoClass(Name, Value);

		public override bool Equals(SelectorFilter? filter)
			=> ReferenceEquals(this, filter) ? true
				: ReferenceEquals(filter, null) ? false
				: filter is SelectorUnknownPseudoClass other
					&& Kind == other.Kind
					&& IsElement == other.IsElement
					&& Name == other.Name
					&& Value == other.Value;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + (IsElement ? 1 : 0);
				hashCode = hashCode * 65599 + (Name?.GetHashCode() ?? 0);
				hashCode = hashCode * 65599 + (Value?.GetHashCode() ?? 0);
				return hashCode;
			}
		}

		public override void ToString(StringBuilder dest)
		{
			dest.Append(IsElement ? "::" : ":");
			dest.Append(Name);
			if (Value != null)
			{
				dest.Append('(');
				dest.Append(Value);
				dest.Append(')');
			}
		}
	}
}
