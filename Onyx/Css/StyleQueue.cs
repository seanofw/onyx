using Onyx.Html.Dom;

namespace Onyx.Css
{
	/// <summary>
	/// A "queue" of elements that need to have their styles recomputed.  If an element
	/// needs to have its style recomputed, it is implicitly assumed that its descendants
	/// also need to have their styles recomputed as well.
	/// </summary>
	public class StyleQueue : ObjectQueue<Element>, IStyleQueue
	{
		/// <summary>
		/// Process the entire queue, restyling all elements in the queue, and their
		/// descendants, recursively.  This will run until the entire queue is empty.
		/// </summary>
		public void ProcessQueue()
		{
			Element? element;
			while ((element = TryDequeue()) != null)
			{
				element.GetComputedStyle();
				element.InvalidateChildComputedStyles();
			}
		}
	}
}
