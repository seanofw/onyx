using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class ListStyleProperty : StyleProperty
	{
		public ListStyleTypeProperty? Type { get; init; }
		public ListStylePositionProperty? Position { get; init; }
		public ListStyleImageProperty? Image { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (Type != null)
				pieces.Add(Type.ToString());
			if (Position != null)
				pieces.Add(Position.ToString());
			if (Image != null)
				pieces.Add(Image.ToString());

			return string.Join(" ", pieces);
		}

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (Type != null)
				yield return Type;
			if (Position != null)
				yield return Position;
			if (Image != null)
				yield return Image;
		}

		public override bool IsDecomposable => true;
	}
}
