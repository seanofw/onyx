using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class BackgroundProperty : StyleProperty
    {
		public BackgroundColorProperty? BackgroundColor { get; init; }
		public BackgroundImageProperty? BackgroundImage { get; init; }
		public BackgroundRepeatProperty? BackgroundRepeat { get; init; }
		public BackgroundAttachmentProperty? BackgroundAttachment { get; init; }
		public BackgroundPositionProperty? BackgroundPosition { get; init; }
		public BackgroundOriginProperty? BackgroundOrigin { get; init; }
		public BackgroundSizeProperty? BackgroundSize { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> throw ShorthandException;

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> throw ShorthandException;

		internal static IEnumerable<ComputedBackgroundLayer>? UnifyBackground<T>(
			IReadOnlyList<ComputedBackgroundLayer> backgroundLayers,
			int numProperties, Func<ComputedBackgroundLayer, int, ComputedBackgroundLayer> applicator)
		{
			List<ComputedBackgroundLayer> layers = new List<ComputedBackgroundLayer>(backgroundLayers);
			while (layers.Count < numProperties)
				layers.Add(ComputedBackgroundLayer.Default);

			for (int i = 0; i < numProperties; i++)
				layers[i] = applicator(layers[i], i);

			return layers;
		}

		public override string ToString()
		{
			List<string> pieces = new List<string>();

			if (BackgroundColor != null)
				pieces.Add(BackgroundColor.ToString());
			if (BackgroundImage != null)
				pieces.Add(BackgroundImage.ToString());
			if (BackgroundRepeat != null)
				pieces.Add(BackgroundRepeat.ToString());
			if (BackgroundAttachment != null)
				pieces.Add(BackgroundAttachment.ToString());
			if (BackgroundPosition != null)
				pieces.Add(BackgroundPosition.ToString());
			if (BackgroundOrigin != null)
				pieces.Add(BackgroundOrigin.ToString());
			if (BackgroundSize != null)
				pieces.Add(BackgroundSize.ToString());

			return string.Join(" ", pieces);
		}

		protected override IEnumerable<StyleProperty> DecomposeInternal()
		{
			if (BackgroundColor != null)
				yield return BackgroundColor;
			if (BackgroundImage != null)
				yield return BackgroundImage;
			if (BackgroundRepeat != null)
				yield return BackgroundRepeat;
			if (BackgroundAttachment != null)
				yield return BackgroundAttachment;
			if (BackgroundPosition != null)
				yield return BackgroundPosition;
			if (BackgroundOrigin != null)
				yield return BackgroundOrigin;
			if (BackgroundSize != null)
				yield return BackgroundSize;
		}

		public override bool IsDecomposable => true;
	}
}
