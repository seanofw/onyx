using Onyx.Css.Properties;
using Onyx.Css.Selectors;
using Onyx.Css.Types.Media;

namespace Onyx.Css
{
	public class StyleRule
	{
		public MediaQuery? MediaQuery { get; }
		public CompoundSelector Selector { get; }
		public StylePropertySet Properties { get; }

		public StyleRule(MediaQuery? mediaQuery, CompoundSelector selector, StylePropertySet? properties = null)
		{
			MediaQuery = mediaQuery;
			Selector = selector;
			Properties = properties ?? StylePropertySet.Empty;
		}

		public override string ToString()
			=> MediaQuery != null
				? $"@media {MediaQuery} {{ {Selector} {{ {Properties} }} }}"
				: $"{Selector} {{ {Properties} }}";
	}
}
