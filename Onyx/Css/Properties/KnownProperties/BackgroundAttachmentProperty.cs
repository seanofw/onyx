using System.Collections.Immutable;
using Onyx.Css.Computed;
using Onyx.Css.Types;
using Onyx.Extensions;

namespace Onyx.Css.Properties.KnownProperties
{
	public sealed record class BackgroundAttachmentProperty : StyleProperty
	{
		public IReadOnlyList<BackgroundAttachment> Attachments
		{
			get => _attachments;
			init => _attachments = value is ImmutableArray<BackgroundAttachment> array ? array
				: value is null ? ImmutableArray<BackgroundAttachment>.Empty
				: value.ToImmutableArray();
		}
		private readonly ImmutableArray<BackgroundAttachment> _attachments = ImmutableArray<BackgroundAttachment>.Empty;

		public static BackgroundAttachmentProperty Default { get; } =
			new BackgroundAttachmentProperty { Kind = KnownPropertyKind.BackgroundAttachment };

		public BackgroundAttachmentProperty AddAttachment(BackgroundAttachment attachment)
			=> this with { Attachments = _attachments.Add(attachment) };

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundAttachment>(
				style.BackgroundLayers, Attachments.Count,
				(l, i) => l.WithAttachment(Attachments[i])));

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithBackgroundLayers(BackgroundProperty.UnifyBackground<BackgroundAttachment>(
				source.BackgroundLayers, Math.Max(dest.BackgroundLayers.Count, source.BackgroundLayers.Count),
				(l, i) => l.WithAttachment(i < source.BackgroundLayers.Count
					? source.BackgroundLayers[i].Attachment
					: default)));

		public override string ToString()
			=> string.Join(", ", Attachments.Select(r => r.ToString().Hyphenize()));
	}
}
