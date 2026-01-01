using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Onyx.Types
{
	public readonly struct Thickness2d : IEquatable<Thickness2d>
	{
		public readonly double Top;
		public readonly double Right;
		public readonly double Bottom;
		public readonly double Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2d()
			=> Top = Right = Bottom = Left = 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2d(double n)
			=> (Top, Right, Bottom, Left) = (n, n, n, n);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2d(double vert, double horz)
			=> (Top, Right, Bottom, Left) = (vert, horz, vert, horz);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2d(double top, double horz, double bottom)
			=> (Top, Right, Bottom, Left) = (top, horz, bottom, horz);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Thickness2d(double top, double right, double bottom, double left)
			=> (Top, Right, Bottom, Left) = (top, right, bottom, left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Thickness2i(Thickness2d thickness)
			=> new Thickness2i((int)thickness.Top, (int)thickness.Left, (int)thickness.Right, (int)thickness.Bottom);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2d operator +(Thickness2d a, Thickness2d b)
			=> new Thickness2d(a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom, a.Left + b.Left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2d operator +(Thickness2d t)
			=> t;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2d operator -(Thickness2d a, Thickness2d b)
			=> new Thickness2d(a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom, a.Left - b.Left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2d operator -(Thickness2d t)
			=> new Thickness2d(-t.Top, -t.Right, -t.Bottom, -t.Left);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2d operator *(Thickness2d t, double scale)
			=> new Thickness2d(t.Top * scale, t.Right * scale, t.Bottom * scale, t.Left * scale);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Thickness2d operator /(Thickness2d t, double scale)
		{
			double inv = 1.0 / scale;
			return new Thickness2d(t.Top * inv, t.Right * inv, t.Bottom * inv, t.Left * inv);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Thickness2d a, Thickness2d b)
			=> a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom && a.Left == b.Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Thickness2d a, Thickness2d b)
			=> a.Top != b.Top || a.Right != b.Right || a.Bottom != b.Bottom || a.Left != b.Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Thickness2d other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Thickness2d other)
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
