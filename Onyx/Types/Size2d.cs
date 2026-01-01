using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Size2d : IEquatable<Size2d>
	{
		public readonly double Width;
		public readonly double Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Size2d()
			=> Width = Height = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Size2d(double n)
			=> (Width, Height) = (n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Size2d(double width, double height)
			=> (Width, Height) = (width, height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Vector2d(Size2d s)
			=> new Vector2d(s.Width, s.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Size2d(Vector2d s)
			=> new Size2d(s.X, s.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Size2i(Size2d s)
			=> new Size2i((int)s.Width, (int)s.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2d operator +(Size2d a, Size2d b)
			=> new Size2d(a.Width + b.Width, a.Height + b.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2d operator -(Size2d a, Size2d b)
			=> new Size2d(a.Width - b.Width, a.Height - b.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2d operator +(Size2d v)
			=> v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2d operator -(Size2d v)
			=> new Size2d(-v.Width, -v.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2d operator *(Size2d s, double scale)
			=> new Size2d(s.Width * scale, s.Height * scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2d operator /(Size2d s, double scale)
		{
			double inv = 1.0 / scale;
			return new Size2d(s.Width * scale, s.Height * scale);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Size2d a, Size2d b)
			=> a.Width == b.Width && a.Height == b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Size2d a, Size2d b)
			=> a.Width != b.Width || a.Height != b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Size2d other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Size2d other)
			=> Width == other.Width && Height == other.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> unchecked(Width.GetHashCode() * 65599 + Height.GetHashCode());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
			=> $"{Width.ToString(CultureInfo.InvariantCulture)},{Height.ToString(CultureInfo.InvariantCulture)}";
	}
}
