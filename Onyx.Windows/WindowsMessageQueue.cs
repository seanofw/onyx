
namespace Onyx.Windows
{
	public sealed class WindowsMessageQueue
	{
		[ThreadStatic]
		private static WindowsMessageQueue _instance = new WindowsMessageQueue();

		public static WindowsMessageQueue Instance => _instance;

		private WindowsMessageQueue()
		{
		}

		public static void Run()
		{
			Win32.MSG msg = default;

			while (Win32.GetMessage(out msg, 0, 0, 0))
			{
				Win32.TranslateMessage(ref msg);
				Win32.DispatchMessage(ref msg);
			}
		}

		public static void Quit()
		{
			Win32.PostQuitMessage(0);
		}
	}
}
