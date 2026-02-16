using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Onyx.Css.Types;
using Onyx.Html.Dom;
using Onyx.Rendering;
using Onyx.Types;
using Onyx.Windows.Rendering;
using SkiaSharp;

namespace Onyx.Windows
{
	/// <summary>
	/// A Window, which wraps the Win32 operations for displaying and managing a window,
	/// and proxies Win32 events to and from a single Document that the Window contains.
	/// </summary>
	public class Window : IDisposable
	{
		/// <summary>
		/// The Win32 handle for the window.  Read-only.
		/// </summary>
		public IntPtr Handle { get; private set; }

		private GCHandle _gcHandle;

		/// <summary>
		/// The document to render inside the window.
		/// </summary>
		public Document? Document
		{
			get => _document;
			set
			{
				if (_document != value)
				{
					_document = value;
					OnDocumentChanged();
				}
			}
		}
		private Document? _document;

		/// <summary>
		/// The color to render/erase if there is no document or if it doesn't
		/// provide an actual background of its own.
		/// </summary>
		public Color32 DefaultBackgroundColor
		{
			get => _defaultBackgroundColor;
			set
			{
				if (_defaultBackgroundColor != value)
				{
					_defaultBackgroundColor = value;
					Win32.RECT rect = new Win32.RECT
					{
						Left = 0, Top = 0, Right = ClientRect.Width, Bottom = ClientRect.Height
					};
					Win32.InvalidateRect(Handle, ref rect, true);
				}
			}
		}
		private Color32 _defaultBackgroundColor = Color32.White;

		/// <summary>
		/// The title to display on top of the window.
		/// </summary>
		public string? Title
		{
			get => _title;
			set
			{
				if (_title != value)
				{
					_title = value;
					Win32.SetWindowText(Handle, value ?? string.Empty);
					if (Marshal.GetLastWin32Error() != 0)
						throw new Win32Exception();
					OnTitleChanged();
				}
			}
		}
		private string? _title;

		/// <summary>
		/// The current window rectangle.  Can be set as a way to move or resize the window.
		/// </summary>
		public Rect2i Rect
		{
			get => _rect;
			set
			{
				if (_rect != value)
				{
					_rect = value;
					Win32.SetWindowPos(Handle, IntPtr.Zero, value.X, value.Y, value.Width, value.Height,
						Win32.SWP_NOACTIVATE);
					if (Marshal.GetLastWin32Error() != 0)
						throw new Win32Exception();
				}
			}
		}
		private Rect2i _rect;

		/// <summary>
		/// The rectangle of the client area (non-titlebar, non-border area).
		/// </summary>
		public Rect2i ClientRect => _clientRect;
		private Rect2i _clientRect;

		/// <summary>
		/// Whether to show a titlebar on this window.
		/// </summary>
		public bool ShowTitlebar
		{
			get => _showTitlebar;
			set
			{
				if (_showTitlebar != value)
				{
					_showTitlebar = value;

					uint style = Win32.GetWindowLong(Handle, Win32.GWL_STYLE);
					if (Marshal.GetLastWin32Error() != 0)
						throw new Win32Exception();

					uint newStyle = value
						? style | Win32.WS_CAPTION
						: style & ~Win32.WS_CAPTION;

					Win32.SetWindowLong(Handle, Win32.GWL_STYLE, newStyle);
					if (Marshal.GetLastWin32Error() != 0)
						throw new Win32Exception();

					OnShowTitlebarChanged();
				}
			}
		}
		private bool _showTitlebar = true;

		public bool CanMaximize { get; set; } = true;

		public bool CanMinimize { get; set; } = true;

		public bool CanResize { get; set; } = true;

		public bool HasBorder { get; set; } = true;

		public bool AlwaysOnTop { get; set; }

		public bool IsMaximized { get; set; }

		public bool IsMinimized { get; set; }

		public bool IsVisible { get; }

