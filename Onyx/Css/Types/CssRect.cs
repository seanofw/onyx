using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Onyx.Css.Types
{
	public readonly struct CssRect : IEquatable<CssRect>
	{
		public readonly Measure Top;
		public readonly Measure Right;
		public readonly Measure Bottom;
		public readonly Measure Left;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CssRect(Measure top, Measure right, Measure bottom, Measure left)
		{
			Top = top;
			Right = right;
			Bottom = bottom;
			Left = left;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is CssRect other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(CssRect other)
			=> Top == other.Top && Right == other.Right
				&& Bottom == other.Bottom && Left == other.Left;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + Top.GetHashCode();
				hashCode = hashCode * 65599 + Right.GetHashCode();
				hashCode = hashCode * 65599 + Bottom.GetHashCode();
				hashCode = hashCode * 65599 + Left.GetHashCode();
				return hashCode;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(CssRect a, CssRect b)
			=> a.Equals(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(CssRect a, CssRect b)
			=> !a.Equals(b);

		public override string ToString()
			=> $"rect({Top}, {Right}, {Bottom}, {Left})";
	}
}
