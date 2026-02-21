using System.Linq.Expressions;
using System.Reflection;

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
				BindingFlags.Public | BindingFlags.Instance)!;

			public override Expression GetExpression(ParameterExpression param)
			{
				ParameterExpression resultVariable = Expression.Parameter(typeof(int), "result");

				Expression valueExpression = Expression.Call(_convertAndCompare,
					MediaQueryFeature.GetExpression(Feature, param),
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
				: MediaQueryNull.Instance;

		public static MediaQuery Create(MediaQueryKind kind, Measure left, MediaFeature right)
			=> MediaQueryFeature.GetFeatureType(right) == typeof(Measure)
				? new MediaQueryMeasureComparison(FlipComparison(kind), true, right, left)
				: MediaQueryNull.Instance;

		public static MediaQuery Create(MediaQueryKind kind, MediaFeature left, double right)
			=> MediaQueryFeature.GetFeatureType(left) == typeof(double)
				? new MediaQueryDoubleComparison(kind, false, left, right)
				: MediaQueryNull.Instance;

		public static MediaQuery Create(MediaQueryKind kind, double left, MediaFeature right)
			=> MediaQueryFeature.GetFeatureType(right) == typeof(Measure)
				? new MediaQueryDoubleComparison(FlipComparison(kind), true, right, left)
				: MediaQueryNull.Instance;

		public static MediaQuery CreateEnum<T>(MediaQueryKind kind, MediaFeature left, T right)
			where T : struct
			=> MediaQueryFeature.GetFeatureType(left) == typeof(T) && typeof(T).IsEnum && kind == MediaQueryKind.Eq
				? new MediaQueryEnumComparison<T>(kind, false, left, right)
				: MediaQueryNull.Instance;

		public static MediaQuery CreateEnum<T>(MediaQueryKind kind, T left, MediaFeature right)
			where T : struct
			=> MediaQueryFeature.GetFeatureType(right) == typeof(T) && typeof(T).IsEnum && kind == MediaQueryKind.Eq
				? new MediaQueryEnumComparison<T>(kind, true, right, left)
				: MediaQueryNull.Instance;

		public static MediaQuery CreateEnum(MediaQueryKind kind, MediaFeature left, Type rightType, object? rightValue)
			=> MediaQueryFeature.GetFeatureType(left) == rightType && rightType.IsEnum && kind == MediaQueryKind.Eq
				? (MediaQuery)Activator.CreateInstance(
					typeof(MediaQueryEnumComparison<>).MakeGenericType(rightType),
					kind, false, left, rightValue)!
				: MediaQueryNull.Instance;

		public static MediaQuery CreateEnum(MediaQueryKind kind, Type leftType, object? leftValue, MediaFeature right)
			=> MediaQueryFeature.GetFeatureType(right) == leftType && leftType.IsEnum && kind == MediaQueryKind.Eq
				? (MediaQuery)Activator.CreateInstance(
					typeof(MediaQueryEnumComparison<>).MakeGenericType(leftType),
					kind, true, right, leftValue)!
				: MediaQueryNull.Instance;

		internal static MediaQueryKind FlipComparison(MediaQueryKind kind)
			=> kind switch
			{
				MediaQueryKind.Lt => MediaQueryKind.Gt,
				MediaQueryKind.Gt => MediaQueryKind.Lt,
				MediaQueryKind.Le => MediaQueryKind.Ge,
				MediaQueryKind.Ge => MediaQueryKind.Le,
				_ => kind,
			};

		public override string ToString()
			=> $"({Left} {Kind.ToString().ToLowerInvariant()} {Right})";
	}
}
