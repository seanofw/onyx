using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace Onyx.Windows
{
	internal static partial class Win32
	{
		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr CreateWindowEx(uint dwExStyle,
			string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight,
			IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern void PostQuitMessage(int nExitCode);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern uint RegisterClassEx([In] ref WNDCLASSEX wndClass);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool TranslateMessage([In] ref MSG lpMsg);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
			int x, int y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool SetWindowText(IntPtr hWnd, string text);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr BeginPaint(IntPtr hwnd, ref PAINTSTRUCT lpPaint);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern bool DeleteObject(IntPtr ho);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint usage,
			out IntPtr ppvBits, IntPtr hSection, uint offset);

		[DllImport("gdi32.dll", SetLastError = true)]
		public static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy,
			IntPtr hdcSrc, int x1, int y1, uint rop);

		public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
	}
}
