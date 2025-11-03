using Onyx.Css.Computed;
using Onyx.Css.Types;

namespace Onyx.Css.Properties.KnownProperties
{
	public abstract record class WidthPropertyBase : StyleProperty
	{
		public Width Width { get; init; }

		protected Measure Measure => Width.Auto ? PseudoMeasures.Auto : Width.Offset;

		public override string ToString()
			=> Width.ToString();
	}

	public sealed record class TopProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithTop(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithTop(source.Top);
	}
	public sealed record class RightProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithRight(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithRight(source.Right);
	}
	public sealed record class BottomProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithBottom(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithBottom(source.Bottom);
	}
	public sealed record class LeftProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithLeft(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithLeft(source.Left);
	}

	public sealed record class HeightProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithHeight(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithHeight(source.Height);
	}
	public sealed record class WidthProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithWidth(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithWidth(source.Width);
	}

	public sealed record class MarginTopProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithMarginTop(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithMarginTop(source.MarginTop);
	}
	public sealed record class MarginRightProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithMarginRight(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithMarginRight(source.MarginRight);
	}
	public sealed record class MarginBottomProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithMarginBottom(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithMarginBottom(source.MarginBottom);
	}
	public sealed record class MarginLeftProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithMarginLeft(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithMarginLeft(source.MarginLeft);
	}

	public sealed record class PaddingTopProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithPaddingTop(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithPaddingTop(source.PaddingTop);
	}
	public sealed record class PaddingRightProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithPaddingRight(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithPaddingRight(source.PaddingRight);
	}
	public sealed record class PaddingBottomProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithPaddingBottom(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithPaddingBottom(source.PaddingBottom);
	}
	public sealed record class PaddingLeftProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithPaddingLeft(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithPaddingLeft(source.PaddingLeft);
	}

	public sealed record class TextIndentProperty : WidthPropertyBase
	{
		public override ComputedStyle Apply(ComputedStyle style) => style.WithTextIndent(Measure);
		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source) => dest.WithTextIndent(source.TextIndent);
	}
}
