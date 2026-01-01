using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Rect2i : IEquatable<Rect2i>
	{
		public readonly int X;
		public readonly int Y;
		public readonly int Width;
		public readonly int Height;

		public Vector2i Point
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Vector2i(X, Y);
		}

		public Size2i Size
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Size2i(Width, Height);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2i()
			=> X = Y = Width = Height = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2i(int n)
			=> (X, Y, Width, Height) = (0, 0, n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2i(int x, int y, int width, int height)
			=> (X, Y, Width, Height) = (x, y, width, height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2i(Vector2i point, Size2i size)
			=> (X, Y, Width, Height) = (point.X, point.Y, size.Width, size.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Rect2i(Rect2d r)
			=> new Rect2i((Vector2i)r.Point, (Size2i)r.Size);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Rect2i a, Rect2i b)
			=> a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Rect2i a, Rect2i b)
			=> a.X != b.X || a.Y != b.Y || a.Width != b.Width || a.Height != b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Rect2i other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Rect2i other)
			=> X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> unchecked((((
				  X.GetHashCode()) * 65599
				+ Y.GetHashCode()) * 65599
				+ Width.GetHashCode()) * 65599
				+ Height.GetHashCode());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
			=> $"({Point}),({Size})";
	}
}
