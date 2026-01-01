using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Rect2d : IEquatable<Rect2d>
	{
		public readonly double X;
		public readonly double Y;
		public readonly double Width;
		public readonly double Height;

		public Vector2d Point
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Vector2d(X, Y);
		}

		public Size2d Size
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new Size2d(Width, Height);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2d()
			=> X = Y = Width = Height = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2d(double n)
			=> (X, Y, Width, Height) = (0, 0, n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2d(double x, double y, double width, double height)
			=> (X, Y, Width, Height) = (x, y, width, height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Rect2d(Vector2d point, Size2d size)
			=> (X, Y, Width, Height) = (point.X, point.Y, size.Width, size.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Rect2d(Rect2i r)
			=> new Rect2d(r.Point, r.Size);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Rect2d a, Rect2d b)
			=> a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Rect2d a, Rect2d b)
			=> a.X != b.X || a.Y != b.Y || a.Width != b.Width || a.Height != b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Rect2d other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Rect2d other)
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
