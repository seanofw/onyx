using Onyx.Css.Properties;
using Onyx.Css.Selectors;

namespace Onyx.Css
{
	public class StyleRule
	{
		public CompoundSelector Selector { get; }
		public StylePropertySet Properties { get; }

		public StyleRule(CompoundSelector selector, StylePropertySet? properties = null)
		{
			Selector = selector;
			Properties = properties ?? StylePropertySet.Empty;
		}

		public override string ToString()
			=> $"{Selector} {{ {Properties} }}";
	}
}
