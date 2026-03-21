namespace Onyx.Rendering
{
	/// <summary>
	/// Describes the geometric style of a stroked line: its dash pattern, endpoint caps,
	/// corner joins, and miter limit. Immutable; use the predefined static instances for
	/// common patterns, or construct a custom style from explicit parameters.
	/// </summary>
	public class LineStyle : IEquatable<LineStyle>
	{
		#region Properties

		/// <summary>
		/// The alternating on/off segment lengths that define the dash pattern, in CSS
		/// pixels. Even-indexed elements are drawn; odd-indexed elements are gaps. Empty
		/// for a solid line.
		/// </summary>
		public ReadOnlyMemory<float> Segments { get; }

		/// <summary>True if this is a solid (unbroken) line with no dash pattern.</summary>
		public bool IsSolid
			=> Segments.Length == 0;

		/// <summary>How the endpoints of an open stroked path are rendered.</summary>
		public LineCap Cap { get; }

		/// <summary>How corners are rendered where two line segments meet.</summary>
		public LineJoin Join { get; }

		/// <summary>
		/// The ratio of miter length to line thickness at which a miter join falls back
		/// to a bevel join. Only meaningful when <see cref="Join"/> is <see cref="LineJoin.Miter"/>.
		/// The CSS default is 4.0.
		/// </summary>
		public float MiterLimit { get; }

		#endregion

		#region Static instances

		/// <summary>A solid, unbroken line.</summary>
		public static LineStyle Solid       { get; } = new LineStyle([]);

		/// <summary>Tightly-spaced dots: 1px on, 1px off.</summary>
		public static LineStyle TightDotted { get; } = new LineStyle([1, 1]);

		/// <summary>Standard dots: 1px on, 2px off.</summary>
		public static LineStyle Dotted      { get; } = new LineStyle([1, 2]);

		/// <summary>Widely-spaced dots: 1px on, 4px off.</summary>
		public static LineStyle WideDotted  { get; } = new LineStyle([1, 4]);

		/// <summary>Tightly-spaced dashes: 4px on, 2px off.</summary>
		public static LineStyle TightDashed { get; } = new LineStyle([4, 2]);

		/// <summary>Standard dashes: 4px on, 4px off.</summary>
		public static LineStyle Dashed      { get; } = new LineStyle([4, 4]);

		/// <summary>Widely-spaced dashes: 4px on, 8px off.</summary>
		public static LineStyle WideDashed  { get; } = new LineStyle([4, 8]);

		/// <summary>Alternating dash and dot: 4px on, 2px off, 1px on, 2px off.</summary>
		public static LineStyle DashDot     { get; } = new LineStyle([4, 2, 1, 2]);

		/// <summary>Alternating dash and two dots: 4px on, 2px off, 1px on, 2px off, 1px on, 2px off.</summary>
		public static LineStyle DashDotDot  { get; } = new LineStyle([4, 2, 1, 2, 1, 2]);

		/// <summary>Alternating two dashes and a dot: 4px on, 2px off, 4px on, 2px off, 1px on, 2px off.</summary>
		public static LineStyle DashDashDot  { get; } = new LineStyle([4, 2, 4, 2, 1, 2]);

		#endregion

		#region Construction

		/// <summary>
		/// Construct a custom line style from a dash pattern, cap style, join style, and
		/// miter limit. An empty segment span produces a solid line.
		/// </summary>
		/// <param name="segments">
		/// Alternating on/off segment lengths in CSS pixels. Must contain an even number
		/// of elements. Each element must be positive and is rounded to three decimal places.
		/// </param>
		/// <param name="cap">How the endpoints of an open stroked path are rendered. Defaults to <see cref="LineCap.Flat"/>.</param>
		/// <param name="join">How corners are rendered where two line segments meet. Defaults to <see cref="LineJoin.Miter"/>.</param>
		/// <param name="miterLimit">
		/// The ratio of miter length to line thickness at which a miter join falls back to
		/// a bevel join. Only meaningful when <paramref name="join"/> is <see cref="LineJoin.Miter"/>.
		/// Defaults to 4.0 (the CSS default).
		/// </param>
		public LineStyle(ReadOnlySpan<float> segments,
			LineCap cap = LineCap.Flat, LineJoin join = LineJoin.Miter, float miterLimit = 4.0f)
		{
			Cap = cap;
			Join = join;
			MiterLimit = miterLimit;

			if ((segments.Length & 1) != 0)
				throw new ArgumentException("Line styles must be constructed with an even number of segments.");

			if (segments.Length == 0)
			{
				Segments = Array.Empty<float>();
				return;
			}

			float[] newSegments = new float[segments.Length];
			for (int i = 0; i < segments.Length; i++)
			{
				float segment = MathF.Round(segments[i], 3);
				if (segment <= 0)
					throw new ArgumentException("No line-style segment can be of length zero.");
				newSegments[i] = segment;
			}

			Segments = newSegments;
		}

		/// <summary>
		/// Construct a custom line style from a dash pattern, cap style, join style, and
		/// miter limit. An empty segment span produces a solid line.
		/// </summary>
		/// <param name="segments">
		/// Alternating on/off segment lengths in CSS pixels. Must contain an even number
		/// of elements. Each element must be positive and is rounded to three decimal places.
		/// </param>
		/// <param name="cap">How the endpoints of an open stroked path are rendered. Defaults to <see cref="LineCap.Flat"/>.</param>
		/// <param name="join">How corners are rendered where two line segments meet. Defaults to <see cref="LineJoin.Miter"/>.</param>
		/// <param name="miterLimit">
		/// The ratio of miter length to line thickness at which a miter join falls back to
		/// a bevel join. Only meaningful when <paramref name="join"/> is <see cref="LineJoin.Miter"/>.
		/// Defaults to 4.0 (the CSS default).
		/// </param>
		public LineStyle(float[] segments,
			LineCap cap = LineCap.Flat, LineJoin join = LineJoin.Miter, float miterLimit = 4.0f)
			: this(segments.AsSpan(), cap, join, miterLimit)
		{
		}

		/// <summary>
		/// Construct a custom line style from a dash pattern, cap style, join style, and
		/// miter limit. An empty segment span produces a solid line.
		/// </summary>
		/// <param name="segments">
		/// Alternating on/off segment lengths in CSS pixels. Must contain an even number
		/// of elements. Each element must be positive and is rounded to three decimal places.
		/// </param>
		/// <param name="cap">How the endpoints of an open stroked path are rendered. Defaults to <see cref="LineCap.Flat"/>.</param>
		/// <param name="join">How corners are rendered where two line segments meet. Defaults to <see cref="LineJoin.Miter"/>.</param>
		/// <param name="miterLimit">
		/// The ratio of miter length to line thickness at which a miter join falls back to
		/// a bevel join. Only meaningful when <paramref name="join"/> is <see cref="LineJoin.Miter"/>.
		/// Defaults to 4.0 (the CSS default).
		/// </param>
		public LineStyle(IEnumerable<float> segments,
			LineCap cap = LineCap.Flat, LineJoin join = LineJoin.Miter, float miterLimit = 4.0f)
			: this(segments.ToArray(), cap, join, miterLimit)
		{
		}

		/// <summary>
		/// Internal constructor, for use by the With*() methods.  Does not validate its inputs.
		/// </summary>
		private LineStyle(ReadOnlyMemory<float> segments,
			LineCap cap = LineCap.Flat, LineJoin join = LineJoin.Miter, float miterLimit = 4.0f, bool internalOnly = false)
		{
			Cap = cap;
			Join = join;
			MiterLimit = miterLimit;
			Segments = segments;
		}

		#endregion

		#region With*() methods

		/// <summary>Copy this LineStyle, replacing Segments.</summary>
		public LineStyle WithSegments(ReadOnlySpan<float> segments)
			=> new LineStyle(segments, Cap, Join, MiterLimit);
		/// <summary>Copy this LineStyle, replacing Segments.</summary>
		public LineStyle WithSegments(IEnumerable<float> segments)
			=> new LineStyle(segments, Cap, Join, MiterLimit);
		/// <summary>Copy this LineStyle, replacing Cap.</summary>
		public LineStyle WithCap(LineCap cap)
			=> new LineStyle(Segments, cap, Join, MiterLimit, internalOnly: true);
		/// <summary>Copy this LineStyle, replacing Join.</summary>
		public LineStyle WithJoin(LineJoin join)
			=> new LineStyle(Segments, Cap, join, MiterLimit, internalOnly: true);
		/// <summary>Copy this LineStyle, replacing MiterLimit.</summary>
		public LineStyle WithMiterLimit(float miterLimit)
			=> new LineStyle(Segments, Cap, Join, miterLimit, internalOnly: true);

		#endregion

		#region Equality and GetHashCode()

		public bool Equals(LineStyle? other)
			=> other is not null
				&& Cap == other.Cap
				&& Join == other.Join
				&& MiterLimit == other.MiterLimit
				&& Segments.Span.SequenceEqual(other.Segments.Span);

		public override bool Equals(object? obj)
			=> obj is LineStyle other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = (hashCode * 65599) + (int)Cap;
				hashCode = (hashCode * 65599) + (int)Join;
				hashCode = (hashCode * 65599) + MiterLimit.GetHashCode();
				ReadOnlySpan<float> segments = Segments.Span;
				for (int i = 0; i < segments.Length; i++)
					hashCode = (hashCode * 65599) + segments[i].GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(LineStyle? left, LineStyle? right)
			=> ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);

		public static bool operator !=(LineStyle? left, LineStyle? right)
			=> ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : !left.Equals(right);

		#endregion

		public override string ToString()
			=> IsSolid ? $"Solid, Cap:{Cap}, Join:{Join}, MiterLimit:{MiterLimit}"
				: $"DashPattern:{Segments}, Cap:{Cap}, Join:{Join}, MiterLimit:{MiterLimit}";
	}
}
