using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Onyx.Extensions;

namespace Onyx.Css.Types.Media
{
	public abstract class MediaQueryComparison : MediaQueryBinary
	{
		public MediaFeature Feature { get; }
		public bool IsFlipped { get; }
		public abstract object Value { get; }

		protected MediaQueryComparison(MediaQueryKind kind, bool isFlipped,
			MediaFeature feature, MediaQuery right)
			: base(kind, new MediaQueryFeature(feature), right)
		{
			Feature = feature;
			IsFlipped = isFlipped;
		}

		private sealed class MediaQueryMeasureComparison : MediaQueryComparison
		{
			public Measure ComparisonValue { get; }
			public override object Value => ComparisonValue;

			public MediaQueryMeasureComparison(MediaQueryKind kind, bool isInverted,
				MediaFeature feature, Measure comparisonValue)
				: base(kind, isInverted, feature, new MediaQueryMeasure(comparisonValue))
			{
				ComparisonValue = comparisonValue;
			}

			private MethodInfo _measure_CompareTo = typeof(Measure).GetMethod(nameof(Measure.CompareTo),
				BindingFlags.Public | BindingFlags.Instance,
				[typeof(Measure)])!;

			private PropertyInfo _nullable_bool_HasValue = typeof(Nullable<bool>).GetProperty(
				nameof(Nullable<bool>.HasValue), BindingFlags.Public | BindingFlags.Instance)!;

			private PropertyInfo _nullable_bool_Value = typeof(Nullable<bool>).GetProperty(
				nameof(Nullable<bool>.Value), BindingFlags.Public | BindingFlags.Instance)!;

			private PropertyInfo _nullable_int_HasValue = typeof(Nullable<int>).GetProperty(
				nameof(Nullable<int>.HasValue), BindingFlags.Public | BindingFlags.Instance)!;

			private PropertyInfo _nullable_int_Value = typeof(Nullable<int>).GetProperty(
				nameof(Nullable<int>.Value), BindingFlags.Public | BindingFlags.Instance)!;

			public override Expression GetExpression(ParameterExpression param)
			{
				ParameterExpression nullableValueVariable = Expression.Parameter(typeof(int?), "nullableValue");

				Expression nullableValueExpression = Expression.Call(_convertAndCompare,
					MediaQueryFeature.GetExpression(Feature, param),
					Expression.Constant(ComparisonValue));

				Expression valueExpression = Expression.Property(nullableValueVariable, _nullable_int_Value);

				MediaQueryKind kind = IsFlipped ? FlipComparison(Kind) : Kind;

				Expression comparisonExpression = kind switch
				{
					MediaQueryKind.Eq => Expression.Equal(valueExpression, Expression.Constant(0)),
					MediaQueryKind.Lt => Expression.LessThan(valueExpression, Expression.Constant(0)),
					MediaQueryKind.Gt => Expression.GreaterThan(valueExpression, Expression.Constant(0)),
					MediaQueryKind.Le => Expression.LessThanOrEqual(valueExpression, Expression.Constant(0)),
					MediaQueryKind.Ge => Expression.GreaterThanOrEqual(valueExpression, Expression.Constant(0)),
					_ => Expression.Constant(null),
				};

				Expression condition = Expression.Condition(
					Expression.Property(nullableValueVariable, _nullable_int_HasValue),
					Expression.Convert(
						comparisonExpression,
						typeof(bool?)
					),
					Expression.Constant(null, typeof(bool?))
				);

				// Generates the equivalent of:
				//
				//     {
				//         int? nullableValue = ConvertAndCompare(feature..., ComparisonValue);
				//         return nullableValue.HasValue ? (bool?)(nullableValue.Value [cmp] 0) : (bool?)null;
				//     }
				//
				Expression result = Expression.Block(typeof(bool?),
					[nullableValueVariable],
					[
						Expression.Assign(nullableValueVariable, nullableValueExpression),
						condition
					]
				);

				return result;
			}

