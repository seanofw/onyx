using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Onyx.Html
{
	/// <summary>
	/// A table of known HTML entities, designed for fast lookup and efficient
	/// entity encoding and decoding.
	/// </summary>
	public static class HtmlEntities
	{
		/// <summary>
		/// A mapping of known HTML entity names to their equivalent values.
		/// </summary>
		public static IReadOnlyDictionary<string, int> EntitiesToValues => _entitiesToValues;
		private static readonly Dictionary<string, int> _entitiesToValues;

		/// <summary>
		/// A mapping of known HTML entity values to their equivalent names.
		/// </summary>
		public static IReadOnlyDictionary<int, string> ValuesToEntities => _valuesToEntities;
		private static readonly Dictionary<int, string> _valuesToEntities;

		/// <summary>
		/// High-speed entity lookup; this avoids the cost of having to construct a string
		/// to match in the dictionary above.
		/// </summary>
		private static Dictionary<ulong, int> _entitiesToValuesInternal;

		/// <summary>
		/// A list of the known HTML entities, in numerical order.
		/// </summary>
		public static IReadOnlyList<(string, int)> Entities => _entities;

		/// <summary>
		/// A simple array where each byte indicates that the entity has a known value.
		/// This goes up to contain all 4-decimal-digit entity values, which in HTML 4
		/// includes all of them.
		/// </summary>
		private static readonly unsafe byte* _escapeBits;

		/// <summary>
		/// Construct the various entity lookup tables.
		/// </summary>
		unsafe static HtmlEntities()
		{
			Dictionary<string, int> entitiesToValues = new Dictionary<string, int>();
			Dictionary<int, string> valuesToEntities = new Dictionary<int, string>();
			Dictionary<ulong, int> entitiesToValuesInternal = new Dictionary<ulong, int>();

			// This is allocated once off the external heap, populated, and then never
			// touched again.
			byte* escapeBits = (byte*)Marshal.AllocHGlobal(16384);
			Unsafe.InitBlockUnaligned(escapeBits, 0, 16384);

			foreach ((string name, int value) in _entities)
			{
				entitiesToValues[name] = value;
				valuesToEntities[value] = name;
				entitiesToValuesInternal[MakeEntityKey(name)] = value;
				escapeBits[value] = 1;
			}

			_entitiesToValues = entitiesToValues;
			_valuesToEntities = valuesToEntities;
			_entitiesToValuesInternal = entitiesToValuesInternal;
			_escapeBits = escapeBits;
		}

		/// <summary>
		/// Escape the given text input using HTML entities.
		/// </summary>
		/// <param name="text">The text string to escape.</param>
		/// <param name="pureAscii">Whether the output should be pure ASCII (i.e., all chars less than 127).</param>
		/// <param name="controlCodes">Whether to escape control codes (values 0-31).</param>
		/// <returns>The same text, escaped.</returns>
		public static string Escape(ReadOnlySpan<char> text, bool pureAscii = false, bool controlCodes = false)
		{
			StringBuilder stringBuilder = new StringBuilder();
			EscapeTo(text, stringBuilder, pureAscii, controlCodes);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Escape the given text input using HTML entities.
		/// </summary>
		/// <param name="text">The text string to escape.</param>
		/// <param name="stringBuilder">A StringBuilder to which the escaped text will be written.</param>
		/// <param name="pureAscii">Whether the output should be pure ASCII (i.e., all chars less than 127).</param>
		/// <param name="controlCodes">Whether to escape control codes (values 0-31).</param>
		public static void EscapeTo(ReadOnlySpan<char> text, StringBuilder stringBuilder,
			bool pureAscii = false, bool controlCodes = false)
		{
			Span<char> numberBuffer = stackalloc char[16];

			unsafe
			{
				char[] flarb = new char[128];
				for (int i = 0; i < 128; i++)
					flarb[i] = (char)('0' + _escapeBits[i]);
				string flarb2 = new string(flarb);
			}

			int min = controlCodes ? 0 : 32;
			int max = pureAscii ? 127 : 0x110000;

			for (int i = 0; i < text.Length;)
			{
				// Append the next chunk of text that *doesn't* contain an escaped character.
				int start = i;
				char ch = '\0';
				while (i < text.Length && (ch = text[i]) < max && ch >= min && !IsKnownEntity(ch))
					i++;
				if (i > start)
					stringBuilder.Append(text.Slice(start, i - start));

				// If we ran out of input, we're done.
				if (i >= text.Length)
					break;

				// We have an escape character, so replace it.  We prefer named escapes
				// over numbers, but use numbers where necessary.
				if (_valuesToEntities.TryGetValue(ch, out string? name))
				{
					stringBuilder.Append('&');
					stringBuilder.Append(name);
					stringBuilder.Append(';');
				}
				else
				{
					stringBuilder.Append("&#");
					((int)ch).TryFormat(numberBuffer, out int charCount, default, CultureInfo.InvariantCulture);
					stringBuilder.Append(numberBuffer.Slice(0, charCount));
					stringBuilder.Append(';');
				}

				// Move past the escaped character.
				i++;
			}
		}

		/// <summary>
		/// Answer whether the given character code has a known HTML entity, faster
		/// than actually looking up that entity code.  This optimizes down to just
		/// a couple of machine instructions total, far faster than any other kind
		/// of lookup you can do (in exchange for storing a 16K memory buffer).
		/// </summary>
		/// <param name="ch">The character code to test.</param>
		/// <returns>True if the character has an entity, false if it does not.</returns>
		//[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static unsafe bool IsKnownEntity(char ch)
			=> ch < 16384 && _escapeBits[ch] != 0;

		/// <summary>
		/// Convert all HTML &entity; forms into their equivalent characters in the given
		/// string, unescaping anything that's escaped.  This follows HTML 5 rules and allows
		/// the trailing ';' to be omitted, and copies the '&' verbatim to the output for
		/// non-matching entities.
		/// </summary>
		/// <param name="text">The text to unescape.</param>
		/// <returns>The same text, with all escapes replaced.</returns>
		public static string Unescape(ReadOnlySpan<char> text)
		{
			StringBuilder stringBuilder = new StringBuilder();
			UnescapeTo(text, stringBuilder);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Convert all HTML &entity; forms into their equivalent characters in the given
		/// string, unescaping anything that's escaped.  This follows HTML 5 rules and allows
		/// the trailing ';' to be omitted, and copies the '&' verbatim to the output for
		/// non-matching entities.
		/// </summary>
		/// <param name="text">The text to unescape.</param>
		/// <param name="stringBuilder">The same text will be written here, with all escapes replaced.</returns>
		public static void UnescapeTo(ReadOnlySpan<char> text, StringBuilder stringBuilder)
		{
			for (int i = 0; i < text.Length;)
			{
				// Append the next chunk of text that *doesn't* contain an '&'.
				int start = i;
				while (i < text.Length && text[i] != '&')
					i++;
				if (i > start)
					stringBuilder.Append(text.Slice(start, i - start));

				// If we ran out of text, abort.
				if (i >= text.Length)
					break;

				start = i++;    // If this is a bad escape, we have to emit it verbatim, so track its start.

				long value = -1;
				if (i < text.Length)
				{
					if (text[i] == '#')
					{
						// Numeric entity:  Consume decimal digits.
						i++;

						char ch;
						while (i < text.Length && (ch = text[i]) >= '0' && ch <= '9')
							i++;
						if (i > (start + 2) && i < (start + 10))
						{
							value = long.Parse(text.Slice(start + 2, i - (start + 2)), CultureInfo.InvariantCulture);
							if (value < 0 || value > 0x110000)
								value = -1;     // Bad character.
						}
					}
					else
					{
						// Text entity:  Consume letters and digits.
						while (i < text.Length && char.IsLetterOrDigit(text[i]))
							i++;
						if (i > start + 1)
						{
							// Use the dictionary that *doesn't* require a string lookup,
							// because in some markup, we're going to hit this path a lot.
							ulong entityKey = MakeEntityKey(text.Slice(start + 1, i - (start + 1)));
							if (_entitiesToValuesInternal.TryGetValue(entityKey, out int entityValue))
								value = entityValue;
						}
					}
				}

				if (value >= 0)
				{
					// We have a valid entity escape, so consume any trailing ';' and
					// then emit it to the output.
					if (i < text.Length && text[i] == ';')
						i++;
					stringBuilder.Append((char)value);
				}
				else
				{
					// Invalid entity, so copy it to the output verbatim, just like a browser would.
					if (i > start)
						stringBuilder.Append(text.Slice(start, i - start));
				}
			}
		}

		/// <summary>
		/// Create an entity key, which is simply the character bytes packed into the bottom of
		/// a 64-bit integer.  Any character code outside printable ASCII is replaced with 0xFF,
		/// and 0x00 is used for fill.  We use this technique to avoid the GC pressure of
		/// allocating a temporary string every time an entity is discovered in the input.
		/// </summary>
		/// <param name="readOnlySpan">The span containing up to 8 characters to test.</param>
		/// <returns>A key representing that entity.  This will be all 0xFF bytes if the
		/// provided span is more than 8 characters (the longest entity length, for 'thetasym').</returns>
		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		private static unsafe ulong MakeEntityKey(ReadOnlySpan<char> readOnlySpan)
		{
			ulong result = 0;
			char ch;
			byte b;

			// Unrolled and pointers for maximum speed, with tricks to avoid
			// branches and data dependencies.
			fixed (char* chars = readOnlySpan)
			{
				switch (readOnlySpan.Length)
				{
					case 8:
						ch = chars[7];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b << (7 * 8);
						goto case 7;
					case 7:
						ch = chars[6];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b << (6 * 8);
						goto case 6;
					case 6:
						ch = chars[5];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b << (5 * 8);
						goto case 5;
					case 5:
						ch = chars[4];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b << (4 * 8);
						goto case 4;
					case 4:
						ch = chars[3];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b << (3 * 8);
						goto case 3;
					case 3:
						ch = chars[2];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b << (2 * 8);
						goto case 2;
					case 2:
						ch = chars[1];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b << (1 * 8);
						goto case 1;
					case 1:
						ch = chars[0];
						b = ((uint)(ch - 32) >= 96) ? (byte)0xFF : (byte)ch;
						result |= (ulong)b;
						break;
					case 0:
						break;
					default:
						return ulong.MaxValue;
				}

				return result;
			}
		}

		/// <summary>
		/// The actual entity table itself, as a simple array of tuples.
		/// </summary>
		private static (string, int)[] _entities =
		{
			("quot",     34),
			("amp",      38),
			("lt",       60),
			("gt",       62),

			("nbsp",     160),
			("iexcl",    161),
			("cent",     162),
			("pound",    163),
			("curren",   164),
			("yen",      165),
			("brvbar",   166),
			("sect",     167),
			("uml",      168),
			("copy",     169),
			("ordf",     170),
			("laquo",    171),
			("not",      172),
			("shy",      173),
			("reg",      174),
			("macr",     175),
			("deg",      176),
			("plusmn",   177),
			("sup2",     178),
			("sup3",     179),
			("acute",    180),
			("micro",    181),
			("para",     182),
			("middot",   183),
			("cedil",    184),
			("sup1",     185),
			("ordm",     186),
			("raquo",    187),
			("frac14",   188),
			("frac12",   189),
			("frac34",   190),
			("iquest",   191),
			("Agrave",   192),
			("Aacute",   193),
			("Acirc",    194),
			("Atilde",   195),
			("Auml",     196),
			("Aring",    197),
			("AElig",    198),
			("Ccedil",   199),
			("Egrave",   200),
			("Eacute",   201),
			("Ecirc",    202),
			("Euml",     203),
			("Igrave",   204),
			("Iacute",   205),
			("Icirc",    206),
			("Iuml",     207),
			("ETH",      208),
			("Ntilde",   209),
			("Ograve",   210),
			("Oacute",   211),
			("Ocirc",    212),
			("Otilde",   213),
			("Ouml",     214),
			("times",    215),
			("Oslash",   216),
			("Ugrave",   217),
			("Uacute",   218),
			("Ucirc",    219),
			("Uuml",     220),
			("Yacute",   221),
			("THORN",    222),
			("szlig",    223),
			("agrave",   224),
			("aacute",   225),
			("acirc",    226),
			("atilde",   227),
			("auml",     228),
			("aring",    229),
			("aelig",    230),
			("ccedil",   231),
			("egrave",   232),
			("eacute",   233),
			("ecirc",    234),
			("euml",     235),
			("igrave",   236),
			("iacute",   237),
			("icirc",    238),
			("iuml",     239),
			("eth",      240),
			("ntilde",   241),
			("ograve",   242),
			("oacute",   243),
			("ocirc",    244),
			("otilde",   245),
			("ouml",     246),
			("divide",   247),
			("oslash",   248),
			("ugrave",   249),
			("uacute",   250),
			("ucirc",    251),
			("uuml",     252),
			("yacute",   253),
			("thorn",    254),
			("yuml",     255),

			("OElig",    338),
			("oelig",    339),
			("Scaron",   352),
			("scaron",   353),
			("Yuml",     376),

			("fnof",     402),

			("circ",     710),
			("tilde",    732),

			("Alpha",    913),
			("Beta",     914),
			("Gamma",    915),
			("Delta",    916),
			("Epsilon",  917),
			("Zeta",     918),
			("Eta",      919),
			("Theta",    920),
			("Iota",     921),
			("Kappa",    922),
			("Lambda",   923),
			("Mu",       924),
			("Nu",       925),
			("Xi",       926),
			("Omicron",  927),
			("Pi",       928),
			("Rho",      929),
			("Sigma",    931),
			("Tau",      932),
			("Upsilon",  933),
			("Phi",      934),
			("Chi",      935),
			("Psi",      936),
			("Omega",    937),

			("alpha",    945),
			("beta",     946),
			("gamma",    947),
			("delta",    948),
			("epsilon",  949),
			("zeta",     950),
			("eta",      951),
			("theta",    952),
			("iota",     953),
			("kappa",    954),
			("lambda",   955),
			("mu",       956),
			("nu",       957),
			("xi",       958),
			("omicron",  959),
			("pi",       960),
			("rho",      961),
			("sigmaf",   962),
			("sigma",    963),
			("tau",      964),
			("upsilon",  965),
			("phi",      966),
			("chi",      967),
			("psi",      968),
			("omega",    969),
			("thetasym", 977),
			("upsih",    978),
			("piv",      982),

			("ensp",     8194),
			("emsp",     8195),
			("thinsp",   8201),
			("zwnj",     8204),
			("zwj",      8205),
			("lrm",      8206),
			("rlm",      8207),
			("ndash",    8211),
			("mdash",    8212),
			("lsquo",    8216),
			("rsquo",    8217),
			("sbquo",    8218),
			("ldquo",    8220),
			("rdquo",    8221),
			("bdquo",    8222),
			("dagger",   8224),
			("Dagger",   8225),
			("bull",     8226),
			("hellip",   8230),
			("permil",   8240),
			("prime",    8242),
			("Prime",    8243),
			("lsaquo",   8249),
			("rsaquo",   8250),
			("oline",    8254),
			("frasl",    8260),
			("euro",     8364),

			("weierp",   8472),
			("image",    8465),
			("real",     8476),
			("trade",    8482),
			("alefsym",  8501),

			("larr",     8592),
			("uarr",     8593),
			("rarr",     8594),
			("darr",     8595),
			("harr",     8596),
			("crarr",    8629),
			("lArr",     8656),
			("uArr",     8657),
			("rArr",     8658),
			("dArr",     8659),
			("hArr",     8660),

			("forall",   8704),
			("part",     8706),
			("exist",    8707),
			("empty",    8709),
			("nabla",    8711),
			("isin",     8712),
			("notin",    8713),
			("ni",       8715),
			("prod",     8719),
			("sum",      8721),
			("minus",    8722),
			("lowast",   8727),
			("radic",    8730),
			("prop",     8733),
			("infin",    8734),
			("ang",      8736),
			("and",      8743),
			("or",       8744),
			("cap",      8745),
			("cup",      8746),
			("int",      8747),
			("there4",   8756),
			("sim",      8764),
			("cong",     8773),
			("asymp",    8776),
			("ne",       8800),
			("equiv",    8801),
			("le",       8804),
			("ge",       8805),
			("sub",      8834),
			("sup",      8835),
			("nsub",     8836),
			("sube",     8838),
			("supe",     8839),
			("oplus",    8853),
			("otimes",   8855),
			("perp",     8869),
			("sdot",     8901),

			("lceil",    8968),
			("rceil",    8969),
			("lfloor",   8970),
			("rfloor",   8971),
			("lang",     9001),
			("rang",     9002),

			("loz",      9674),

			("spades",   9824),
			("clubs",    9827),
			("hearts",   9829),
			("diams",    9830),
		};
	}
}
