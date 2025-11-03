using System.Collections.Immutable;

namespace Onyx.Css
{
	public class Stylesheet
	{
		public IReadOnlyList<StyleRule> Rules => _rules;
		private ImmutableList<StyleRule> _rules;

		public Stylesheet(IEnumerable<StyleRule>? rules = null)
		{
			_rules = rules is ImmutableList<StyleRule> list ? list
				: rules is null ? ImmutableList<StyleRule>.Empty
				: rules.ToImmutableList(); 
		}

		public override string ToString()
			=> $"Stylesheet with {_rules.Count} rules";
	}
}
