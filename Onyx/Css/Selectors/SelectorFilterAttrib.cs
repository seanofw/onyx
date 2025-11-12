using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Onyx.Extensions;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorFilterAttrib : SelectorFilter
	{
		public string Name { get; }
		public string Value { get; }

		public override Specificity Specificity => new Specificity(attributeCount: 1);

		public SelectorFilterAttrib(SelectorFilterKind kind, string? name, string? value)
			: base(kind)
		{
			Name = name?.FastLowercase() ?? string.Empty;
			Value = value ?? string.Empty;
		}

		private static readonly PropertyInfo _attributesProperty =
			typeof(Element).GetProperty(nameof(Element.Attributes), BindingFlags.Instance | BindingFlags.Public)!;
		private static readonly MethodInfo _tryGetValueMethod =
			typeof(NamedNodeMap).GetMethod(nameof(NamedNodeMap.TryGetValue), BindingFlags.Instance | BindingFlags.Public)!;

		private static readonly MethodInfo _equalsMethod =
			typeof(string).GetMethod(nameof(string.Equals),
				BindingFlags.Instance | BindingFlags.Public, [typeof(string), typeof(StringComparison)])!;

		private static readonly MethodInfo _startsWithMethod =
			typeof(string).GetMethod(nameof(string.StartsWith),
				BindingFlags.Instance | BindingFlags.Public, [typeof(string), typeof(StringComparison)])!;
		private static readonly MethodInfo _endsWithMethod =
			typeof(string).GetMethod(nameof(string.EndsWith),
				BindingFlags.Instance | BindingFlags.Public, [typeof(string), typeof(StringComparison)])!;
		private static readonly MethodInfo _containsMethod =
			typeof(string).GetMethod(nameof(string.Contains),
				BindingFlags.Instance | BindingFlags.Public, [typeof(string), typeof(StringComparison)])!;

		private static readonly MethodInfo _stringIncludesMethod =
			typeof(SelectorFilterAttrib).GetMethod(nameof(StringIncludes),
				BindingFlags.Static | BindingFlags.NonPublic, [typeof(string), typeof(string), typeof(StringComparison)])!;
		private static readonly MethodInfo _stringDashMatchesMethod =
			typeof(SelectorFilterAttrib).GetMethod(nameof(StringDashMatches),
				BindingFlags.Static | BindingFlags.NonPublic, [typeof(string), typeof(string), typeof(StringComparison)])!;

		public override Expression GetMatchExpression(ParameterExpression element)
		{
			ParameterExpression valueVariable = Expression.Parameter(typeof(string), "value");

			Expression equalExpression = Kind switch
			{
				SelectorFilterKind.AttribEq or SelectorFilterKind.CSAttribEq
					=> Expression.Equal(valueVariable, Expression.Constant(Value)),

				SelectorFilterKind.AttribContains or SelectorFilterKind.CSAttribContains
					=> Expression.Call(valueVariable,
						_containsMethod,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.Ordinal)),

				SelectorFilterKind.AttribStartsWith or SelectorFilterKind.CSAttribStartsWith
					=> Expression.Call(valueVariable,
						_startsWithMethod,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.Ordinal)),

				SelectorFilterKind.AttribEndsWith or SelectorFilterKind.CSAttribEndsWith
					=> Expression.Call(valueVariable,
						_endsWithMethod,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.Ordinal)),

				SelectorFilterKind.AttribIncludes or SelectorFilterKind.CSAttribIncludes
					=> Expression.Call(_stringIncludesMethod,
						valueVariable,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.Ordinal)),

				SelectorFilterKind.AttribDashMatch or SelectorFilterKind.CSAttribDashMatch
					=> Expression.Call(_stringDashMatchesMethod,
						valueVariable,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.Ordinal)),

				SelectorFilterKind.CIAttribEq
					=> Expression.Call(valueVariable,
						_equalsMethod,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)),

				SelectorFilterKind.CIAttribContains
					=> Expression.Call(valueVariable,
						_containsMethod,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)),

				SelectorFilterKind.CIAttribStartsWith
					=> Expression.Call(valueVariable,
						_startsWithMethod,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)),

				SelectorFilterKind.CIAttribEndsWith
					=> Expression.Call(valueVariable,
						_endsWithMethod,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)),

				SelectorFilterKind.CIAttribIncludes
					=> Expression.Call(_stringIncludesMethod,
						valueVariable,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)),

				SelectorFilterKind.CIAttribDashMatch
					=> Expression.Call(_stringDashMatchesMethod,
						valueVariable,
						Expression.Constant(Value),
						Expression.Constant(StringComparison.OrdinalIgnoreCase)),

				_ => Expression.Constant(false),
			};

			return Expression.Block(typeof(bool),
				[valueVariable],
				[
					Expression.Condition(						// if
						Expression.Not(							//    (!
							Expression.Call(
								Expression.MakeMemberAccess(	//      element.Attributes
									element,
									_attributesProperty),
								_tryGetValueMethod,				//                        .TryGetValue(
								Expression.Constant(Name),		//                                     Name,
								valueVariable)),				//                                           out value))
						Expression.Constant(false),				//     => false;

						Expression.Block(typeof(bool),			// else
							equalExpression						//     => expression;
						)
					),
				]
			);
		}

		public override bool IsMatch(Element element)
		{
			if (!element.Attributes.TryGetValue(Name, out string? value))
				return false;

			value ??= string.Empty;

			return Kind switch
			{
				SelectorFilterKind.AttribEq or SelectorFilterKind.CSAttribEq
					=> value == Value,

				SelectorFilterKind.AttribContains or SelectorFilterKind.CSAttribContains
					=> value.Contains(Value, StringComparison.Ordinal),

				SelectorFilterKind.AttribStartsWith or SelectorFilterKind.CSAttribStartsWith
					=> value.StartsWith(Value, StringComparison.Ordinal),

				SelectorFilterKind.AttribEndsWith or SelectorFilterKind.CSAttribEndsWith
					=> value.EndsWith(Value, StringComparison.Ordinal),

				SelectorFilterKind.AttribIncludes or SelectorFilterKind.CSAttribIncludes
					=> StringIncludes(value, Value, StringComparison.Ordinal),

				SelectorFilterKind.AttribDashMatch or SelectorFilterKind.CSAttribDashMatch
					=> StringDashMatches(value, Value, StringComparison.Ordinal),

				SelectorFilterKind.CIAttribEq
					=> value.Equals(Value, StringComparison.OrdinalIgnoreCase),

				SelectorFilterKind.CIAttribContains
					=> value.Contains(Value, StringComparison.OrdinalIgnoreCase),

				SelectorFilterKind.CIAttribStartsWith
					=> value.StartsWith(Value, StringComparison.OrdinalIgnoreCase),

				SelectorFilterKind.CIAttribEndsWith
					=> value.EndsWith(Value, StringComparison.OrdinalIgnoreCase),

				SelectorFilterKind.CIAttribIncludes
					=> StringIncludes(value, Value, StringComparison.OrdinalIgnoreCase),

				SelectorFilterKind.CIAttribDashMatch
					=> StringDashMatches(value, Value, StringComparison.OrdinalIgnoreCase),

				_ => false,
			};
		}

		private static bool StringDashMatches(string haystack, string needle, StringComparison stringComparison)
		{
			return haystack.StartsWith(needle, stringComparison)
				&& (haystack.Length == needle.Length || haystack[haystack.Length - 1] == '-');
		}

		private static bool StringIncludes(string haystack, string needle, StringComparison stringComparison)
		{
			if (string.IsNullOrEmpty(haystack)
				|| string.IsNullOrEmpty(needle))
				return false;

			// Spin over the string, using Ordinal.IndexOf to do the heavy lifting of being a fast search.
			for (int ptr, start = 0; start <= haystack.Length - needle.Length; start = ptr + 1)
			{
				// Search for the next instance of the needle in the haystack.
				ptr = haystack.IndexOf(needle, start, stringComparison);
				if (ptr < 0)
					return false;

				// If the instance is bounded by whitespace (and being at the start or end of
				// the haystack counts as being bounded by whitespace) then we found a match.
				if ((ptr <= 0 || haystack[ptr - 1] <= 32)
					&& (ptr + needle.Length >= haystack.Length || haystack[ptr + needle.Length] <= 32))
					return true;
			}

			return false;
		}

		public override bool Equals(SelectorFilter? filter)
			=> ReferenceEquals(this, filter) ? true
				: ReferenceEquals(filter, null) ? false
				: filter is SelectorFilterAttrib other
					&& Kind == other.Kind
					&& Name == other.Name
					&& Value == other.Value;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Kind;
				hashCode = hashCode * 65599 + Name.GetHashCode();
				hashCode = hashCode * 65599 + Value.GetHashCode();
				return hashCode;
			}
		}

		public override void ToString(StringBuilder dest)
		{
			dest.Append('[');
			dest.Append(Name);

			dest.Append(Kind switch
			{
				SelectorFilterKind.AttribEq => "=",
				SelectorFilterKind.AttribIncludes => "~=",
				SelectorFilterKind.AttribDashMatch => "|=",
				SelectorFilterKind.AttribContains => "*=",
				SelectorFilterKind.AttribStartsWith => "^=",
				SelectorFilterKind.AttribEndsWith => "$=",
				_ => string.Empty,
			});

			string? escaped = Value?.AddCSlashes();
			if (string.IsNullOrEmpty(escaped) || escaped.IndexOf('\\') >= 0)
			{
				dest.Append('"');
				if (!string.IsNullOrEmpty(escaped))
					dest.Append(escaped);
				dest.Append('"');
			}
			else dest.Append(escaped);

			if (Kind >= SelectorFilterKind.CaseSensitive
				&& Kind < SelectorFilterKind.CaseInsensitive)
				dest.Append(" s");
			else if (Kind >= SelectorFilterKind.CaseInsensitive)
				dest.Append(" i");

			dest.Append(']');
		}
	}
}
