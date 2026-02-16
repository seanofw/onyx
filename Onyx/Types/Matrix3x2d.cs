using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	/// <summary>
	/// A 3x2 matrix in row order, like you learned in school.  We use this to avoid
	/// taking a dependency on System.Numerics or any other external math library.
	/// </summary>
	public readonly struct Matrix3x2d : IEquatable<Matrix3x2d>
	{
		public readonly double M11, M12, M13;
		public readonly double M21, M22, M23;

		public double this[int row, int column]
			=> row switch
			{
				0 => column switch { 0 => M11, 1 => M12, 2 => M13, _ => throw new ArgumentOutOfRangeException() },
				1 => column switch { 0 => M21, 1 => M22, 2 => M23, _ => throw new ArgumentOutOfRangeException() },
				_ => throw new ArgumentOutOfRangeException()
			};

		public static Matrix3x2d Identity { get; } = new Matrix3x2d(1, 0, 0, 0, 1, 0);

		public static Matrix3x2d Zero { get; } = new Matrix3x2d(0, 0, 0, 0, 0, 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Matrix3x2d(
			double m11, double m12, double m13,
			double m21, double m22, double m23)
		{
			M11 = m11; M12 = m12; M13 = m13;
			M21 = m21; M22 = m22; M23 = m23;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Matrix3x2d(Matrix3d m)
		{
			M11 = m.M11; M12 = m.M12; M13 = m.M13;
			M21 = m.M21; M22 = m.M22; M23 = m.M23;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Matrix3x2d(double m11, double m12, double m21, double m22)
			: this(m11, m12, 0, m21, m22, 0)
		{ }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Matrix3x2d(Matrix3d m)
			=> new Matrix3x2d(m);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3x2d operator +(Matrix3x2d a, Matrix3x2d b)
			=> new Matrix3x2d(
				a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13,
				a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3x2d operator -(Matrix3x2d a, Matrix3x2d b)
			=> new Matrix3x2d(
				a.M11 - b.M11, a.M12 - b.M12, a.M13 - b.M13,
				a.M21 - b.M21, a.M22 - b.M22, a.M23 - b.M23
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3x2d operator -(Matrix3x2d m)
			=> new Matrix3x2d(
				-m.M11, -m.M12, -m.M13,
				-m.M21, -m.M22, -m.M23
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3x2d operator *(Matrix3x2d m, double s)
			=> new Matrix3x2d(
				m.M11 * s, m.M12 * s, m.M13 * s,
				m.M21 * s, m.M22 * s, m.M23 * s
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3x2d operator /(Matrix3x2d m, double s)
			=> m * (1.0 / s);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector2d operator *(Matrix3x2d m, Vector2d v)
			=> new Vector2d(
				v.X * m.M11 + v.Y * m.M12 + m.M13,
				v.X * m.M21 + v.Y * m.M22 + m.M23
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2d CreateTranslation(double dx, double dy)
			=> new Matrix3x2d(1, 0, dx, 0, 1, dy);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2d CreateScale(double s)
			=> new Matrix3x2d(s, 0, 0, 0, s, 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2d CreateScale(double sx, double sy)
			=> new Matrix3x2d(sx, 0, 0, 0, sy, 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3x2d CreateRotation(double angle)
		{
			double s = Math.Sin(angle);
			double c = Math.Cos(angle);
			return new Matrix3x2d(c, s, 0, -s, c, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Matrix3x2d other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public bool Equals(Matrix3x2d other)
			=> M11 == other.M11
				&& M12 == other.M12
				&& M13 == other.M13
				&& M21 == other.M21
				&& M22 == other.M22
				&& M23 == other.M23;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + M11.GetHashCode();
				hashCode = hashCode * 65599 + M12.GetHashCode();
				hashCode = hashCode * 65599 + M13.GetHashCode();
				hashCode = hashCode * 65599 + M21.GetHashCode();
				hashCode = hashCode * 65599 + M22.GetHashCode();
				hashCode = hashCode * 65599 + M23.GetHashCode();
				return hashCode;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Matrix3x2d a, Matrix3x2d b)
			=> a.Equals(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Matrix3x2d a, Matrix3x2d b)
			=> !a.Equals(b);

		public override string ToString()
			=> $"[ {M11:0.0###},{M12:0.0###},{M13:0.0###}; {M21:0.0###},{M22:0.0###},{M23:0.0###} ]";
	}
}
