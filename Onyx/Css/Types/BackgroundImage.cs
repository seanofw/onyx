using Onyx.Extensions;

namespace Onyx.Css.Types
{
	public sealed record class BackgroundImage : BackgroundLayerBase
	{
		public string Url { get; init; } = string.Empty;

		public BackgroundImage()
		{
			Kind = BackgroundKind.Image;
		}

		public override string ToString()
			=> "url(\"" + Url.AddCSlashes() + "\")";
	}
}
