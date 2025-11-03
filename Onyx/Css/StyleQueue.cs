using System.Collections;
using Onyx.Html.Dom;

namespace Onyx.Css
{
	/// <summary>
	/// A "queue" of elements that need to have their styles recomputed.  If an element
	/// needs to have its style recomputed, it is implicitly assumed that its descendants
	/// also need to have their styles recomputed as well.
	/// </summary>
	public class StyleQueue : IReadOnlyCollection<Element>, IStyleQueue
	{
		/// <summary>
		/// The actual "queue" itself, represented as a set to provide random access
		/// to add and remove elements.
		/// </summary>
		private readonly HashSet<Element> _set = new HashSet<Element>();

		/// <summary>
		/// The number of subtree roots currently in the queue.
		/// </summary>
		public int Count => _set.Count;

		/// <summary>
		/// Enqueue the given element for restyling.  It is assumed that the element
		/// already has an invalid computed style.
		/// </summary>
		/// <param name="element">The element to restyle.</param>
		public void Enqueue(Element element)
		{
			_set.Add(element);
		}

		/// <summary>
		/// Remove the given element from the queue.  Even though it was previously
		/// enqueued, it is now assumed to have already been given a valid style.
		/// </summary>
		/// <param name="element">The element that no longer needs to be restyled.</param>
		public void Remove(Element element)
		{
			_set.Remove(element);
		}

		/// <summary>
		/// Remove the next element from the queue so it can be restyled.
		/// </summary>
		/// <returns>The next element to restyle.</returns>
		public Element? TryDequeue()
		{
			if (_set.Count == 0)
				return null;

			Element result = _set.First();
			_set.Remove(result);

			return result;
		}

		/// <summary>
		/// Enumerate the subtree roots currently in the queue.
		/// </summary>
		public IEnumerator<Element> GetEnumerator()
			=> _set.GetEnumerator();

		/// <summary>
		/// Enumerate the subtree roots currently in the queue.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
			=> _set.GetEnumerator();

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
