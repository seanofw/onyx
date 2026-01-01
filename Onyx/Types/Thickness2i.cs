using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Thickness2i : IEquatable<Thickness2i>
	{
		public readonly int Top;
		public readonly int Right;
		public readonly int Bottom;
		public readonly int Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2i()
			=> Top = Right = Bottom = Left = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2i(int n)
			=> (Top, Right, Bottom, Left) = (n, n, n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2i(int vert, int horz)
			=> (Top, Right, Bottom, Left) = (vert, horz, vert, horz);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2i(int top, int horz, int bottom)
			=> (Top, Right, Bottom, Left) = (top, horz, bottom, horz);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2i(int top, int right, int bottom, int left)
			=> (Top, Right, Bottom, Left) = (top, right, bottom, left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Thickness2d(Thickness2i thickness)
			=> new Thickness2d(thickness.Top, thickness.Left, thickness.Right, thickness.Bottom);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2i operator +(Thickness2i a, Thickness2i b)
			=> new Thickness2i(a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom, a.Left + b.Left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2i operator +(Thickness2i t)
			=> t;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2i operator -(Thickness2i a, Thickness2i b)
			=> new Thickness2i(a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom, a.Left - b.Left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2i operator -(Thickness2i t)
			=> new Thickness2i(-t.Top, -t.Right, -t.Bottom, -t.Left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2i operator *(Thickness2i t, int scale)
			=> new Thickness2i(t.Top * scale, t.Right * scale, t.Bottom * scale, t.Left * scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2i operator /(Thickness2i t, int scale)
			=> new Thickness2i(t.Top / scale, t.Right / scale, t.Bottom / scale, t.Left / scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Thickness2i a, Thickness2i b)
			=> a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom && a.Left == b.Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Thickness2i a, Thickness2i b)
			=> a.Top != b.Top || a.Right != b.Right || a.Bottom != b.Bottom || a.Left != b.Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Thickness2i other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Thickness2i other)
			=> Top == other.Top && Right == other.Right && Bottom == other.Bottom && Left == other.Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> unchecked((((
				  Top.GetHashCode()) * 65599
				+ Right.GetHashCode()) * 65599
				+ Bottom.GetHashCode()) * 65599
				+ Left.GetHashCode());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString()
			=> $"{Top.ToString(CultureInfo.InvariantCulture)},{Right.ToString(CultureInfo.InvariantCulture)},{Bottom.ToString(CultureInfo.InvariantCulture)},{Left.ToString(CultureInfo.InvariantCulture)}";
	}
}
