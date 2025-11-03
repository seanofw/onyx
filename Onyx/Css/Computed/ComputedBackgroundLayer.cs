using System.Runtime.CompilerServices;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedBackgroundLayer
	{
		public readonly BackgroundAttachment Attachment;
		public readonly BackgroundOrigin Origin;
		public readonly BackgroundRepeat Repeat;
		public readonly BackgroundPosition Position;
		public readonly BackgroundSize Size;

		public readonly BackgroundLayerBase? Layer;

		public static ComputedBackgroundLayer Default { get; }
			= new ComputedBackgroundLayer(BackgroundAttachment.Scroll, BackgroundOrigin.BorderBox,
				BackgroundRepeat.Repeat, BackgroundPosition.Default, BackgroundSize.Default, null);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedBackgroundLayer(BackgroundAttachment attachment,
			BackgroundOrigin origin, BackgroundRepeat repeat, BackgroundPosition position,
			BackgroundSize size, BackgroundLayerBase? layer)
		{
			Attachment = attachment;
			Origin = origin;
			Repeat = repeat;
			Position = position;
			Size = size;
			Layer = layer;
		}

		public ComputedBackgroundLayer WithAttachment(BackgroundAttachment attachment)
			=> new ComputedBackgroundLayer(attachment, Origin, Repeat, Position, Size, Layer);
		public ComputedBackgroundLayer WithOrigin(BackgroundOrigin origin)
			=> new ComputedBackgroundLayer(Attachment, origin, Repeat, Position, Size, Layer);
		public ComputedBackgroundLayer WithRepeat(BackgroundRepeat repeat)
			=> new ComputedBackgroundLayer(Attachment, Origin, repeat, Position, Size, Layer);
		public ComputedBackgroundLayer WithPosition(BackgroundPosition position)
			=> new ComputedBackgroundLayer(Attachment, Origin, Repeat, position, Size, Layer);
		public ComputedBackgroundLayer WithSize(BackgroundSize size)
			=> new ComputedBackgroundLayer(Attachment, Origin, Repeat, Position, size, Layer);
		public ComputedBackgroundLayer WithLayer(BackgroundLayerBase? layer)
			=> new ComputedBackgroundLayer(Attachment, Origin, Repeat, Position, Size, layer);
	}
}