		/// <summary>
		/// The minimum allowable size of this window.
		/// </summary>
		public Size2i MinSize { get; set; } = new Size2i(0, 0);

		/// <summary>
		/// The maximum allowable size of this window.
		/// </summary>
		public Size2i MaxSize { get; set; } = new Size2i(int.MaxValue, int.MaxValue);

		/// <summary>
		/// The surface on which all content rendering takes place.
		/// </summary>
		private SKSurface? _surface;
		private Size2i _surfaceSize = new Size2i(-1, -1);
		private IntPtr _memoryDc = IntPtr.Zero;
		private IntPtr _dib = IntPtr.Zero;
		private IntPtr _oldBitmap = IntPtr.Zero;
		private SkiaRenderer? _renderer;

		#region Events

		public event EventHandler? DocumentChanged;
		public event EventHandler? TitleChanged;
		public event EventHandler? ShowTitlebarChanged;
		public event EventHandler<RectChangingEventArgs>? RectChanging;
		public event EventHandler<RectChangedEventArgs>? RectChanged;
		public event EventHandler<RectChangedEventArgs>? Moved;
		public event EventHandler<RectChangedEventArgs>? Sized;
		public event EventHandler<CancelEventArgs>? CloseClicked;
		public event EventHandler<DisposeEventArgs>? Disposing;
		public event EventHandler<DisposeEventArgs>? Disposed;

		#endregion

		[ThreadStatic]
		private static readonly ConcurrentDictionary<IntPtr, Window> _windowLookup =
			new ConcurrentDictionary<IntPtr, Window>();

		private static uint _classAtom = uint.MaxValue;
		private static object _classAtomLock = new object();

		[ThreadStatic]
		private static int _windowCount;

		private const string ClassName = "OnyxWindow";

		public Window(Document? document = null,
			string? title = null,
			Vector2i? point = null,
			Size2i? size = null,
			Rect2i? rect = null,
			Size2i? minSize = null,
			Size2i? maxSize = null,
			bool showTitlebar = true,
			bool canMaximize = true,
			bool canMinimize = true,
			bool canResize = true,
			bool hasBorder = true,
			bool alwaysOnTop = true,
			bool isMaximized = false,
			bool isMinimized = false)
		{
			Rect2i r;
			if (rect.HasValue)
				r = rect.Value;
			else if (point.HasValue && size.HasValue)
				r = new Rect2i(point.Value, size.Value);
			else if (point.HasValue)
				r = new Rect2i(point.Value, new Size2i(Win32.CW_USEDEFAULT, Win32.CW_USEDEFAULT));
			else if (size.HasValue)
				r = new Rect2i(new Vector2i(Win32.CW_USEDEFAULT, Win32.CW_USEDEFAULT), size.Value);
			else
				r = new Rect2i(new Vector2i(Win32.CW_USEDEFAULT, Win32.CW_USEDEFAULT),
					new Size2i(Win32.CW_USEDEFAULT, Win32.CW_USEDEFAULT));

			_title = title;
			_document = document;
			_rect = r;
			_showTitlebar = showTitlebar;

			MinSize = minSize ?? MinSize;
			MaxSize = maxSize ?? MaxSize;

			//_canMaximize = canMaximize;
			//_canMinimize = canMinimize;
			//_canResize = canResize;
			//_alwaysOnTop = alwaysOnTop;
			//_isMaximized = isMaximized;
			//_isMinimized = isMinimized;

			uint atom = GetWindowClassAtom();

			_gcHandle = GCHandle.Alloc(this);

			Handle = Win32.CreateWindowEx(0, ClassName, title ?? string.Empty, Win32.WS_OVERLAPPEDWINDOW,
				r.X, r.Y, r.Width, r.Height, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
				GCHandle.ToIntPtr(_gcHandle));
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();

			_windowLookup[Handle] = this;
		}

		~Window()
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
			OnDisposing(isDisposing);

