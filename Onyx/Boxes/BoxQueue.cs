
namespace Onyx.Boxes
{
	/// <summary>
	/// A "queue" of boxes that need to have their layouts recomputed.
	/// </summary>
	public class BoxQueue : ObjectQueue<Box>, IBoxQueue
	{
		/// <summary>
		/// Process the entire queue, restyling all elements in the queue, and their
		/// descendants, recursively.  This will run until the entire queue is empty.
		/// </summary>
		public void ProcessQueue()
		{
			Box? box;
			while ((box = TryDequeue()) != null)
			{
			}
		}
	}
}
