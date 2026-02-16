
namespace Onyx.Windows
{
	public class DisposeEventArgs : EventArgs
	{
		public bool IsDisposing { get; }

		public DisposeEventArgs(bool isDisposing)
		{
			IsDisposing = isDisposing;
		}

		public override string ToString()
			=> $"IsDisposing:{IsDisposing}";
	}
}