			if (Handle != IntPtr.Zero)
			{
				Win32.DestroyWindow(Handle);
				if (Marshal.GetLastWin32Error() != 0)
					throw new Win32Exception();

				Handle = IntPtr.Zero;
			}

			OnDisposed(isDisposing);
		}

		public void Show(bool activate = true)
		{
			Win32.ShowWindow(Handle, activate ? Win32.SW_SHOW : Win32.SW_SHOWNA);
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
		}

		public void Hide()
		{
			Win32.ShowWindow(Handle, Win32.SW_HIDE);
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
		}

		#region Event methods

		protected virtual void OnDocumentChanged()
			=> DocumentChanged?.Invoke(this, EventArgs.Empty);

		protected virtual void OnTitleChanged()
			=> TitleChanged?.Invoke(this, EventArgs.Empty);

		protected virtual void OnShowTitlebarChanged()
			=> ShowTitlebarChanged?.Invoke(this, EventArgs.Empty);

		protected virtual void OnRectChanged(Rect2i oldRect, Rect2i newRect)
			=> RectChanged?.Invoke(this, new RectChangedEventArgs(oldRect, newRect));

		protected virtual Rect2i? OnRectChanging(Rect2i oldRect, Rect2i newRect)
		{
			Rect2i? constrainedRect = ConstrainRect(oldRect, newRect);
			newRect = constrainedRect ?? newRect;

			RectChangingEventArgs eventArgs = new RectChangingEventArgs(oldRect, newRect);
			RectChanging?.Invoke(this, eventArgs);
			return eventArgs.Cancel ? null : newRect;
		}

		protected virtual void OnMoved(Rect2i oldRect, Rect2i newRect)
			=> Moved?.Invoke(this, new RectChangedEventArgs(oldRect, newRect));

		protected virtual void OnSized(Rect2i oldRect, Rect2i newRect)
			=> Sized?.Invoke(this, new RectChangedEventArgs(oldRect, newRect));

		protected virtual void OnCloseClicked()
		{
			CancelEventArgs cancelEventArgs = new CancelEventArgs();
			CloseClicked?.Invoke(this, cancelEventArgs);

			if (!cancelEventArgs.Cancel)
				Dispose(true);
		}

		protected virtual void OnDisposing(bool isDisposing)
			=> Disposing?.Invoke(this, new DisposeEventArgs(isDisposing));

		protected virtual void OnDisposed(bool isDisposing)
			=> Disposed?.Invoke(this, new DisposeEventArgs(isDisposing));

		#endregion

		/// <summary>
		/// Given an old rect and a new rect, constrain the new rect to the defined
		/// MinSize/MaxSize of the window.
		/// </summary>
		/// <param name="oldRect">The old rectangle before the move.</param>
		/// <param name="newRect">The new rectangle after the move.</param>
		/// <returns>A revised rectangle, if the constraints were violated and a
		/// different rectangle must be used; or null if the provided rectangle is
		/// acceptable as-is.</returns>
		protected virtual Rect2i? ConstrainRect(Rect2i oldRect, Rect2i newRect)
			=> Constrain2D(oldRect, newRect, MinSize, MaxSize);