			public override bool? Eval(MediaQueryContext context)
			{
				Measure featureValue = (Measure)MediaQueryFeature.GetValue(Feature, context)!;

				int? result = ConvertAndCompare(featureValue, ComparisonValue);
				if (!result.HasValue)
					return null;

				int resultValue = result.Value;

				if (IsFlipped)
					resultValue = -resultValue;

				return Kind switch
				{
					MediaQueryKind.Eq => resultValue == 0,
					MediaQueryKind.Lt => resultValue < 0,
					MediaQueryKind.Gt => resultValue > 0,
					MediaQueryKind.Le => resultValue <= 0,
					MediaQueryKind.Ge => resultValue >= 0,
					_ => null,
				};
			}

			private static int? ConvertAndCompare(Measure a, Measure b)
			{
				if (!a.TryConvert(b.Units, out Measure convertedValue))
					return null;

				int result = convertedValue.CompareTo(b);
				return result;
			}

			private static readonly MethodInfo _convertAndCompare = typeof(MediaQueryMeasureComparison)
				.GetMethod(nameof(ConvertAndCompare), BindingFlags.NonPublic | BindingFlags.Static)!;
		}

		private sealed class MediaQueryDoubleComparison : MediaQueryComparison
		{
			public double ComparisonValue { get; }
			public override object Value => ComparisonValue;

			public MediaQueryDoubleComparison(MediaQueryKind kind, bool isInverted,
				MediaFeature feature, double comparisonValue)
				: base(kind, isInverted, feature, new MediaQueryNumber(comparisonValue))
			{
				ComparisonValue = comparisonValue;
			}

			private MethodInfo _double_CompareTo = typeof(double).GetMethod(nameof(double.CompareTo),
				BindingFlags.Public | BindingFlags.Instance)!;

			public override Expression GetExpression(ParameterExpression param)
			{
				ParameterExpression resultVariable = Expression.Parameter(typeof(int), "result");

				Expression valueExpression = Expression.Call(
					MediaQueryFeature.GetExpression(Feature, param),
					_double_CompareTo,
					Expression.Constant(ComparisonValue));

				Expression orderedValueExpression = valueExpression;
				if (IsFlipped)
					orderedValueExpression = Expression.Negate(orderedValueExpression);

				Expression comparisonExpression = Kind switch
				{
					MediaQueryKind.Eq => Expression.Equal(orderedValueExpression, Expression.Constant(0)),
					MediaQueryKind.Lt => Expression.LessThan(orderedValueExpression, Expression.Constant(0)),
					MediaQueryKind.Gt => Expression.GreaterThan(orderedValueExpression, Expression.Constant(0)),
					MediaQueryKind.Le => Expression.LessThanOrEqual(orderedValueExpression, Expression.Constant(0)),
					MediaQueryKind.Ge => Expression.GreaterThanOrEqual(orderedValueExpression, Expression.Constant(0)),
					_ => Expression.Constant(null),
				};

				return Expression.Block(typeof(bool),
					[resultVariable],
					[
						Expression.Assign(resultVariable, valueExpression),
						Expression.IfThenElse(
							Expression.Equal(resultVariable, Expression.Constant(null)),
							Expression.Constant(null),
							comparisonExpression
						)
					]
				);
			}

			public override bool? Eval(MediaQueryContext context)
			{
				double featureValue = (double)MediaQueryFeature.GetValue(Feature, context)!;

				int? result = featureValue.CompareTo(ComparisonValue);
				if (!result.HasValue)
					return null;

				int resultValue = result.Value;

				if (IsFlipped)
					resultValue = -resultValue;

				return Kind switch
				{
					MediaQueryKind.Eq => resultValue == 0,
					MediaQueryKind.Lt => resultValue < 0,
					MediaQueryKind.Gt => resultValue > 0,
					MediaQueryKind.Le => resultValue <= 0,
					MediaQueryKind.Ge => resultValue >= 0,
					_ => null,
				};
			}
		}

		private sealed class MediaQueryEnumComparison<T> : MediaQueryComparison
			where T : struct
		{
			public T ComparisonValue { get; }
			public override object Value => ComparisonValue;

			public MediaQueryEnumComparison(MediaQueryKind kind, bool isInverted,
				MediaFeature feature, T comparisonValue)
				: base(kind, isInverted, feature, new MediaQueryEnum<T>(comparisonValue))
			{
				ComparisonValue = comparisonValue;
			}

			public override Expression GetExpression(ParameterExpression param)
				=> Expression.Equal(
					MediaQueryFeature.GetExpression(Feature, param),
					Expression.Constant(ComparisonValue));

			public override bool? Eval(MediaQueryContext context)
				=> object.Equals(MediaQueryFeature.GetValue(Feature, context), ComparisonValue);
		}

