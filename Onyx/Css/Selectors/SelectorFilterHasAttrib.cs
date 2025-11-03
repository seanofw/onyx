using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using Onyx.Extensions;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorFilterHasAttrib : SelectorFilter
	{
		public string Name { get; }

		public override Specificity Specificity => new Specificity(attributeCount: 1);

		public SelectorFilterHasAttrib(string? name = null)
			: base(SelectorFilterKind.HasAttrib)
		{
			Name = name?.FastLowercase() ?? string.Empty;
		}

		private static readonly PropertyInfo _attributesProperty =
			typeof(Element).GetProperty(nameof(Element.Attributes), BindingFlags.Instance | BindingFlags.Public)!;
		private static readonly MethodInfo _containsKeyMethod =
			typeof(NamedNodeMap).GetMethod(nameof(NamedNodeMap.ContainsKey), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> Expression.Call(
				Expression.MakeMemberAccess(element, _attributesProperty),
				_containsKeyMethod,
				Expression.Constant(Name));

		public override bool IsMatch(Element element)
			=> element.Attributes.ContainsKey(Name);

		public override bool Equals(SelectorFilter? filter)
			=> ReferenceEquals(this, filter) ? true
				: ReferenceEquals(filter, null) ? false
				: filter is SelectorFilterHasAttrib other
					&& Kind == other.Kind
					&& Name == other.Name;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + Name.GetHashCode();
				return hashCode;
			}
		}

		public override void ToString(StringBuilder dest)
		{
			dest.Append('[');
			dest.Append(Name);
			dest.Append(']');
		}
	}
}