		/// <summary>
		/// Given an old rect and a new rect, constrain the new rect to the provided
		/// minSize/maxSize using the standard size-constraint algorithm.
		/// </summary>
		/// <param name="oldRect">The old rectangle before the move.</param>
		/// <param name="newRect">The new rectangle after the move.</param>
		/// <returns>A revised rectangle, if the constraints were violated and a
		/// different rectangle must be used; or null if the provided rectangle is
		/// acceptable as-is.</returns>
		public static Rect2i? Constrain2D(Rect2i oldRect, Rect2i newRect, Size2i minSize, Size2i maxSize)
		{
			// First check if the new rect is valid within the constraints.  If so,
			// we have nothing to do.
			if (newRect.Width >= minSize.Width
				&& newRect.Height >= minSize.Height
				&& newRect.Width <= maxSize.Width
				&& newRect.Height <= maxSize.Height)
				return null;

			// We violated a size constraint, so we have to correct for it.

			// Decide which edge(s) have moved.  This will help us decide what the new
			// rect should be.
			bool leftMoved = oldRect.X != newRect.X;
			bool rightMoved = oldRect.X + oldRect.Width != newRect.X + newRect.Width;
			bool topMoved = oldRect.Y != newRect.Y;
			bool bottomMoved = oldRect.Y + oldRect.Height != newRect.Y + newRect.Height;

			if (newRect.Width < minSize.Width || newRect.Width > maxSize.Width)
			{
				// Invalid width, so fix it.
				(int left, int right) = Constrain1D(
					oldRect.X, oldRect.X + oldRect.Width,
					newRect.X, newRect.X + newRect.Width,
					minSize.Width, maxSize.Width);
				newRect = new Rect2i(left, newRect.Y, right - left, newRect.Height);
			}

			if (newRect.Height < minSize.Height || newRect.Height > maxSize.Height)
			{
				// Invalid height, so fix it.
				(int top, int bottom) = Constrain1D(
					oldRect.Y, oldRect.Y + oldRect.Height,
					newRect.Y, newRect.Y + newRect.Height,
					minSize.Height, maxSize.Height);
				newRect = new Rect2i(newRect.X, top, newRect.Width, bottom - top);
			}

			return newRect;
		}

		/// <summary>
		/// Given an old 1D span and a new 1D span, constrain the new span to the provided
		/// minSize/maxSize using the standard size-constraint algorithm.
		/// </summary>
		/// <param name="oldRect">The old 1D span before the move.</param>
		/// <param name="newRect">The new 1D span after the move.</param>
		/// <returns>A valid 1D span that matches the constraints.  If the constraints are
		/// invalid and cannot be met, the minimum size constraint will be preferred over
		/// the maximum size constraint.</returns>
		public static (int Start, int End) Constrain1D(
			int oldStart, int oldEnd, int newStart, int newEnd, int minSize, int maxSize)
		{
			if (newStart != oldStart)
			{
				// Dragging the start with the end fixed in place, so adjust the start if necessary.
				int desiredSize = oldEnd - newStart;
				int clampedSize = Math.Max(Math.Min(desiredSize, maxSize), minSize);

				int start = oldEnd - clampedSize;
				return (start, oldEnd);
			}
			else if (newEnd != oldEnd)
			{
				// Dragging the end with the start fixed in place, so adjust the end if necessary.
				int desiredSize = newEnd - oldStart;
				int clampedSize = Math.Max(Math.Min(desiredSize, maxSize), minSize);

				int end = oldStart + clampedSize;
				return (oldStart, end);
			}
			else {
				// If both edges moved, then preserve the position as best as possible while
				// still constraining to the correct size.
				int desiredSize = newEnd - newStart;
				int clampedSize = Math.Max(Math.Min(desiredSize, maxSize), minSize);

				int start = (newStart + newEnd - clampedSize) / 2;
				int end = start + clampedSize;
				return (start, end);
			}
		}

		private static uint GetWindowClassAtom()
		{
			if (_classAtom == uint.MaxValue)
			{
				lock (_classAtomLock)
				{
					if (_classAtom == uint.MaxValue)
					{
						Win32.WNDCLASSEX wndClass = default;

						wndClass.cbSize = (uint)Marshal.SizeOf(wndClass);
						wndClass.style = 0;
						wndClass.lpfnWndProc = WindowProc;
						wndClass.cbClsExtra = 0;
						wndClass.cbWndExtra = Marshal.SizeOf<IntPtr>();
						wndClass.hInstance = Marshal.GetHINSTANCE(typeof(Window).Module);
						wndClass.hIcon = IntPtr.Zero;
						wndClass.hCursor = IntPtr.Zero;
						wndClass.hbrBackground = IntPtr.Zero;
						wndClass.lpszMenuName = null;
						wndClass.lpszClassName = ClassName;
						wndClass.hIconSm = IntPtr.Zero;

						uint atom = Win32.RegisterClassEx(ref wndClass);
						if (Marshal.GetLastWin32Error() != 0)
							throw new Win32Exception();

						_classAtom = atom;
					}
				}
			}

			return _classAtom;
		}

