using Onyx.Css.Properties;
using Onyx.Css.Selectors;

namespace Onyx.Css
{
	public readonly struct StylePropertyBag
	{
		public StylePropertySet StylePropertySet { get; }
		public Specificity Specificity { get; }

		public StylePropertyBag(StylePropertySet stylePropertySet, Specificity specificity)
		{
			StylePropertySet = stylePropertySet;
			Specificity = specificity;
		}
	}
}
