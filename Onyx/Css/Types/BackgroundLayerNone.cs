namespace Onyx.Css.Types
{
	public record class BackgroundLayerNone : BackgroundLayerBase
	{
		public static BackgroundLayerNone Instance { get; } = new BackgroundLayerNone();

		public BackgroundLayerNone()
		{
			Kind = BackgroundKind.None;
		}
	}
}
