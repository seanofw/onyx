using Onyx.Extensions;
using Onyx.Types;

namespace Onyx.Css.Types
{
	public sealed record class LinearGradient : GradientBase
	{
		public SideOrCorner SideOrCorner { get; init; }
		public Measure Angle { get; init; }
		public bool UsesTo { get; init; }

		/// <summary>
		/// The angle, converted to radians from whatever form it was in before,
		/// normalized to [0, 2pi).
		/// </summary>
		public double AngleInRadians => _angleInRadians ??= CalculateAngleInRadians();
		private double? _angleInRadians;

		/// <summary>
		/// The normalized direction vector, derived from the angle in radians.  This assumes
		/// that Y points DOWN like on a screen, not UP like traditional math, and assumes
		/// that the angle goes clockwise from 0 being UP (like CSS, not like traditional math).
		/// </summary>
		public Vector2d DirectionVector => _directionVector ??= CalculateDirectionVector(AngleInRadians);
		private Vector2d? _directionVector;

		/// <summary>
		/// A delegate to the color-stop calculator to avoid allocating it more than once.
		/// </summary>
		private readonly Func<double, IReadOnlyList<(Color32 Color, double Offset)>> _calculateColorStopsInternalFunc;

		private Cache<double, IReadOnlyList<(Color32 Color, double Offset)>>? _colorStopCache;

		public LinearGradient()
		{
			Kind = BackgroundKind.LinearGradient;
			_calculateColorStopsInternalFunc = CalculateColorStopsInternal;
		}

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (SideOrCorner != default)
			{
				pieces.Add((UsesTo ? "to " : string.Empty) + SideOrCorner.ToString().Hyphenize());
			}
			else if (Angle != default)
				pieces.Add(Angle.ToString());

			foreach (ColorStop colorStop in ColorStops)
				pieces.Add(colorStop.ToString());

			return string.Join(", ", pieces);
		}

		private double CalculateAngleInRadians()
		{
			double angle;
			if (SideOrCorner != default)
			{
				angle = SideOrCorner switch
				{
					SideOrCorner.Left => 270 * (Math.PI / 180),
					SideOrCorner.Right => 90 * (Math.PI / 180),
					SideOrCorner.Top => 0 * (Math.PI / 180),
					SideOrCorner.Bottom => 180 * (Math.PI / 180),
					SideOrCorner.TopLeft => 315 * (Math.PI / 180),
					SideOrCorner.BottomLeft => 225 * (Math.PI / 180),
					SideOrCorner.TopRight => 45 * (Math.PI / 180),
					SideOrCorner.BottomRight => 135 * (Math.PI / 180),
					_ => 0,
				};
			}
			else
			{
				angle = Angle.Units switch
				{
					Units.Degrees => Angle.Value * (Math.PI / 180),
					Units.Radians => Angle.Value,
					Units.Grads => Angle.Value * (Math.PI / 200),
					_ => 0,
				};
			}

			if (!UsesTo)
				angle += Math.PI;

			angle %= 2.0 * Math.PI;

			if (angle < 0)
				angle += 2.0 * Math.PI;

			return angle;
		}

		/// <summary>
		/// Get the start and end points for drawing a linear gradient using the common method where
		/// the gradient runs along the line between those two points, scaled to fit in the given
		/// pixel rectangle.
		/// </summary>
		/// <param name="rect">The pixel rectangle defining the drawing surface.</param>
		/// <returns>Two points that describe where to start and end the gradient within that rectangle.</returns>
		public (Vector2d Start, Vector2d End) GetStartAndEndPoints(Rect2d rect)
		{
			Vector2d direction = DirectionVector;
			Vector2d center = rect.Center;

			double min = double.PositiveInfinity;
			double max = double.NegativeInfinity;

			foreach (Vector2d corner in new[]
				{ rect.TopLeft, rect.TopRight, rect.BottomRight, rect.BottomLeft })
			{
				Vector2d relative = corner - center;
				double projection = relative.Dot(direction);

				min = Math.Min(min, projection);
				max = Math.Max(max, projection);
			}

			Vector2d start = center + direction * min;
			Vector2d end = center + direction * max;

			return (start, end);
		}

		private static Vector2d CalculateDirectionVector(double angle)
		{
			// Convert CSS angle to math angle.  Because CSS is weird, that's why.  And then
			// flip it upside-down because Y points downward because we're on a screen.
			double theta = angle - (Math.PI * 0.5);

			Vector2d direction = new Vector2d(Math.Cos(theta), Math.Sin(theta));
			return direction;
		}