		private static IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			IntPtr gcHandleValue = Win32.GetWindowLongPtr(hWnd, 0);
			Window? window = gcHandleValue != IntPtr.Zero
				? GCHandle.FromIntPtr(gcHandleValue).Target as Window
				: null;

			switch (msg)
			{
				case Win32.WM_NCCREATE:
					unsafe
					{
						Win32.CREATESTRUCT* lpCreateStruct = (Win32.CREATESTRUCT*)lParam;
						Win32.SetWindowLongPtr(hWnd, 0, lpCreateStruct->lpCreateParams);
						Interlocked.Increment(ref _windowCount);
					}
					break;

				case Win32.WM_NCDESTROY:
					if (window != null)
					{
						window.FreeSurface();
						window.FreeGdiBackBuffer();

						window._gcHandle.Free();
						window._gcHandle = default;
						Win32.SetWindowLongPtr(hWnd, 0, IntPtr.Zero);

						if (Interlocked.Decrement(ref _windowCount) == 0)
						{
							WindowsMessageQueue.Quit();
						}
					}
					break;

				case Win32.WM_WINDOWPOSCHANGING:
					if (window != null)
					{
						unsafe
						{
							Win32.WINDOWPOS* lpWindowPos = (Win32.WINDOWPOS*)lParam;
							Rect2i oldRect = window._rect;
							Rect2i newRect = new Rect2i(lpWindowPos->x, lpWindowPos->y, lpWindowPos->cx, lpWindowPos->cy);
							if (oldRect != newRect)
							{
								Rect2i? modifiedRect;
								if (!(modifiedRect = window.OnRectChanging(oldRect, newRect)).HasValue)
								{
									lpWindowPos->flags |= Win32.SWP_NOACTIVATE
										| Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOREPOSITION
										| Win32.SWP_NOOWNERZORDER | Win32.SWP_NOZORDER;
								}
								else
								{
									lpWindowPos->x = modifiedRect.Value.X;
									lpWindowPos->y = modifiedRect.Value.Y;
									lpWindowPos->cx = modifiedRect.Value.Width;
									lpWindowPos->cy = modifiedRect.Value.Height;
								}
							}
						}
					}
					return 0;

				case Win32.WM_WINDOWPOSCHANGED:
					if (window != null)
					{
						unsafe
						{
							Win32.WINDOWPOS* lpWindowPos = (Win32.WINDOWPOS*)lParam;
							Rect2i oldRect = window._rect;
							Rect2i newRect = new Rect2i(lpWindowPos->x, lpWindowPos->y, lpWindowPos->cx, lpWindowPos->cy);
							if (oldRect != newRect)
							{
								window._rect = newRect;
								Win32.RECT clientRect;
								Win32.GetClientRect(hWnd, out clientRect);
								window._clientRect = new Rect2i(clientRect.Left, clientRect.Top,
									clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);
								window.OnRectChanged(oldRect, newRect);
							}
							if (oldRect.Point != newRect.Point)
								window.OnMoved(oldRect, newRect);
							if (oldRect.Size != newRect.Size)
								window.OnSized(oldRect, newRect);
						}
					}
					return 0;

				case Win32.WM_PAINT:
					Win32.PAINTSTRUCT paintStruct = default;
					IntPtr hdc = Win32.BeginPaint(hWnd, ref paintStruct);

					if (window != null)
					{
						window.Render(hdc, new Rect2i(paintStruct.rcPaint.Left, paintStruct.rcPaint.Top,
							paintStruct.rcPaint.Right - paintStruct.rcPaint.Left,
							paintStruct.rcPaint.Bottom - paintStruct.rcPaint.Top));
					}

					Win32.EndPaint(hWnd, ref paintStruct);
					return 0;

				case Win32.WM_CLOSE:
					window?.OnCloseClicked();
					return 0;
			}

