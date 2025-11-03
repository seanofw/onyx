namespace Onyx.Css.Types
{
	public abstract record class BackgroundLayerBase
	{
		public BackgroundKind Kind { get; protected init; }
	}
}
