using System.Runtime.InteropServices;

namespace Onyx.Windows
{
	internal static partial class Win32
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct MSG
		{
			public IntPtr hwnd;
			public uint message;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WNDCLASSEX
		{
			public uint cbSize;
			public uint style;
			[MarshalAs(UnmanagedType.FunctionPtr)]
			public WndProc lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			public string? lpszMenuName;
			public string lpszClassName;
			public IntPtr hIconSm;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct CREATESTRUCT
		{
			public IntPtr lpCreateParams;
			public IntPtr hInstance;
			public IntPtr hMenu;
			public IntPtr hwndParent;
			public int cy;
			public int cx;
			public int y;
			public int x;
			public uint style;
			public IntPtr lpszName;
			public IntPtr lpszClass;
			public uint dwExStyle;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WINDOWPOS
		{
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public uint flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct PAINTSTRUCT
		{
			public IntPtr hdc;
			public bool fErase;
			public RECT rcPaint;
			public bool fRestore;
			public bool fIncUpdate;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] rgbReserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFOHEADER
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct BITMAPINFO
		{
			public BITMAPINFOHEADER bmiHeader;
			public uint bmiColors;
		}
	}
}
