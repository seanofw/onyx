using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Onyx.Css.Types
{
	/// <summary>
	/// 256-bit unsigned integer.  We're not guaranteed to have such a thing
	/// on any CPU, plus even when we do it's usually a vector register, so this
	/// provides a non-vector integer of 256 bits to work with, and implements
	/// most (but not all) of the usual operations on integer.  (Notably missing:
	/// multiplication, division, and modulus.)
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public readonly struct UInt256 : IEquatable<UInt256>, IComparable<UInt256>, IComparable
	{
		private readonly ulong _v1;
		private readonly ulong _v2;
		private readonly ulong _v3;
		private readonly ulong _v4;

		public static UInt256 Zero { get; } = default;
		public static UInt256 One { get; } = FromBit(0);

		public ulong V1 => _v1;
		public ulong V2 => _v2;
		public ulong V3 => _v3;
		public ulong V4 => _v4;

		public bool IsZero
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_v1 | _v2 | _v3 | _v4) == 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(ulong v1, ulong v2, ulong v3, ulong v4)
		{
			_v1 = v1;
			_v2 = v2;
			_v3 = v3;
			_v4 = v4;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(byte i)
			: this((ulong)i) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(sbyte i)
			: this((long)i) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(short i)
			: this((long)i) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(ushort i)
			: this((ulong)i) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(int i)
			: this((long)i) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(uint i)
			: this((ulong)i) { }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(long i)
		{
			_v1 = (ulong)i;
			_v2 = _v3 = _v4 = (ulong)(i >> 63);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256(ulong i)
		{
			_v1 = i;
			_v2 = _v3 = _v4 = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(byte i)
			=> new UInt256(i);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(sbyte i)
			=> new UInt256(i);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(short i)
			=> new UInt256(i);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(ushort i)
			=> new UInt256(i);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(int i)
			=> new UInt256(i);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(uint i)
			=> new UInt256(i);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(long i)
			=> new UInt256(i);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UInt256(ulong i)
			=> new UInt256(i);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator byte(UInt256 u)
			=> (byte)u._v1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator sbyte(UInt256 u)
			=> (sbyte)u._v1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator short(UInt256 u)
			=> (short)u._v1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator ushort(UInt256 u)
			=> (ushort)u._v1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator int(UInt256 u)
			=> (int)u._v1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator uint(UInt256 u)
			=> (uint)u._v1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator long(UInt256 u)
			=> (long)u._v1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator ulong(UInt256 u)
			=> u._v1;

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static UInt256 FromBit(int bit)
			=> new UInt256(
				v1:               bit <  64 ? (1UL << (bit -   0)) : 0,
				v2: bit >=  64 && bit < 128 ? (1UL << (bit -  64)) : 0,
				v3: bit >= 128 && bit < 192 ? (1UL << (bit - 128)) : 0,
				v4: bit >= 192              ? (1UL << (bit - 192)) : 0
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UInt256 operator |(UInt256 a, UInt256 b)
			=> new UInt256(a._v1 | b._v1, a._v2 | b._v2, a._v3 | b._v3, a._v4 | b._v4);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UInt256 operator &(UInt256 a, UInt256 b)
			=> new UInt256(a._v1 & b._v1, a._v2 & b._v2, a._v3 & b._v3, a._v4 & b._v4);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Intersects(UInt256 other)
			=> ((_v1 & other._v1) | (_v2 & other._v2) | (_v3 & other._v3) | (_v4 & other._v4)) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UInt256 operator ^(UInt256 a, UInt256 b)
			=> new UInt256(a._v1 ^ b._v1, a._v2 ^ b._v2, a._v3 ^ b._v3, a._v4 ^ b._v4);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UInt256 operator ~(UInt256 u)
			=> new UInt256(~u._v1, ~u._v2, ~u._v3, ~u._v4);

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static UInt256 operator +(UInt256 a, UInt256 b)
		{
			unchecked
			{
				ulong v1 = a._v1 + b._v1, c1 = (v1 < a._v1) ? 1UL : 0UL;

				ulong t2 = a._v2 + b._v2;
				ulong v2 = t2 + c1, c2 = (t2 < a._v2 || v2 < t2) ? 1UL : 0UL;

				ulong t3 = a._v3 + b._v3;
				ulong v3 = t3 + c2, c3 = (t3 < a._v3 || v3 < t3) ? 1UL : 0UL;

				ulong v4 = a._v4 + b._v4 + c3;

				return new UInt256(v1, v2, v3, v4);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static UInt256 operator +(UInt256 a, long b)
		{
			ulong bh = (ulong)(b >> 63);    // Propagate the sign across all bits.

			unchecked
			{
				ulong v1 = a._v1 + (ulong)b, c1 = (v1 < a._v1) ? 1UL : 0UL;

				ulong t2 = a._v2 + bh;
				ulong v2 = t2 + c1, c2 = (t2 < a._v2 || v2 < t2) ? 1UL : 0UL;

				ulong t3 = a._v3 + bh;
				ulong v3 = t3 + c2, c3 = (t3 < a._v3 || v3 < t3) ? 1UL : 0UL;

				ulong v4 = a._v4 + bh + c3;

				return new UInt256(v1, v2, v3, v4);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static UInt256 operator +(UInt256 a, ulong b)
		{
			unchecked
			{
				ulong v1 = a._v1 + b;
				ulong v2 = a._v2 + (v1 < a._v1 ? 1UL : 0UL);
				ulong v3 = a._v3 + (v2 < a._v2 ? 1UL : 0UL);
				ulong v4 = a._v4 + (v3 < a._v3 ? 1UL : 0UL);
				return new UInt256(v1, v2, v3, v4);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static UInt256 operator -(UInt256 u)
		{
			unchecked
			{
				ulong v1 = ~u._v1 +  1, c1 = (           v1 == 0) ? 1UL : 0UL;
				ulong v2 = ~u._v2 + c1, c2 = (c1 != 0 && v2 == 0) ? 1UL : 0UL;
				ulong v3 = ~u._v3 + c2, c3 = (c2 != 0 && v3 == 0) ? 1UL : 0UL;
				ulong v4 = ~u._v4 + c3;
				return new UInt256(v1, v2, v3, v4);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static UInt256 operator -(UInt256 a, UInt256 b)
		{
			unchecked
			{
				ulong v1 = a._v1 - b._v1;
				ulong b1 = (a._v1 < b._v1) ? 1UL : 0UL;

				ulong t2 = a._v2 - b._v2;
				ulong v2 = t2 - b1;
				ulong b2 = (a._v2 < b._v2 || t2 < b1) ? 1UL : 0UL;

				ulong t3 = a._v3 - b._v3;
				ulong v3 = t3 - b2;
				ulong b3 = (a._v3 < b._v3 || t3 < b2) ? 1UL : 0UL;

				ulong v4 = a._v4 - b._v4 - b3;

				return new UInt256(v1, v2, v3, v4);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UInt256 operator >>(UInt256 x, int shift)
			=> (uint)shift < 64 && shift > 0
				? ShiftRightSmall(x._v1, x._v2, x._v3, x._v4, shift)
				: ShiftRightLarge(x, shift);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UInt256 operator <<(UInt256 x, int shift)
			=> (uint)shift < 64 && shift > 0
				? ShiftLeftSmall(x._v1, x._v2, x._v3, x._v4, shift)
				: ShiftLeftLarge(x, shift);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static UInt256 ShiftRightSmall(ulong v1, ulong v2, ulong v3, ulong v4, int s)
			=> new UInt256(
				v1: (v1 >> s) | (v2 << (64 - s)),
				v2: (v2 >> s) | (v3 << (64 - s)),
				v3: (v3 >> s) | (v4 << (64 - s)),
				v4:  v4 >> s
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static UInt256 ShiftLeftSmall(ulong v1, ulong v2, ulong v3, ulong v4, int s)
			=> new UInt256(
				v1:  v1 << s,
				v2: (v2 << s) | (v1 >> (64 - s)),
				v3: (v3 << s) | (v2 >> (64 - s)),
				v4: (v4 << s) | (v3 >> (64 - s))
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static UInt256 ShiftRightSmallZeroSafe(ulong v1, ulong v2, ulong v3, ulong v4, int s)
		{
			int inv = (64 - s) & 63;
			ulong mask = s != 0 ? 0xFFFF_FFFF_FFFF_FFFFUL : 0;

			return new UInt256(
				v1: (v1 >> s) | ((v2 << inv) & mask),
				v2: (v2 >> s) | ((v3 << inv) & mask),
				v3: (v3 >> s) | ((v4 << inv) & mask),
				v4:  v4 >> s
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static UInt256 ShiftLeftSmallZeroSafe(ulong v1, ulong v2, ulong v3, ulong v4, int s)
		{
			int inv = (64 - s) & 63;
			ulong mask = s != 0 ? 0xFFFF_FFFF_FFFF_FFFFUL : 0;

			return new UInt256(
				v1:  v1 << s,
				v2: (v2 << s) | ((v1 >> inv) & mask),
				v3: (v3 << s) | ((v2 >> inv) & mask),
				v4: (v4 << s) | ((v3 >> inv) & mask)
			);
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static UInt256 ShiftRightLarge(UInt256 x, int shift)
			=> shift >= 0 ? (shift >> 6) switch
				{
					0 => ShiftRightSmallZeroSafe(x._v1, x._v2, x._v3, x._v4, shift & 63),
					1 => ShiftRightSmallZeroSafe(x._v2, x._v3, x._v4, 0, shift & 63),
					2 => ShiftRightSmallZeroSafe(x._v3, x._v4, 0, 0, shift & 63),
					3 => ShiftRightSmallZeroSafe(x._v4, 0, 0, 0, shift & 63),
					_ => default
				}
				: shift > -256 ? x << -shift
				: default;

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static UInt256 ShiftLeftLarge(UInt256 x, int shift)
			=> shift >= 0 ? (shift >> 6) switch
				{
					0 => ShiftLeftSmallZeroSafe(x._v1, x._v2, x._v3, x._v4, shift & 63),
					1 => ShiftLeftSmallZeroSafe(0, x._v1, x._v2, x._v3, shift & 63),
					2 => ShiftLeftSmallZeroSafe(0, 0, x._v1, x._v2, shift & 63),
					3 => ShiftLeftSmallZeroSafe(0, 0, 0, x._v1, shift & 63),
					_ => default
				}
				: shift > -256 ? x >> -shift
				: default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(UInt256 other)
			=> _v1 == other._v1 && _v2 == other._v2 && _v3 == other._v3 && _v4 == other._v4;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is UInt256 other && Equals(other);

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = (hashCode * 65599) + _v1.GetHashCode();
				hashCode = (hashCode * 65599) + _v2.GetHashCode();
				hashCode = (hashCode * 65599) + _v3.GetHashCode();
				hashCode = (hashCode * 65599) + _v4.GetHashCode();
				return hashCode;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(UInt256 a, UInt256 b)
			=> a.Equals(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(UInt256 a, UInt256 b)
			=> !a.Equals(b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(UInt256 a, UInt256 b)
			=> a.CompareTo(b) <= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(UInt256 a, UInt256 b)
			=> a.CompareTo(b) >= 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(UInt256 a, UInt256 b)
			=> a.CompareTo(b) < 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(UInt256 a, UInt256 b)
			=> a.CompareTo(b) > 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(UInt256 other)
		{
			UInt256 d = this - other;
			return d.IsZero ? 0
				: (d._v1 >> 63) != 0 ? 1
				: -1;
		}

		public int CompareTo(object? obj)
			=> obj is UInt256 other ? CompareTo(other) : 1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int PopCount()
			=> (int)(ulong.PopCount(_v1) + ulong.PopCount(_v2) + ulong.PopCount(_v3) + ulong.PopCount(_v4));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int LeadingZeroCount()
			=>    _v4 != 0 ? (int)ulong.LeadingZeroCount(_v4)
				: _v3 != 0 ? (int)ulong.LeadingZeroCount(_v3) + 64
				: _v2 != 0 ? (int)ulong.LeadingZeroCount(_v2) + 128
				: (int)ulong.LeadingZeroCount(_v1) + 192;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int TrailingZeroCount()
			=>    _v1 != 0 ? (int)ulong.TrailingZeroCount(_v1)
				: _v2 != 0 ? (int)ulong.TrailingZeroCount(_v2) + 64
				: _v3 != 0 ? (int)ulong.TrailingZeroCount(_v3) + 128
				: (int)ulong.TrailingZeroCount(_v4) + 192;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsBitSet(int index)
			=> ExtractBit(index) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsBitClear(int index)
			=> ExtractBit(index) == 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ExtractBit(int index)
		{
			ulong v = (index >> 6) switch
			{
				0 => _v1,
				1 => _v2,
				2 => _v3,
				3 => _v4,
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

			return (int)(v >> (index & 63)) & 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ExtractNibble(int index)
		{
			ulong v = (index >> 4) switch
			{
				0 => _v1,
				1 => _v2,
				2 => _v3,
				3 => _v4,
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

			return (int)(v >> ((index & 15) << 2)) & 0xF;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ExtractByte(int index)
		{
			ulong v = (index >> 3) switch
			{
				0 => _v1,
				1 => _v2,
				2 => _v3,
				3 => _v4,
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

			return (byte)(v >> ((index & 7) << 3));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ushort ExtractShort(int index)
		{
			ulong v = (index >> 2) switch
			{
				0 => _v1,
				1 => _v2,
				2 => _v3,
				3 => _v4,
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

			return (ushort)(v >> ((index & 3) << 4));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint ExtractInt(int index)
		{
			ulong v = (index >> 1) switch
			{
				0 => _v1,
				1 => _v2,
				2 => _v3,
				3 => _v4,
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

			return (uint)(v >> ((index & 1) << 5));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ulong ExtractLong(int index)
			=> index switch
			{
				0 => _v1,
				1 => _v2,
				2 => _v3,
				3 => _v4,
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256 SetBit(int index)
			=> (index >> 6) switch
			{
				0 => new UInt256(_v1 | (1UL << (index & 63)), _v2, _v3, _v4),
				1 => new UInt256(_v1, _v2 | (1UL << (index & 63)), _v3, _v4),
				2 => new UInt256(_v1, _v2, _v3 | (1UL << (index & 63)), _v4),
				3 => new UInt256(_v1, _v2, _v3, _v4 | (1UL << (index & 63))),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UInt256 ClearBit(int index)
			=> (index >> 6) switch
			{
				0 => new UInt256(_v1 & ~(1UL << (index & 63)), _v2, _v3, _v4),
				1 => new UInt256(_v1, _v2 & ~(1UL << (index & 63)), _v3, _v4),
				2 => new UInt256(_v1, _v2, _v3 & ~(1UL << (index & 63)), _v4),
				3 => new UInt256(_v1, _v2, _v3, _v4 & ~(1UL << (index & 63))),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};

		public UInt256 InsertBit(int index, int bit)
		{
			int shift = index & 63;
			ulong mask = 1UL << shift;

			return (index >> 6) switch
			{
				0 => new UInt256((_v1 & ~mask) | ((ulong)(bit & 1) << shift), _v2, _v3, _v4),
				1 => new UInt256(_v1, (_v2 & ~mask) | ((ulong)(bit & 1) << shift), _v3, _v4),
				2 => new UInt256(_v1, _v2, (_v3 & ~mask) | ((ulong)(bit & 1) << shift), _v4),
				3 => new UInt256(_v1, _v2, _v3, (_v4 & ~mask) | ((ulong)(bit & 1) << shift)),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};
		}

		public UInt256 InsertNibble(int index, int nibble)
		{
			int shift = (index & 15) << 2;
			ulong mask = 0xFUL << shift;

			return (index >> 4) switch
			{
				0 => new UInt256((_v1 & ~mask) | ((ulong)(nibble & 0xF) << shift), _v2, _v3, _v4),
				1 => new UInt256(_v1, (_v2 & ~mask) | ((ulong)(nibble & 0xF) << shift), _v3, _v4),
				2 => new UInt256(_v1, _v2, (_v3 & ~mask) | ((ulong)(nibble & 0xF) << shift), _v4),
				3 => new UInt256(_v1, _v2, _v3, (_v4 & ~mask) | ((ulong)(nibble & 0xF) << shift)),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};
		}

		public UInt256 InsertByte(int index, byte b)
		{
			int shift = (index & 7) << 3;
			ulong mask = 0xFFUL << shift;

			return (index >> 3) switch
			{
				0 => new UInt256((_v1 & ~mask) | ((ulong)b << shift), _v2, _v3, _v4),
				1 => new UInt256(_v1, (_v2 & ~mask) | ((ulong)b << shift), _v3, _v4),
				2 => new UInt256(_v1, _v2, (_v3 & ~mask) | ((ulong)b << shift), _v4),
				3 => new UInt256(_v1, _v2, _v3, (_v4 & ~mask) | ((ulong)b << shift)),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};
		}

		public UInt256 InsertShort(int index, ushort s)
		{
			int shift = (index & 3) << 4;
			ulong mask = 0xFFFFUL << shift;

			return (index >> 2) switch
			{
				0 => new UInt256((_v1 & ~mask) | ((ulong)s << shift), _v2, _v3, _v4),
				1 => new UInt256(_v1, (_v2 & ~mask) | ((ulong)s << shift), _v3, _v4),
				2 => new UInt256(_v1, _v2, (_v3 & ~mask) | ((ulong)s << shift), _v4),
				3 => new UInt256(_v1, _v2, _v3, (_v4 & ~mask) | ((ulong)s << shift)),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};
		}

		public UInt256 InsertInt(int index, uint s)
		{
			int shift = (index & 1) << 5;
			ulong mask = 0xFFFF_FFFFUL << shift;

			return (index >> 1) switch
			{
				0 => new UInt256((_v1 & ~mask) | ((ulong)s << shift), _v2, _v3, _v4),
				1 => new UInt256(_v1, (_v2 & ~mask) | ((ulong)s << shift), _v3, _v4),
				2 => new UInt256(_v1, _v2, (_v3 & ~mask) | ((ulong)s << shift), _v4),
				3 => new UInt256(_v1, _v2, _v3, (_v4 & ~mask) | ((ulong)s << shift)),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};
		}

		public UInt256 InsertLong(int index, ulong l)
		{
			return index switch
			{
				0 => new UInt256(l, _v2, _v3, _v4),
				1 => new UInt256(_v1, l, _v3, _v4),
				2 => new UInt256(_v1, _v2, l, _v4),
				3 => new UInt256(_v1, _v2, _v3, l),
				_ => throw new ArgumentOutOfRangeException(nameof(index)),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public override string ToString()
		{
			if (IsZero)
				return "0";

			Span<char> chars = stackalloc char[64];

			int dest = 0;
			int digits = ((256 - LeadingZeroCount()) + 3) >> 2;
			for (int i = digits - 1; i >= 0; i--)
			{
				int n = ExtractNibble(i);
				chars[dest++] = n < 10 ? (char)('0' + n) : (char)('A' + (n - 10));
			}

			return new string(chars.Slice(0, dest));
		}

		public static bool TryParse(ReadOnlySpan<char> text, out UInt256 value)
		{
			if (text.Length > 64)
			{
				value = default;
				return false;
			}

			ulong v1 = 0, v2 = 0, v3 = 0, v4 = 0;

			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];

				int n;
				if (ch >= '0' && ch <= '9') n = ch - '0';
				else if (ch >= 'a' && ch <= 'f') n = ch - 'a' + 10;
				else if (ch >= 'A' && ch <= 'F') n = ch - 'A' + 10;
				else
				{
					value = default;
					return false;
				}

				int index = text.Length - i - 1;
				switch (index >> 4)
				{
					case 0: v1 |= (ulong)n << ((index & 15) << 2); break;
					case 1: v2 |= (ulong)n << ((index & 15) << 2); break;
					case 2: v3 |= (ulong)n << ((index & 15) << 2); break;
					case 3: v4 |= (ulong)n << ((index & 15) << 2); break;
				}
			}

			value = new UInt256(v1, v2, v3, v4);
			return true;
		}
	}
}
