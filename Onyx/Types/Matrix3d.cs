using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	/// <summary>
	/// A 3x3 matrix in row order, like you learned in school.  We use this to avoid
	/// taking a dependency on System.Numerics or any other external math library.
	/// </summary>
	public readonly struct Matrix3d : IEquatable<Matrix3d>
	{
		public readonly double M11, M12, M13;
		public readonly double M21, M22, M23;
		public readonly double M31, M32, M33;

		public readonly double Determinant
			=>    (M11 * M22 * M33) + (M12 * M23 * M31) + (M13 * M21 * M32)
				- (M13 * M22 * M31) - (M11 * M23 * M32) - (M12 * M21 * M33);

		public readonly double Trace
			=> M11 + M22 + M33;

		public readonly Matrix3d Transposed
			=> new Matrix3d(M11, M21, M31, M12, M22, M32, M13, M23, M33);

		public readonly Matrix3d Normalized
		{
			get
			{
				double d = 1.0 / Determinant;
				return new Matrix3d(M11 * d, M12 * d, M13 * d, M21 * d, M22 * d, M23 * d, M31 * d, M32 * d, M33 * d);
			}
		}

		public readonly bool Invertible
			=> Math.Abs(
				  M11 * ( M22 * M33 + -M23 * M32)
				+ M12 * (-M21 * M33 +  M23 * M31)
				+ M13 * ( M21 * M32 + -M22 * M31)) >= 0.000000000001;

		public readonly Matrix3d Inverted
		{
			get
			{
				double c11 =  M22 * M33 + -M23 * M32;
				double c12 = -M21 * M33 +  M23 * M31;
				double c13 =  M21 * M32 + -M22 * M31;

				double c21 = -M12 * M33 +  M13 * M32;
				double c22 =  M11 * M33 + -M13 * M31;
				double c23 = -M11 * M32 +  M12 * M31;

				double c31 =  M12 * M23 + -M13 * M22;
				double c32 = -M11 * M23 +  M13 * M21;
				double c33 =  M11 * M22 + -M12 * M21;

				double d = M11 * c11 + M12 * c12 + M13 * c13;
				if (Math.Abs(d) < 0.000000000001)
					throw new InvalidOperationException("Non-invertible matrix: Determinant is zero.");

				double di = 1.0 / d;

				return new Matrix3d(
					m11: c11 * di, m12: c21 * di, m13: c31 * di,
					m21: c12 * di, m22: c22 * di, m23: c32 * di,
					m31: c13 * di, m32: c23 * di, m33: c33 * di
				);
			}
		}

		public double this[int row, int column]
			=> row switch
			{
				0 => column switch { 0 => M11, 1 => M12, 2 => M13, _ => throw new ArgumentOutOfRangeException() },
				1 => column switch { 0 => M21, 1 => M22, 2 => M23, _ => throw new ArgumentOutOfRangeException() },
				2 => column switch { 0 => M31, 1 => M32, 2 => M33, _ => throw new ArgumentOutOfRangeException() },
				_ => throw new ArgumentOutOfRangeException()
			};

		public static Matrix3d Identity { get; } = new Matrix3d(1, 0, 0, 0, 1, 0, 0, 0, 1);

		public static Matrix3d Zero { get; } = new Matrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Matrix3d(
			double m11, double m12, double m13,
			double m21, double m22, double m23,
			double m31, double m32, double m33)
		{
			M11 = m11; M12 = m12; M13 = m13;
			M21 = m21; M22 = m22; M23 = m23;
			M31 = m31; M32 = m32; M33 = m33;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Matrix3d(Matrix3x2d m)
		{
			M11 = m.M11; M12 = m.M12; M13 = m.M13;
			M21 = m.M21; M22 = m.M22; M23 = m.M23;
			M31 = 0; M32 = 0; M33 = 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Matrix3d(double m11, double m12, double m21, double m22)
			: this(m11, m12, 0, m21, m22, 0, 0, 0, 1)
		{ }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Matrix3d(Matrix3x2d m)
			=> new Matrix3d(m);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3d operator +(Matrix3d a, Matrix3d b)
			=> new Matrix3d(
				a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13,
				a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23,
				a.M31 + b.M31, a.M32 + b.M32, a.M33 + b.M33
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3d operator -(Matrix3d a, Matrix3d b)
			=> new Matrix3d(
				a.M11 - b.M11, a.M12 - b.M12, a.M13 - b.M13,
				a.M21 - b.M21, a.M22 - b.M22, a.M23 - b.M23,
				a.M31 - b.M31, a.M32 - b.M32, a.M33 - b.M33
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3d operator -(Matrix3d m)
			=> new Matrix3d(
				-m.M11, -m.M12, -m.M13,
				-m.M21, -m.M22, -m.M23,
				-m.M31, -m.M32, -m.M33
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3d operator *(Matrix3d m, double s)
			=> new Matrix3d(
				m.M11 * s, m.M12 * s, m.M13 * s,
				m.M21 * s, m.M22 * s, m.M23 * s,
				m.M31 * s, m.M32 * s, m.M33 * s
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Matrix3d operator /(Matrix3d m, double s)
			=> m * (1.0 / s);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Vector2d operator *(Matrix3d m, Vector2d v)
			=> new Vector2d(
				v.X * m.M11 + v.Y * m.M12 + m.M13,
				v.X * m.M21 + v.Y * m.M22 + m.M23
			);

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static Matrix3d operator *(Matrix3d a, Matrix3d b)
			=> new Matrix3d(
				m11: (a.M11 * b.M11) + (a.M12 * b.M21) + (a.M13 * b.M31),
				m12: (a.M11 * b.M12) + (a.M12 * b.M22) + (a.M13 * b.M32),
				m13: (a.M11 * b.M13) + (a.M12 * b.M23) + (a.M13 * b.M33),
				m21: (a.M21 * b.M11) + (a.M22 * b.M21) + (a.M23 * b.M31),
				m22: (a.M21 * b.M12) + (a.M22 * b.M22) + (a.M23 * b.M32),
				m23: (a.M21 * b.M13) + (a.M22 * b.M23) + (a.M23 * b.M33),
				m31: (a.M31 * b.M11) + (a.M32 * b.M21) + (a.M33 * b.M31),
				m32: (a.M31 * b.M12) + (a.M32 * b.M22) + (a.M33 * b.M32),
				m33: (a.M31 * b.M13) + (a.M32 * b.M23) + (a.M33 * b.M33)
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3d CreateTranslation(double dx, double dy)
			=> new Matrix3d(1, 0, dx, 0, 1, dy, 0, 0, 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3d CreateScale(double s)
			=> new Matrix3d(s, 0, 0, 0, s, 0, 0, 0, s);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3d CreateScale(double sx, double sy)
			=> new Matrix3d(sx, 0, 0, 0, sy, 0, 0, 0, 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix3d CreateRotation(double angle)
		{
			double s = Math.Sin(angle);
			double c = Math.Cos(angle);
			return new Matrix3d(c, s, 0, -s, c, 0, 0, 0, 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Matrix3d other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public bool Equals(Matrix3d other)
			=> M11 == other.M11
				&& M12 == other.M12
				&& M13 == other.M13
				&& M21 == other.M21
				&& M22 == other.M22
				&& M23 == other.M23
				&& M31 == other.M31
				&& M32 == other.M32
				&& M33 == other.M33;

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
				hashCode = hashCode * 65599 + M31.GetHashCode();
				hashCode = hashCode * 65599 + M32.GetHashCode();
				hashCode = hashCode * 65599 + M33.GetHashCode();
				return hashCode;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Matrix3d a, Matrix3d b)
			=> a.Equals(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Matrix3d a, Matrix3d b)
			=> !a.Equals(b);

		public override string ToString()
			=> $"[ {M11:0.0###},{M12:0.0###},{M13:0.0###}; {M21:0.0###},{M22:0.0###},{M23:0.0###}; {M31:0.0###},{M32:0.0###},{M33:0.0###} ]";
	}
}
