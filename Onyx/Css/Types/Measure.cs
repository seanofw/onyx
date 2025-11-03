using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Onyx.Css.Types
{
	public readonly struct Measure : IEquatable<Measure>, IComparable<Measure>, IComparable
	{
		public readonly Units Units;
		public readonly double Value;

		private static readonly IReadOnlyList<(Units Units, string Suffix)> _suffixes =
		[
			(Units.Em, "em"),
			(Units.Ex, "ex"),
			(Units.Pixels, "px"),
			(Units.Centimeters, "cm"),
			(Units.Millimeters, "mm"),
			(Units.Inches, "in"),
			(Units.Points, "pt"),
			(Units.Picas, "pc"),
			(Units.Degrees, "deg"),
			(Units.Radians, "rad"),
			(Units.Grads, "grad"),
			(Units.Milliseconds, "ms"),
			(Units.Seconds, "s"),
			(Units.Hertz, "Hz"),
			(Units.Kilohertz, "kHz"),
			(Units.Percent, "%"),
		];

		public static IReadOnlyDictionary<Units, string> UnitsToSuffix { get; }
			= _suffixes.ToDictionary(p => p.Units, p => p.Suffix);

		public static IReadOnlyDictionary<string, Units> SuffixToUnits { get; }
			= _suffixes.ToDictionary(p => p.Suffix, p => p.Units);

		public static Measure Zero { get; } = new Measure(Units.Pixels, 0);

		public string Suffix => UnitsToSuffix[Units];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Measure(Units units, double value)
		{
			Units = units;
			Value = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Measure other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Measure other)
			=> Units == other.Units && Value == other.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(Measure other)
			=> Units == other.Units ? Value.CompareTo(other.Value) : -1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(object? obj)
			=> obj is Measure other ? CompareTo(other) : -1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
			=> unchecked(Value.GetHashCode() * 65599 + (int)Units);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Measure a, Measure b)
			=> a.Units == b.Units && a.Value == b.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Measure a, Measure b)
			=> a.Units != b.Units || a.Value != b.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(Measure a, Measure b)
			=> a.Units == b.Units && a.Value < b.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(Measure a, Measure b)
			=> a.Units == b.Units && a.Value > b.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(Measure a, Measure b)
			=> a.Units == b.Units && a.Value <= b.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(Measure a, Measure b)
			=> a.Units == b.Units && a.Value >= b.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Measure operator +(Measure a, Measure b)
		{
			if (a.Units != b.Units && !b.TryConvert(a.Units, out b))
				throw new ArgumentException($"Cannot add {b.Units} to {a.Units}.");
			return new Measure(a.Units, a.Value + b.Value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Measure operator -(Measure a, Measure b)
		{
			if (a.Units != b.Units && !b.TryConvert(a.Units, out b))
				throw new ArgumentException($"Cannot subtract {b.Units} from {a.Units}.");
			return new Measure(a.Units, a.Value - b.Value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Measure operator +(Measure m)
			=> m;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Measure operator -(Measure m)
			=> new Measure(m.Units, -m.Value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Measure operator *(Measure m, double scalar)
			=> new Measure(m.Units, m.Value * scalar);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Measure operator /(Measure m, double scalar)
			=> new Measure(m.Units, m.Value / scalar);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Measure operator %(Measure m, double scalar)
			=> new Measure(m.Units, m.Value % scalar);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Measure Abs()
			=> new Measure(Units, Math.Abs(Value));

		public int Sign
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Math.Sign(Value);
		}

		public override string ToString()
			=> Value.ToString(CultureInfo.InvariantCulture) + Suffix;

		private const double PxToPt = 0.75;
		private const double PtToPx = 1.0 / PxToPt;

		private const double PtToPc = 12;
		private const double PcToPt = 1.0 / PtToPc;

		private const double InToPt = 72.0;
		private const double PtToIn = 1.0 / InToPt;

		private const double InToCm = 2.54;
		private const double CmToIn = 1.0 / InToCm;

		private const double CmToMm = 10;
		private const double MmToCm = 1.0 / CmToMm;

		private const double DegToRad = Math.PI / 180;
		private const double RadToDeg = 180 / Math.PI;

		private const double GradToDeg = 90 / 100;
		private const double DegToGrad = 100 / 90;

		/// <summary>
		/// Convert between measures of equivalent units.  This can apply all valid
		/// conversions.  Note that "em" and "ex" are only measured relative to their
		/// fonts, so they cannot be converted at all.  The relationship between pixels
		/// and physical measurements is fixed at 72, per the CSS spec.
		/// </summary>
		/// <param name="units">The target unit of measure.</param>
		/// <param name="measure">The resulting measure.</param>
		/// <returns>True on a successful conversion, false if there is no conversion between units.</returns>
		public bool TryConvert(Units units, out Measure measure)
		{
			if (units == Units)
			{
				measure = this;
				return true;
			}

			switch (Units)
			{
				case Units.Pixels:
					switch (units)
					{
						case Units.Centimeters:
							measure = new Measure(units, Value * (PxToPt * PtToIn * InToCm));
							return true;
						case Units.Millimeters:
							measure = new Measure(units, Value * (PxToPt * PtToIn * InToCm * CmToMm));
							return true;
						case Units.Inches:
							measure = new Measure(units, Value * (PxToPt * PtToIn));
							return true;
						case Units.Points:
							measure = new Measure(units, Value * PxToPt);
							return true;
						case Units.Picas:
							measure = new Measure(units, Value * (PxToPt * PtToPc));
							return true;
					}
					break;
				case Units.Centimeters:
					switch (units)
					{
						case Units.Pixels:
							measure = new Measure(units, Value * (CmToIn * InToPt * PtToPx));
							return true;
						case Units.Millimeters:
							measure = new Measure(units, Value * CmToMm);
							return true;
						case Units.Inches:
							measure = new Measure(units, Value * CmToIn);
							return true;
						case Units.Points:
							measure = new Measure(units, Value * (CmToIn * InToPt));
							return true;
						case Units.Picas:
							measure = new Measure(units, Value * (CmToIn * InToPt * PtToPc));
							return true;
					}
					break;
				case Units.Millimeters:
					switch (units)
					{
						case Units.Pixels:
							measure = new Measure(units, Value * (MmToCm * CmToIn * InToPt * PtToPx));
							return true;
						case Units.Centimeters:
							measure = new Measure(units, Value * MmToCm);
							return true;
						case Units.Inches:
							measure = new Measure(units, Value * (MmToCm * CmToIn));
							return true;
						case Units.Points:
							measure = new Measure(units, Value * (MmToCm * CmToIn * InToPt));
							return true;
						case Units.Picas:
							measure = new Measure(units, Value * (MmToCm * CmToIn * InToPt * PtToPc));
							return true;
					}
					break;
				case Units.Inches:
					switch (units)
					{
						case Units.Pixels:
							measure = new Measure(units, Value * (InToPt * PtToPx));
							return true;
						case Units.Centimeters:
							measure = new Measure(units, Value * InToCm);
							return true;
						case Units.Millimeters:
							measure = new Measure(units, Value * (InToCm * CmToMm));
							return true;
						case Units.Points:
							measure = new Measure(units, Value * InToPt);
							return true;
						case Units.Picas:
							measure = new Measure(units, Value * (InToPt * PtToPc));
							return true;
					}
					break;
				case Units.Points:
					switch (units)
					{
						case Units.Pixels:
							measure = new Measure(units, Value * PtToPx);
							return true;
						case Units.Centimeters:
							measure = new Measure(units, Value * (PtToIn * InToCm));
							return true;
						case Units.Millimeters:
							measure = new Measure(units, Value * (PtToIn * InToCm * CmToMm));
							return true;
						case Units.Inches:
							measure = new Measure(units, Value * PtToIn);
							return true;
						case Units.Picas:
							measure = new Measure(units, Value * PtToPc);
							return true;
					}
					break;
				case Units.Picas:
					switch (units)
					{
						case Units.Pixels:
							measure = new Measure(units, Value * (PcToPt * PtToPx));
							return true;
						case Units.Centimeters:
							measure = new Measure(units, Value * (PcToPt * PtToIn * InToCm));
							return true;
						case Units.Millimeters:
							measure = new Measure(units, Value * (PcToPt * PtToIn * InToCm * CmToMm));
							return true;
						case Units.Inches:
							measure = new Measure(units, Value * (PcToPt * PtToIn));
							return true;
						case Units.Points:
							measure = new Measure(units, Value * PcToPt);
							return true;
					}
					break;

				case Units.Degrees:
					switch (units)
					{
						case Units.Radians:
							measure = new Measure(units, Value * DegToRad);
							return true;
						case Units.Grads:
							measure = new Measure(units, Value * DegToGrad);
							return true;
					}
					break;
				case Units.Radians:
					switch (units)
					{
						case Units.Degrees:
							measure = new Measure(units, Value * RadToDeg);
							return true;
						case Units.Grads:
							measure = new Measure(units, Value * (RadToDeg * DegToGrad));
							return true;
					}
					break;
				case Units.Grads:
					switch (units)
					{
						case Units.Degrees:
							measure = new Measure(units, Value * GradToDeg);
							return true;
						case Units.Radians:
							measure = new Measure(units, Value * (GradToDeg * DegToRad));
							return true;
					}
					break;

				case Units.Milliseconds:
					if (units == Units.Seconds)
					{
						measure = new Measure(Units.Seconds, Value * (1.0 / 1000));
						return true;
					}
					break;
				case Units.Seconds:
					if (units == Units.Milliseconds)
					{
						measure = new Measure(Units.Milliseconds, Value * 1000);
						return true;
					}
					break;

				case Units.Hertz:
					if (units == Units.Kilohertz)
					{
						measure = new Measure(Units.Kilohertz, Value * (1.0 / 1000));
						return true;
					}
					break;
				case Units.Kilohertz:
					if (units == Units.Hertz)
					{
						measure = new Measure(Units.Hertz, Value * 1000);
						return true;
					}
					break;
			}

			measure = default!;
			return false;
		}
	}
}
