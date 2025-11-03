using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorFilterId : SelectorFilter
	{
		public string Id { get; }

		public override Specificity Specificity => new Specificity(idCount: 1);

		public SelectorFilterId(string? id)
			: base(SelectorFilterKind.Id)
		{
			Id = id ?? string.Empty;
		}

		private static readonly PropertyInfo _idProperty =
			typeof(Element).GetProperty(nameof(Element.Id), BindingFlags.Instance | BindingFlags.Public)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> Expression.Equal(
				Expression.MakeMemberAccess(element, _idProperty),
				Expression.Constant(Id));

		public override bool IsMatch(Element element)
			=> element.Id == Id;

		public override bool Equals(SelectorFilter? filter)
			=> ReferenceEquals(this, filter) ? true
				: ReferenceEquals(filter, null) ? false
				: filter is SelectorFilterId other
					&& Kind == other.Kind
					&& Id == other.Id;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + Id.GetHashCode();
				return hashCode;
			}
		}

		public override void ToString(StringBuilder dest)
		{
			dest.Append('#');
			dest.Append(Id);
		}
	}
}
