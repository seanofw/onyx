using System.Diagnostics.CodeAnalysis;

namespace Onyx.Css.Types
{
	public readonly struct Width : IEquatable<Width>
	{
		public readonly Measure Offset;
		public readonly bool Auto;

		public Width(Measure offset)
			=> Offset = offset;

		public Width(bool auto)
			=> Auto = auto;

		internal Width(Measure offset, bool auto)
		{
			Offset = offset;
			Auto = auto;
		}

		public static implicit operator Measure(Width width)
			=> width.Auto ? PseudoMeasures.Auto : width.Offset;

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is FontFamily other && Equals(other);

		public bool Equals(Width other)
			=> Offset == other.Offset && Auto == other.Auto;

		public override int GetHashCode()
			=> unchecked(Offset.GetHashCode() * 65599 + (Auto ? 1 : 0));

		public static bool operator ==(Width a, Width b)
			=> a.Equals(b);

		public static bool operator !=(Width a, Width b)
			=> !a.Equals(b);

		public override string ToString()
			=> Auto ? "auto" : Offset.ToString();
	}
}
