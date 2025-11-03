using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorFilterClass : SelectorFilter
	{
		public string Class { get; }

		public override Specificity Specificity => new Specificity(attributeCount: 1);

		public SelectorFilterClass(string? @class)
			: base(SelectorFilterKind.Class)
		{
			Class = @class ?? string.Empty;
		}

		private static readonly PropertyInfo _classNamesProperty =
			typeof(Element).GetProperty(nameof(Element.ClassNames), BindingFlags.Instance | BindingFlags.Public)!;
		private static readonly MethodInfo _containsMethod =
			typeof(IReadOnlySet<string>).GetMethod(nameof(IReadOnlySet<string>.Contains), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> Expression.Call(
				Expression.MakeMemberAccess(element, _classNamesProperty),
				_containsMethod,
				Expression.Constant(Class));

		public override bool IsMatch(Element element)
			=> element.ClassNames.Contains(Class);

		public override bool Equals(SelectorFilter? filter)
			=> ReferenceEquals(this, filter) ? true
				: ReferenceEquals(filter, null) ? false
				: filter is SelectorFilterClass other
					&& Kind == other.Kind
					&& Class == other.Class;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + Class.GetHashCode();
				return hashCode;
			}
		}

		public override void ToString(StringBuilder dest)
		{
			dest.Append('.');
			dest.Append(Class);
		}
	}
}
