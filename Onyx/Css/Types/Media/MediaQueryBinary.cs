namespace Onyx.Css.Types.Media
{
	public abstract class MediaQueryBinary : MediaQuery
	{
		public MediaQuery Left { get; }
		public MediaQuery Right { get; }

		public MediaQueryBinary(MediaQueryKind kind, MediaQuery left, MediaQuery right)
			: base(kind)
		{
			Left = left;
			Right = right;
		}
	}
}