		public static MediaQuery Create(MediaQueryKind kind, MediaFeature left, Measure right)
			=> MediaQueryFeature.GetFeatureType(left) == typeof(Measure)
				? new MediaQueryMeasureComparison(kind, false, left, right)
				: MediaQueryError.Instance;

		public static MediaQuery Create(MediaQueryKind kind, Measure left, MediaFeature right)
			=> MediaQueryFeature.GetFeatureType(right) == typeof(Measure)
				? new MediaQueryMeasureComparison(FlipComparison(kind), true, right, left)
				: MediaQueryError.Instance;

		public static MediaQuery Create(MediaQueryKind kind, MediaFeature left, double right)
			=> MediaQueryFeature.GetFeatureType(left) == typeof(double)
				? new MediaQueryDoubleComparison(kind, false, left, right)
				: MediaQueryError.Instance;

		public static MediaQuery Create(MediaQueryKind kind, double left, MediaFeature right)
			=> MediaQueryFeature.GetFeatureType(right) == typeof(Measure)
				? new MediaQueryDoubleComparison(FlipComparison(kind), true, right, left)
				: MediaQueryError.Instance;

		public static MediaQuery CreateEnum<T>(MediaQueryKind kind, MediaFeature left, T right)
			where T : struct
			=> MediaQueryFeature.GetFeatureType(left) == typeof(T) && typeof(T).IsEnum && kind == MediaQueryKind.Eq
				? new MediaQueryEnumComparison<T>(kind, false, left, right)
				: MediaQueryError.Instance;

		public static MediaQuery CreateEnum<T>(MediaQueryKind kind, T left, MediaFeature right)
			where T : struct
			=> MediaQueryFeature.GetFeatureType(right) == typeof(T) && typeof(T).IsEnum && kind == MediaQueryKind.Eq
				? new MediaQueryEnumComparison<T>(kind, true, right, left)
				: MediaQueryError.Instance;

		public static MediaQuery CreateEnum(MediaQueryKind kind, MediaFeature left, Type rightType, object? rightValue)
			=> MediaQueryFeature.GetFeatureType(left) == rightType && rightType.IsEnum && kind == MediaQueryKind.Eq
				? (MediaQuery)Activator.CreateInstance(
					typeof(MediaQueryEnumComparison<>).MakeGenericType(rightType),
					kind, false, left, rightValue)!
				: MediaQueryError.Instance;

		public static MediaQuery CreateEnum(MediaQueryKind kind, Type leftType, object? leftValue, MediaFeature right)
			=> MediaQueryFeature.GetFeatureType(right) == leftType && leftType.IsEnum && kind == MediaQueryKind.Eq
				? (MediaQuery)Activator.CreateInstance(
					typeof(MediaQueryEnumComparison<>).MakeGenericType(leftType),
					kind, true, right, leftValue)!
				: MediaQueryError.Instance;

		internal static MediaQueryKind FlipComparison(MediaQueryKind kind)
			=> kind switch
			{
				MediaQueryKind.Lt => MediaQueryKind.Gt,
				MediaQueryKind.Gt => MediaQueryKind.Lt,
				MediaQueryKind.Le => MediaQueryKind.Ge,
				MediaQueryKind.Ge => MediaQueryKind.Le,
				_ => kind,
			};

		public override void ToString(StringBuilder dest)
		{
			dest.Append("(");
			Left.ToString(dest);

			dest.Append(Kind switch
			{
				MediaQueryKind.Lt => " < ",
				MediaQueryKind.Gt => " > ",
				MediaQueryKind.Le => " <= ",
				MediaQueryKind.Ge => " >= ",
				MediaQueryKind.Eq => " = ",
				_ => " " + Kind.ToString().Hyphenize() + " ",
			});

			Right.ToString(dest);
			dest.Append(")");
		}
	}
}
