using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class ClipProperty : StyleProperty
	{
		public CssRect? Rect { get; init; }
		public bool Auto { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> Auto ? style.WithClip(null) : style.WithClip(Rect.GetValueOrDefault());

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithClip(source.Clip);

		public override string ToString()
			=> Auto ? "auto"
				: Rect.HasValue ? Rect.Value.ToString()
				: "<invalid>";
	}
}
