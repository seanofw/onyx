namespace Onyx.Css.Types
{
	public enum Units : byte
	{
		None = 0,

		Percent,

		Em,
		Ex,

		Pixels,
		Centimeters,
		Millimeters,
		Inches,
		Points,
		Picas,

		Degrees,
		Radians,
		Grads,

		Milliseconds,
		Seconds,

		Hertz,
		Kilohertz,

		// Pseudo-units, used with computed styles.
		Pseudo,
	}
}
