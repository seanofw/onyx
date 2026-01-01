using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Size2i : IEquatable<Size2i>
	{
		public readonly int Width;
		public readonly int Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Size2i()
			=> Width = Height = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Size2i(int n)
			=> (Width, Height) = (n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Size2i(int width, int height)
			=> (Width, Height) = (width, height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Vector2i(Size2i s)
			=> new Vector2i(s.Width, s.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Size2i(Vector2i v)
			=> new Size2i(v.X, v.Y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Size2d(Size2i s)
			=> new Size2d(s.Width, s.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2i operator +(Size2i a, Size2i b)
			=> new Size2i(a.Width + b.Width, a.Height + b.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2i operator -(Size2i a, Size2i b)
			=> new Size2i(a.Width - b.Width, a.Height - b.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2i operator +(Size2i v)
			=> v;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2i operator -(Size2i v)
			=> new Size2i(-v.Width, -v.Height);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2i operator *(Size2i s, int scale)
			=> new Size2i(s.Width * scale, s.Height * scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Size2i operator /(Size2i s, int scale)
			=> new Size2i(s.Width / scale, s.Height / scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Size2i a, Size2i b)
			=> a.Width == b.Width && a.Height == b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Size2i a, Size2i b)
			=> a.Width != b.Width || a.Height != b.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Size2i other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Size2i other)
			=> Width == other.Width && Height == other.Height;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> unchecked(Width.GetHashCode() * 65599 + Height.GetHashCode());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
			=> $"{Width.ToString(CultureInfo.InvariantCulture)},{Height.ToString(CultureInfo.InvariantCulture)}";
	}
}
