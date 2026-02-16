namespace Onyx.Boxes
{
	public interface IBoxQueue
	{
		void Enqueue(Box box);
		bool Remove(Box box);
		Box? TryDequeue();
		void ProcessQueue();
	}
}
