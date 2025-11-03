using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SimpleSelector : IEquatable<SimpleSelector>
	{
		public string ElementName { get; }
		public IReadOnlyList<SelectorFilter> Filters { get; }

		public Specificity Specificity => _specificity ??= CalculateSpecificity();
		private Specificity? _specificity;

		public long UsageCount => _usageCount;

		private long _usageCount;
		private Func<Element, bool>? _compiledMatchFunc;

		public SimpleSelector(string? elementName = null, IEnumerable<SelectorFilter>? filters = null)
		{
			ElementName = elementName?.ToLowerInvariant() ?? string.Empty;
			Filters = filters?.ToArray() ?? Array.Empty<SelectorFilter>();
		}

		private static readonly PropertyInfo _elementNameProperty =
			typeof(Element).GetProperty(nameof(Element.NodeName), BindingFlags.Instance | BindingFlags.Public)!;

		private static readonly MethodInfo _equalsMethod =
			typeof(string).GetMethod(nameof(string.Equals),
				BindingFlags.Instance | BindingFlags.Public, [typeof(string), typeof(StringComparison)])!;

		public Func<Element, bool> CompileMatchFunc()
		{
			ParameterExpression element = Expression.Parameter(typeof(Element), "element");

			Expression<Func<Element, bool>> matcher = Expression.Lambda<Func<Element, bool>>(
				GetMatchExpression(element),
				element);

			Func<Element, bool> func = matcher.Compile();
			return func;
		}

		public Expression GetMatchExpression(ParameterExpression element)
		{
			Expression? expression = null;

			if (!string.IsNullOrEmpty(ElementName) && ElementName != "*")
			{
				expression = Expression.Call(
					Expression.MakeMemberAccess(element, _elementNameProperty),
					_equalsMethod,
					Expression.Constant(ElementName),
					Expression.Constant(StringComparison.OrdinalIgnoreCase)
				);
			}

			foreach (SelectorFilter filter in Filters)
			{
				Expression nextExpression = filter.GetMatchExpression(element);

				expression = expression != null
					? Expression.AndAlso(expression, nextExpression)
					: nextExpression;
			}

			return expression ?? Expression.Constant(false);
		}

		public bool IsMatch(Element element)
		{
			if (++_usageCount >= 3)
			{
				_compiledMatchFunc ??= CompileMatchFunc();
				return _compiledMatchFunc(element);
			}
			else
			{
				// Test the element name, if there's an element name.
				if (!string.IsNullOrEmpty(ElementName) && ElementName != "*")
				{
					if (!string.Equals(element.NodeName, ElementName, StringComparison.InvariantCultureIgnoreCase))
						return false;
				}

				// Walk the filters, and test each one.
				foreach (SelectorFilter filter in Filters)
				{
					if (!filter.IsMatch(element))
						return false;
				}

				return true;
			}
		}

		private Specificity CalculateSpecificity()
		{
			Specificity specificity = !string.IsNullOrEmpty(ElementName) && ElementName != "*"
				? new Specificity(elementCount: 1)
				: new Specificity(elementCount: 0);

			foreach (SelectorFilter filter in Filters)
				specificity += filter.Specificity;

			return specificity;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Selector other && Equals(other);

		public bool Equals(SimpleSelector? other)
			=> ReferenceEquals(other, null) ? false
				: ReferenceEquals(this, other) ? true
				: ElementName == other.ElementName
					&& Filters.SequenceEqual(other.Filters);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + ElementName.GetHashCode();
				foreach (SelectorFilter filter in Filters)
					hashCode = hashCode * 65599 + filter.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(SimpleSelector? a, SimpleSelector? b)
			=> ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);

		public static bool operator !=(SimpleSelector? a, SimpleSelector? b)
			=> ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !a.Equals(b);

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		public void ToString(StringBuilder stringBuilder)
		{
			if (!string.IsNullOrEmpty(ElementName))
				stringBuilder.Append(ElementName);

			foreach (SelectorFilter filter in Filters)
				filter.ToString(stringBuilder);
		}
	}
}
