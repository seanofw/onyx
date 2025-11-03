using Onyx.Html.Dom;

namespace Onyx.Css
{
	/// <summary>
	/// A queue of elements that need to be restyled.  An element being included
	/// implicitly means all of its descendants are included as well, since styles
	/// may inherit.
	/// </summary>
	public interface IStyleQueue
	{
		/// <summary>
		/// Enqueue the given element for restyling.  It is assumed that the element
		/// already has an invalid computed style.
		/// </summary>
		/// <param name="element">The element to restyle.</param>
		void Enqueue(Element element);

		/// <summary>
		/// Remove the given element from the queue.  Even though it was previously
		/// enqueued, it is now assumed to have already been given a valid style.
		/// </summary>
		/// <param name="element">The element that no longer needs to be restyled.</param>
		void Remove(Element element);

		/// <summary>
		/// Remove the next element from the queue so it can be restyled.
		/// </summary>
		/// <returns>The next element to restyle.</returns>
		Element? TryDequeue();

		/// <summary>
		/// Process the entire queue, restyling all elements in the queue, and their
		/// descendants, recursively.  This will run until the entire queue is empty.
		/// </summary>
		void ProcessQueue();
	}
}