			return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
		}

		#region Rendering logic

		private void Render(IntPtr hdc, Rect2i paintRect)
		{
			SkiaRenderer skiaRenderer = GetRenderer();
			IRenderables renderables = skiaRenderer;
			IRenderer renderer = skiaRenderer;

			//---- TODO: Render any dirty parts of Document that intersect paintRect.

			renderer.Clear(DefaultBackgroundColor);

			IBrush redBrush = renderables.CreateSolidBrush(Color32.Red);
			IBrush blueBrush = renderables.CreateSolidBrush(Color32.Blue);

			renderer.FillRect(new Rect2d(10, 10, 200, 100), redBrush);
			renderer.FillRect(new Rect2d(100, 100, 200, 100), blueBrush);

			//---- End Document rendering.

			Win32.BitBlt(hdc, paintRect.X, paintRect.Y, paintRect.Width, paintRect.Height,
				_memoryDc, paintRect.X, paintRect.Y, Win32.SRCCOPY);
		}

		private SkiaRenderer GetRenderer()
		{
			if (_surface == null || _surfaceSize != Rect.Size)
			{
				FreeSurface();
				FreeGdiBackBuffer();
				IntPtr pixelBuffer = AllocGdiBackBuffer(Rect.Size);
				AllocSurface(Rect.Size, pixelBuffer);

				_renderer = new SkiaRenderer(_surface!.Canvas);
			}

			return _renderer!;
		}

		private void AllocSurface(Size2i size, IntPtr pixelBuffer)
		{
			_surfaceSize = size;

			_surface = SKSurface.Create(
				new SKImageInfo(size.Width, size.Height, SKColorType.Bgra8888, SKAlphaType.Opaque),
				pixelBuffer, size.Width * 4);
		}

		private void FreeSurface()
		{
			if (_surface != null)
			{
				_surface.Dispose();
				_surface = null;
			}
		}

		private void FreeGdiBackBuffer()
		{
			if (_memoryDc != IntPtr.Zero)
			{
				if (_oldBitmap != IntPtr.Zero)
				{
					Win32.SelectObject(_memoryDc, _oldBitmap);
					_oldBitmap = IntPtr.Zero;
				}

				Win32.DeleteDC(_memoryDc);
				_memoryDc = IntPtr.Zero;
			}

			if (_dib != IntPtr.Zero)
			{
				Win32.DeleteObject(_dib);
				_dib = IntPtr.Zero;
			}
		}

		private IntPtr AllocGdiBackBuffer(Size2i size)
		{
			Win32.BITMAPINFO bitmapInfo = new Win32.BITMAPINFO();
			bitmapInfo.bmiHeader.biSize = (uint)Marshal.SizeOf<Win32.BITMAPINFOHEADER>();
			bitmapInfo.bmiHeader.biWidth = size.Width;
			bitmapInfo.bmiHeader.biHeight = -size.Height;
			bitmapInfo.bmiHeader.biPlanes = 1;
			bitmapInfo.bmiHeader.biBitCount = 32;
			bitmapInfo.bmiHeader.biCompression = Win32.BI_RGB;

			IntPtr pixelBuffer;
			_dib = Win32.CreateDIBSection(IntPtr.Zero, ref bitmapInfo, Win32.DIB_RGB_COLORS,
				out pixelBuffer, IntPtr.Zero, 0);
			if (_dib == IntPtr.Zero)
				throw new Win32Exception();

			_memoryDc = Win32.CreateCompatibleDC(IntPtr.Zero);
			if (_memoryDc == IntPtr.Zero)
				throw new Win32Exception();

			_oldBitmap = Win32.SelectObject(_memoryDc, _dib);
			if (_oldBitmap == IntPtr.Zero)
				throw new Win32Exception();

			return pixelBuffer;
		}

		#endregion
	}
}
