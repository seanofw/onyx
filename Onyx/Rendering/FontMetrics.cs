using Onyx.Types;

namespace Onyx.Rendering
{
	/// <summary>
	/// Metric data for a loaded font at a specific size. All values are in CSS pixels.
	/// Constant for the lifetime of the font; obtained from IFont.FontMetrics.
	/// </summary>
	public class FontMetrics
	{
		/// <summary>The recommended line spacing from baseline to baseline. Always non-negative.</summary>
		public double LineHeight { get; }

		/// <summary>Distance from the baseline to the top of the tallest glyph. Always non-negative.</summary>
		public double Ascent { get; }

		/// <summary>Distance from the baseline to the bottom of the lowest descender. Always non-negative.</summary>
		public double Descent { get; }

		/// <summary>Recommended stroke width for underline and strikethrough decoration lines. Always non-negative.</summary>
		public double UnderlineThickness { get; }

		/// <summary>Signed distance from the baseline to the center of the underline stroke.</summary>
		public double UnderlinePosition { get; }

		/// <summary>Signed distance from the baseline to the center of the strikethrough stroke.</summary>
		public double StrikethroughPosition { get; }

		/// <summary>Signed distance from the baseline to the center of the overline stroke.</summary>
		public double OverlinePosition { get; }

		/// <summary>The measured size of the 'M' glyph, used as the CSS em unit at this font size.</summary>
		public Size2d Em { get; }

		/// <summary>The measured size of the 'N' glyph, used as the CSS en unit at this font size.</summary>
		public Size2d En { get; }

		/// <summary>The measured size of the 'x' glyph, used as the CSS ex unit at this font size.</summary>
		public Size2d Ex { get; }

		/// <summary>The measured size of a space.</summary>
		public double Space { get; }

		/// <summary>
		/// Construct a new FontMetrics. Typically called by the backend inside IRenderables.CreateFont().
		/// </summary>
		/// <param name="lineHeight">The recommended line spacing from baseline to baseline. Always non-negative.</param>
		/// <param name="ascent">Distance from the baseline to the top of the tallest glyph. Always non-negative.</param>
		/// <param name="descent">Distance from the baseline to the bottom of the lowest descender. Always non-negative.</param>
		/// <param name="underlineThickness">Recommended stroke width for underline and strikethrough decoration lines. Always non-negative.</param>
		/// <param name="underlinePosition">Signed distance from the baseline to the center of the underline stroke.</param>
		/// <param name="strikethroughPosition">Signed distance from the baseline to the center of the strikethrough stroke.</param>
		/// <param name="overlinePosition">Signed distance from the baseline to the center of the overline stroke.</param>
		/// <param name="em">The measured size of the 'M' glyph, used as the CSS em unit at this font size.</param>
		/// <param name="en">The measured size of the 'N' glyph, used as the CSS en unit at this font size.</param>
		/// <param name="ex">The measured size of the 'x' glyph, used as the CSS ex unit at this font size.</param>
		/// <param name="space">The measured size of a space.</param>
		public FontMetrics(
			double lineHeight, double ascent, double descent,
			double underlineThickness, double underlinePosition, double strikethroughPosition, double overlinePosition,
			Size2d em, Size2d en, Size2d ex, double space)
		{
			LineHeight = lineHeight;
			Ascent = ascent;
			Descent = descent;

			UnderlineThickness = underlineThickness;
			UnderlinePosition = underlinePosition;
			StrikethroughPosition = strikethroughPosition;
			OverlinePosition = overlinePosition;

			Em = em;
			En = en;
			Ex = ex;
			Space = space;
		}
	}
}
