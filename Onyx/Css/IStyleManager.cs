using Onyx.Css.Computed;
using Onyx.Css.Properties;
using Onyx.Css.Selectors;
using Onyx.Html.Dom;

namespace Onyx.Css
{
	/// <summary>
	/// A StyleManager knows which styles exist, and has fast lookup logic for their
	/// selectors, and provides tools for calculating computed styles, according to
	/// CSS rules.
	/// </summary>
	public interface IStyleManager
	{
		/// <summary>
		/// All of the stylesheets added to this StyleManager.
		/// </summary>
		IReadOnlyList<Stylesheet> Stylesheets { get; }

		/// <summary>
		/// This event is raised whenever the stylesheet collection changes.
		/// </summary>
		event EventHandler? StylesheetsChanged;

		/// <summary>
		/// All attribute names that have been used by at least one selector.
		/// </summary>
		IReadOnlyDictionary<string, int> AttributesUsedByStyles { get; }

		/// <summary>
		/// All class names that have been used by at least one selector.
		/// </summary>
		IReadOnlyDictionary<string, int> ClassnamesUsedByStyles { get; }

		/// <summary>
		/// Parse and add a new stylesheet.  This is just a convenience method around
		/// parsing it and then adding it.
		/// </summary>
		/// <param name="text">The text of the stylesheet to add.</param>
		/// <param name="filename">The filename (for error reporting).</param>
		/// <returns>The stylesheet that was added to the StyleManager.</returns>
		Stylesheet AddStylesheet(string text, string filename);

		/// <summary>
		/// Add a new stylesheet.
		/// </summary>
		/// <param name="stylesheet">The stylesheet to add to the collection
		/// of managed style rules.  This must not already have been added, or an
		/// exception will be thrown.</param>
		void AddStylesheet(Stylesheet stylesheet);

		/// <summary>
		/// Remove a stylesheet from the collection.
		/// </summary>
		/// <param name="stylesheet">The stylesheet to remove.</param>
		/// <returns>True if the stylesheet existed (and was therefore removed), false
		/// if it did not already exist.</returns>
		bool RemoveStylesheet(Stylesheet stylesheet);

		/// <summary>
		/// Look up styles for the given element.  
		/// </summary>
		/// <param name="element">The element to locate styles for.</param>
		/// <returns>*All* of the StylePropertySets that should be applied to
		/// this element.  Note that they are not ordered by specificity, and those
		/// that are overridden are still included, and shorthand properties are not
		/// expanded:  The returned collection is the "raw" style collection, with
		/// the correct specificity to apply for each property set.</returns>
		IReadOnlyCollection<StylePropertySetWithSpecificity> GetStyleRules(Element element);

		/// <summary>
		/// Compute the style for the given element, inheriting it (as necessary or as requested)
		/// from the given parent style.
		/// </summary>
		/// <param name="element">The element whose style is to be computed.</param>
		/// <param name="parentStyle">The parent style to inherit from.</param>
		/// <returns>The element's finished computed style.</returns>
		ComputedStyle ComputeStyle(Element element, ComputedStyle? parentStyle);
	}
}