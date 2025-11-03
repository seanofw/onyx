using Onyx.Css.Computed;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class ListStyleImageProperty : StyleProperty
    {
        public string? Uri { get; init; }
        public bool None { get; init; }

		public static ListStyleImageProperty Default { get; } = new ListStyleImageProperty();

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithListStyleUri(None ? null : Uri);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithListStyleUri(source.ListStyleUri);

		public override string ToString()
			=> None ? "none" : "uri(\"" + Uri?.AddCSlashes() + "\")";
	}
}
