using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Onyx.Css.Selectors
{
	/// <summary>
	/// Specificity of a selector, as an easily-comparable 64-bit value.
	/// </summary>
	public readonly struct Specificity : IEquatable<Specificity>, IComparable<Specificity>, IComparable
	{
		// 666655555555554444444444333333333322222222221111111111
		// 3210987654321098765432109876543210987654321098765432109876543210
		// x...............................................................    A: Inline style or stylesheet
		// ..xxxxxxxxxx....................................................    B: IDs (0-1023)
		// .............xxxxxxxxxx.........................................    C: Attributes (0-1023)
		// ........................xxxxxxxxxx..............................    D: Elements (0-1023)
		// ...................................xxxxxxxxxx...................    E: Stylesheet number (0-1023)
		// ..............................................xxxxxxxxxxxxxxxxxx    F: Rule order (0-131071)
		// .0..........0..........0..........0..........0..................    Reserved (always zero)
		private readonly ulong _value;

		private const ulong InvalidBits     = 0b0100000000001000000000010000000000100000000001000000000000000000UL;
		private const ulong TooManyIds      = 0b0100000000000000000000000000000000000000000000000000000000000000UL;
		private const ulong TooManyAttrs    = 0b0000000000001000000000000000000000000000000000000000000000000000UL;
		private const ulong TooManyElements = 0b0000000000000000000000010000000000000000000000000000000000000000UL;
		private const ulong TooManySheets   = 0b0000000000000000000000000000000000100000000000000000000000000000UL;
		private const ulong TooManyRules    = 0b0000000000000000000000000000000000000000000001000000000000000000UL;

		public bool IsInlineStyle => (_value & 0x8000_0000_0000_0000UL) != 0;
		public int IdCount => (int)(_value >> 52) & 0x3FF;			// Number of #IDs in the selector
		public int AttributeCount => (int)(_value >> 41) & 0x3FF;	// Number of attributes and pseudo-classes
		public int ElementCount => (int)(_value >> 30) & 0x3FF;     // Number of elements and pseudo-elements
		public int Stylesheet => (int)(_value >> 19) & 0x3FF;       // Stylesheet number
		public int RuleIndex => (int)_value & 0x1FFFF;            // Rule index within its stylesheet

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public Specificity WithoutLocation()
			=> new Specificity(_value & ~0x8000_0000_3FFF_FFFFUL);

		public static Specificity Zero { get; } = new Specificity(0);

		public Specificity() { }

		public Specificity(ulong value)
			=> _value = (value & InvalidBits) == 0
				? value
				: throw new ArgumentException("Invalid selector");

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public Specificity(bool isInlineStyle = false,
			int idCount = 0, int attributeCount = 0, int elementCount = 0,
			int stylesheet = 0, int ruleIndex = 0)
		{
			if (stylesheet < 0 || stylesheet > 1000)
				throw new ArgumentOutOfRangeException(nameof(stylesheet), "Too many stylesheets (max 1000)");
			if (ruleIndex < 0 || ruleIndex > 100000)
				throw new ArgumentOutOfRangeException(nameof(ruleIndex), "Too many rules in this stylesheet (max 100000)");
			if (idCount < 0 || idCount > 1000)
				throw new ArgumentOutOfRangeException(nameof(idCount), "Too many IDs (max 1000)");
			if (attributeCount < 0 || attributeCount > 1000)
				throw new ArgumentOutOfRangeException(nameof(attributeCount), "Too many attributes/classes (max 1000)");
			if (elementCount < 0 || elementCount > 1000)
				throw new ArgumentOutOfRangeException(nameof(elementCount), "Too many elements (max 1000)");

			_value =
				  (isInlineStyle ? 0x8000_0000_0000_0000UL : 0)
				| (ulong)(uint)stylesheet << 19
				| (ulong)(uint)ruleIndex << 0
				| (ulong)(uint)idCount << 52
				| (ulong)(uint)attributeCount << 41
				| (ulong)(uint)elementCount << 30;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Specificity operator +(Specificity a, Specificity b)
		{
			ulong sum = a._value + b._value;

			if ((sum & InvalidBits) != 0)
				throw MakeInvalidSelectorException(sum);

			return new Specificity(sum);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Specificity operator |(Specificity a, Specificity b)
			=> new Specificity(a._value | b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Specificity operator &(Specificity a, Specificity b)
			=> new Specificity(a._value & b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Specificity operator ^(Specificity a, Specificity b)
			=> new Specificity(a._value ^ b._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static Specificity operator ~(Specificity s)
			=> new Specificity(~s._value);

		private static ArgumentException MakeInvalidSelectorException(ulong sum)
			=> new ArgumentException("Invalid selector; too many"
				+ ((sum & TooManyIds) != 0 ? "IDs (max 1000)"
					: (sum & TooManyAttrs) != 0 ? "attributes/classes (max 1000)"
					: (sum & TooManyElements) != 0 ? "elements (max 1000)"
					: (sum & TooManySheets) != 0 ? "stylesheets (max 1000)"
					: (sum & TooManyRules) != 0 ? "rules in this stylesheet (max 100000)"
					: "pieces"));

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Specificity other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Specificity other)
			=> _value == other._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> _value.GetHashCode();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Specificity a, Specificity b)
			=> a._value == b._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Specificity a, Specificity b)
			=> a._value != b._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Specificity a, Specificity b)
			=> a._value < b._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Specificity a, Specificity b)
			=> a._value > b._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(Specificity a, Specificity b)
			=> a._value <= b._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(Specificity a, Specificity b)
			=> a._value >= b._value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Specificity other)
			=> _value.CompareTo(other._value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj)
			=> obj is Specificity other ? _value.CompareTo(other._value) : -1;

		public override string ToString()
			=> $"Sheet={Stylesheet}, Rule={RuleIndex}, Ids={IdCount}, Attrs={AttributeCount}, Elems={ElementCount}";
	}
}