		/// <summary>
		/// Given the length of the endpoints of a linear gradient (usually calculated by
		/// GetStartAndEnd()), return corrected, properly-structured color stops following CSS
		/// rules about color stop ordering and clamping, with each color stop offset in the
		/// range of [0, 1] and nondecreasing.  Where possible, this will return a
		/// cached copy of the computed color stops rather than recomputing it.  This assumes
		/// that the ColorStops collection by this point uses only measures that can be directly
		/// converted to Pixels, and Percents; things like Ems will throw an exception here.
		/// </summary>
		/// <param name="lineLength">The length of the gradient, in pixels.</param>
		/// <returns>The color stops, as an ordered immutable list.</returns>
		public IReadOnlyList<(Color32 Color, double Offset)> CalculateColorStops(double lineLength)
		{
			// Constrain the length precision for more-consistent cache lookup.  Three digits
			// is plenty precise to cover nearly every real-world use case.
			double cacheKey = Math.Round(Math.Max(lineLength, 0.001), 3);

			lock (_calculateColorStopsInternalFunc)
			{
				_colorStopCache ??= new Cache<double, IReadOnlyList<(Color32 Color, double Offset)>>(100);
				return _colorStopCache.GetOrAdd(cacheKey, _calculateColorStopsInternalFunc);
			}
		}

		/// <summary>
		/// This is a godawful mess, because the CSS spec requires it.
		/// </summary>
		/// <param name="lineLength">The length of the gradient, in pixels.</param>
		/// <returns>The color stops, as an ordered immutable list.</returns>
		private IReadOnlyList<(Color32 Color, double Offset)> CalculateColorStopsInternal(double lineLength)
		{
			IReadOnlyList<ColorStop> colorStops = ColorStops;
			int count = colorStops.Count;

			if (count == 0)
				return Array.Empty<(Color32, double)>();

			Span<Color32?> colors = count < 32 ? stackalloc Color32?[count] : new Color32?[count];
			Span<double> offsets = count < 32 ? stackalloc double[count] : new double[count];

			// Step 1: Collect raw values.
			for (int i = 0; i < count; i++)
			{
				ColorStop stop = colorStops[i];
				colors[i] = stop.Color;

				double offset = CalculateColorStopOffset(stop.Measure, lineLength);
				offsets[i] = offset; // may be NaN if unspecified
			}

			// Step 2: Handle leading unspecified stops.
			int firstSpecified = -1;
			for (int i = 0; i < count; i++)
			{
				if (!double.IsNaN(offsets[i]))
				{
					firstSpecified = i;
					break;
				}
			}

			if (firstSpecified == -1)
			{
				// No stops had positions, distribute all of them evenly.
				double ooCountM1 = count > 1 ? 1.0 / (count - 1) : 0.0;
				for (int i = 0; i < count; i++)
					offsets[i] = i * ooCountM1;
			}
			else
			{
				for (int i = 0; i < firstSpecified; i++)
					offsets[i] = offsets[firstSpecified];
			}

			// Step 3: Interpolate unspecified runs.
			int start = firstSpecified;
			while (start < count)
			{
				int end = start + 1;

				while (end < count && double.IsNaN(offsets[end]))
					end++;

				if (end >= count)
				{
					// Handle a trailing unspecified run.
					for (int i = start + 1; i < count; i++)
						offsets[i] = offsets[start];
					break;
				}

				double startOffset = offsets[start];
				double endOffset = offsets[end];

				int span = end - start;
				double ooSpan = span > 0 ? 1.0 / span : 0.0;

				for (int i = 1; i < span; i++)
					offsets[start + i] = startOffset + (endOffset - startOffset) * (i * ooSpan);

				start = end;
			}

			// Step 4: Enforce nondecreasing order (CSS spec requires clamping).
			double prevOffset = offsets[0];
			for (int i = 1; i < count; i++)
			{
				if (offsets[i] < prevOffset)
					offsets[i] = prevOffset;

				prevOffset = offsets[i];
			}

			// Step 5: Build the result.
			(Color32 Color, double Offset)[] result = new (Color32 Color, double Offset)[count];

			for (int i = 0; i < count; i++)
			{
				Color32? color = colors[i];
				double offset = offsets[i];

				if (color.HasValue)
				{
					result[i] = (color.Value, offset);
					continue;
				}

				// This entry is a hint.  Find the previous color stop.
				int prev = i - 1;
				while (prev >= 0 && !colors[prev].HasValue)
					prev--;

				// Find the next color stop.
				int next = i + 1;
				while (next < count && !colors[next].HasValue)
					next++;

				if (prev < 0 || next >= count)
					continue;	// Malformed but safe.

				Color32 a = colors[prev]!.Value;
				Color32 b = colors[next]!.Value;

				double aOffset = offsets[prev];
				double bOffset = offsets[next];

				double t = (offset - aOffset) / (bOffset - aOffset);

				Color32 mixed = a.Mix(b, t);

				result[i] = (mixed, offset);
			}

			return result;
		}

		private static double CalculateColorStopOffset(Measure measure, double ooLineLength)
			=> measure.Units switch
			{
				Units.None => double.NaN,
				Units.Pixels => measure.Value * ooLineLength,
				Units.Percent => measure.Value * (1.0 / 100.0),
				Units.Centimeters or Units.Millimeters or Units.Picas or Units.Points or Units.Inches
					=> measure.ConvertTo(Units.Pixels).Value * ooLineLength,
				_ => 0,
			};
	}
}
