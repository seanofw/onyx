using Onyx.Css.Types.Media;

namespace Onyx.Css
{
	/// <summary>
	/// A Node tree root capable of supporting styling.
	/// </summary>
	internal interface IStyleRoot
	{
		/// <summary>
		/// The style manager that knows what styles exist.
		/// </summary>
		IStyleManager StyleManager { get; }

		/// <summary>
		/// The style queue that knows what nodes need to be styled.
		/// </summary>
		IStyleQueue StyleQueue { get; }

		/// <summary>
		/// A media query context that can answer questions for styles that use @media rules.
		/// </summary>
		MediaQueryContext MediaQueryContext { get; }
	}
}
