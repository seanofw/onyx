using Onyx.Css.Properties;
using Onyx.Css.Selectors;

namespace Onyx.Css
{
	public readonly struct StylePropertySetWithSpecificity
	{
		public StylePropertySet StylePropertySet { get; }
		public Specificity Specificity { get; }

		public StylePropertySetWithSpecificity(StylePropertySet stylePropertySet, Specificity specificity)
		{
			StylePropertySet = stylePropertySet;
			Specificity = specificity;
		}
	}
}
