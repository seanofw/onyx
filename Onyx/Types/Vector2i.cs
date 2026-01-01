using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Vector2i : IEquatable<Vector2i>
	{
		public readonly int X;
		public readonly int Y;

		public double Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Math.Sqrt(X * X + Y * Y);
		}

		public long LengthSquared
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (long)X * X + (long)Y * Y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2i()
			=> X = Y = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2i(int n)
			=> (X, Y) = (n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Vector2i(int x, int y)
			=> (X, Y) = (x, y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Vector2d(Vector2i v)
			=> new Vector2d(v.X, v.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i operator +(Vector2i a, Vector2i b)
			=> new Vector2i(a.X + b.X, a.Y + b.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i operator -(Vector2i a, Vector2i b)
			=> new Vector2i(a.X - b.X, a.Y - b.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i operator +(Vector2i v)
			=> v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i operator -(Vector2i v)
			=> new Vector2i(-v.X, -v.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i operator *(Vector2i v, int scale)
			=> new Vector2i(v.X * scale, v.Y * scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2i operator /(Vector2i v, int scale)
			=> new Vector2i(v.X / scale, v.Y / scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Vector2i a, Vector2i b)
			=> a.X == b.X && a.Y == b.Y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Vector2i a, Vector2i b)
			=> a.X != b.X || a.Y != b.Y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Vector2i other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Vector2i other)
			=> X == other.X && Y == other.Y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> unchecked(X * 65599 + Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
			=> $"{X.ToString(CultureInfo.InvariantCulture)},{Y.ToString(CultureInfo.InvariantCulture)}";
	}
}
