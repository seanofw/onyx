using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Vector2d : IEquatable<Vector2d>
	{
		public readonly double X;
		public readonly double Y;

		public double Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Math.Sqrt(X * X + Y * Y);
		}

		public double LengthSquared
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => X * X + Y * Y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2d()
			=> X = Y = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2d(double n)
			=> (X, Y) = (n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2d(double x, double y)
			=> (X, Y) = (x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Vector2i(Vector2d v)
			=> new Vector2i((int)v.X, (int)v.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d operator +(Vector2d a, Vector2d b)
			=> new Vector2d(a.X + b.X, a.Y + b.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d operator -(Vector2d a, Vector2d b)
			=> new Vector2d(a.X - b.X, a.Y - b.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d operator +(Vector2d v)
			=> v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d operator -(Vector2d v)
			=> new Vector2d(-v.X, -v.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d operator *(Vector2d s, double scale)
			=> new Vector2d(s.X * scale, s.Y * scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2d operator /(Vector2d s, double scale)
		{
			double inv = 1.0 / scale;
			return new Vector2d(s.X * scale, s.Y * scale);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public double Dot(Vector2d other)
			=> X * other.X + Y * other.Y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Vector2d a, Vector2d b)
			=> a.X == b.X && a.Y == b.Y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Vector2d a, Vector2d b)
			=> a.X != b.X || a.Y != b.Y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Vector2d other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Vector2d other)
			=> X == other.X && Y == other.Y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> unchecked(X.GetHashCode() * 65599 + Y.GetHashCode());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
			=> $"{X.ToString(CultureInfo.InvariantCulture)},{Y.ToString(CultureInfo.InvariantCulture)}";
	}
}
