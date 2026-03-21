namespace Onyx.Skia
{
	public abstract class SkiaDisposable : IDisposable
	{
		~SkiaDisposable()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
		}
	}
}
