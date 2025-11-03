using Onyx.Css.Computed;
using Onyx.Css.Parsing;

namespace Onyx.Css.Properties
{
    public sealed record class UnknownProperty : StyleProperty
    {
		public string Name { get; init; } = string.Empty;
        public IReadOnlyList<CssToken> Tokens { get; init; } = null!;

		public override ComputedStyle Apply(ComputedStyle style)
			=> style;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest;

		public override string ToString()
			=> string.Join(" ", Tokens.Select(t => t.ToString()));
	}
}
