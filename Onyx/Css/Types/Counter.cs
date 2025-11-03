using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Onyx.Css.Types
{
	public readonly struct Counter : IEquatable<Counter>
	{
		public readonly string Name;
		public readonly int Value;

		public Counter(string name, int value)
		{
			Name = name;
			Value = value;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Counter other && Equals(other);

		public bool Equals(Counter other)
			=> Name == other.Name && Value == other.Value;

		public override int GetHashCode()
			=> unchecked(Name.GetHashCode() * 65599 + Value);

		public static bool operator ==(Counter a, Counter b)
			=> a.Equals(b);

		public static bool operator !=(Counter a, Counter b)
			=> !a.Equals(b);

		public override string ToString()
			=> Name + " " + Value.ToString(CultureInfo.InvariantCulture);
	}
}
