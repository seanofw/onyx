namespace Onyx.Css.Types.Media
{
	public abstract class MediaQueryUnary : MediaQuery
	{
		public MediaQuery Child { get; }

		public MediaQueryUnary(MediaQueryKind kind, MediaQuery child)
			: base(kind, child.UsesDimensions, child.HasErrors)
		{
			Child = child;
		}
	}
}
