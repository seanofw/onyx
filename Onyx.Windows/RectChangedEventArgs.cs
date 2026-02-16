using Onyx.Types;

namespace Onyx.Windows
{
	public class RectChangedEventArgs : EventArgs
	{
		public Rect2i OldRect { get; }
		public Rect2i NewRect { get; }

		public RectChangedEventArgs(Rect2i oldRect, Rect2i newRect)
			: base()
		{
			OldRect = oldRect;
			NewRect = newRect;
		}

		public override string ToString()
			=> $"OldRect:{OldRect}, NewRect:{NewRect}";
	}
}
