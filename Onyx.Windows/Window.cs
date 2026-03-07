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
		#region Properties

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
		/// Whether this window has a titlebar at its top.  You can set this property to
		/// change the window's style.
		/// </summary>
		public bool HasTitlebar
		{
			get => _hasTitlebar;
			set
			{
				if (_hasTitlebar != value)
				{
					_hasTitlebar = value;

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
		private bool _hasTitlebar;

		/// <summary>
		/// Whether this window can be maximized by the user.  You can set this property to
		/// change the window's style.
		/// </summary>
		public bool CanMaximize
		{
			get => _canMaximize;
			set
			{
				if (_canMaximize != value)
				{
					_canMaximize = value;
					UpdateWindowStyle();
				}
			}
		}
		private bool _canMaximize;

		/// <summary>
		/// Whether this window can be minimized by the user.  You can set this property to
		/// change the window's style.
		/// </summary>
		public bool CanMinimize
		{
			get => _canMinimize;
			set
			{
				if (_canMinimize != value)
				{
					_canMinimize = value;
					UpdateWindowStyle();
				}
			}
		}
		private bool _canMinimize;

		/// <summary>
		/// Whether this window can be resized by the user.  You can set this property to
		/// change the window's style.
		/// </summary>
		public bool CanResize
		{
			get => _canResize;
			set
			{
				if (_canResize != value)
				{
					_canResize = value;
					UpdateWindowStyle();
				}
			}
		}
		private bool _canResize;

		/// <summary>
		/// Whether this window has a border.  You can set this property to
		/// change the window's style.
		/// </summary>
		public bool HasBorder
		{
			get => _hasBorder;
			set
			{
				if (_hasBorder != value)
				{
					_hasBorder = value;
					UpdateWindowStyle();
				}
			}
		}
		private bool _hasBorder;

		/// <summary>
		/// Whether this window is a "tool window" with a smaller titlebar.  You can set
		/// this property to change the window's style.
		/// </summary>
		public bool IsToolWindow
		{
			get => _isToolWindow;
			set
			{
				if (_isToolWindow != value)
				{
					_isToolWindow = value;
					UpdateWindowStyle();
				}
			}
		}
		private bool _isToolWindow;

		/// <summary>
		/// Whether this window is on top of other windows.  You can set this property to
		/// change the window's style.
		/// </summary>
		public bool AlwaysOnTop
		{
			get => _alwaysOnTop;
			set
			{
				if (_alwaysOnTop != value)
				{
					_alwaysOnTop = value;
					UpdateWindowStyle();
				}
			}
		}
		private bool _alwaysOnTop;

		/// <summary>
		/// Whether this window is maximized.  Read-only status information.
		/// </summary>
		public bool IsMaximized { get; private set; }

		/// <summary>
		/// Whether this window is minimized.  Read-only status information.
		/// </summary>
		public bool IsMinimized { get; private set; }

		/// <summary>
		/// Whether this window is visible.  You can set this to change the visible state.
		/// </summary>
		public bool IsVisible
		{
			get => _isVisible;
			set
			{
				if (_isVisible != value)
				{
					if (value) Show();
					else Hide();
				}
			}
		}
		private bool _isVisible;

		/// <summary>
		/// Whether this window has the keyboard focus.
		/// </summary>
		public bool IsFocused => _isFocused;
		private bool _isFocused;

		/// <summary>
		/// Whether this is a true child of another parent window.
		/// </summary>
		public bool IsChildWindow => _isChildWindow;
		private readonly bool _isChildWindow;

		/// <summary>
		/// The minimum allowable size of this window.
		/// </summary>
		public Size2i MinSize { get; set; } = new Size2i(0, 0);

		/// <summary>
		/// The maximum allowable size of this window.
		/// </summary>
		public Size2i MaxSize { get; set; } = new Size2i(int.MaxValue, int.MaxValue);

		#endregion

		#region Internal state

		/// <summary>
		/// The surface on which all content rendering takes place.
		/// </summary>
		private SKSurface? _surface;
		private Size2i _surfaceSize = new Size2i(-1, -1);
		private IntPtr _memoryDc = IntPtr.Zero;
		private IntPtr _dib = IntPtr.Zero;
		private IntPtr _oldBitmap = IntPtr.Zero;
		private SkiaRenderer? _renderer;

		#endregion

		#region Externally-bindable events

		public event EventHandler? DocumentChanged;
		public event EventHandler? TitleChanged;
		public event EventHandler? ShowTitlebarChanged;

		public event EventHandler<RectChangingEventArgs>? RectChanging;
		public event EventHandler<RectChangedEventArgs>? RectChanged;
		public event EventHandler<RectChangedEventArgs>? Moved;
		public event EventHandler<RectChangedEventArgs>? Sized;
		public event EventHandler<DisposeEventArgs>? Disposing;
		public event EventHandler<DisposeEventArgs>? Disposed;
		public event EventHandler<bool>? FocusChanged;
		public event EventHandler<bool>? VisibleChanged;

		public event EventHandler<CancelEventArgs>? CloseClicked;

		#endregion

		#region Static data

		private static uint _classAtom = uint.MaxValue;
		private static object _classAtomLock = new object();

		/// <summary>
		/// The current window count (in this thread, not in total).
		/// </summary>
		public static int WindowCount => _windowCount;
		[ThreadStatic]
		private static int _windowCount;

		private const string ClassName = "OnyxWindow";

		#endregion

		#region Window construction and disposal

		/// <summary>
		/// Construct a new Window.
		/// </summary>
		/// <param name="document">(optional) The document to use as the initial content of the window.</param>
		/// <param name="title">(optional) The title to display in the window's titlebar.</param>
		/// <param name="point">(optional) The top-left coordinate for the window.</param>
		/// <param name="size">(optional) The size of the window.</param>
		/// <param name="rect">(optional) The position and size of the window. If given, this overrides
		/// the individual 'point' and 'size' arguments.</param>
		/// <param name="minSize">(optional) The minimum allowed size of the window.</param>
		/// <param name="maxSize">(optional) The maximum allowed size of the window.</param>
		/// <param name="hasBorder">(optional) Whether the window has a border around it of at least one pixel.</param>
		/// <param name="hasTitlebar">(optional) Whether to display a titlebar on the window.</param>
		/// <param name="canMaximize">(optional) Whether the window has a button that allows it to be maximized.</param>
		/// <param name="canMinimize">(optional) Whether the window has a button that allows it to be minimized.</param>
		/// <param name="canResize">(optional) Whether the window can be resized by the user.</param>
		/// <param name="alwaysOnTop">(optional) Whether the window is "always on top" of other windows.</param>
		/// <param name="isToolWindow">(optional) Whether this uses "tool window" styles.</param>
		/// <param name="parentHwnd">(optional) Another HWND that is the parent of this window (as
		/// a floating or contained child window).</param>
		/// <param name="parentWindow">(optional) Another Window that is the parent of this window
		/// (as a floating or contained child window).</param>
		/// <param name="isChildWindow">(optional) Whether the parent window acts as just an owner
		/// window of this where this is a floating window or popup, or whether the parent is a true
		/// parent that contains this window fully inside it.</param>
		/// <exception cref="Win32Exception">Thrown if the window cannot be created.</exception>
		public Window(Document? document = null,
			string? title = null,
			Vector2i? point = null,
			Size2i? size = null,
			Rect2i? rect = null,
			Size2i? minSize = null,
			Size2i? maxSize = null,
			bool hasBorder = true,
			bool hasTitlebar = true,
			bool canResize = true,
			bool canMinimize = true,
			bool canMaximize = true,
			bool isToolWindow = false,
			bool alwaysOnTop = false,
			IntPtr parentHwnd = default,
			Window? parentWindow = null,
			bool isChildWindow = false)
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
			_isChildWindow = isChildWindow;

			_hasTitlebar = hasTitlebar;
			_hasBorder = hasBorder;
			_canResize = canResize;
			_canMinimize = canMinimize;
			_canMaximize = canMaximize;
			_isToolWindow = isToolWindow;
			_alwaysOnTop = alwaysOnTop;

			MinSize = minSize ?? MinSize;
			MaxSize = maxSize ?? MaxSize;

			uint atom = GetWindowClassAtom();

			_gcHandle = GCHandle.Alloc(this);

			(uint style, uint exStyle) = CalculateWindowStyle();

			Handle = Win32.CreateWindowEx(exStyle,
				ClassName, title ?? string.Empty, style,
				r.X, r.Y, r.Width, r.Height, parentWindow?.Handle ?? parentHwnd, IntPtr.Zero, IntPtr.Zero,
				GCHandle.ToIntPtr(_gcHandle));
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
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

		#endregion

		#region Window-style calculation

		/// <summary>
		/// Update Windows's understanding of the style based on our internal style flags.
		/// </summary>
		private void UpdateWindowStyle()
		{
			(uint style, uint exStyle) = CalculateWindowStyle();

			uint oldStyle = Win32.GetWindowLong(Handle, Win32.GWL_STYLE);
			if (style != oldStyle)
				Win32.SetWindowLong(Handle, Win32.GWL_STYLE, style);

			uint oldExStyle = Win32.GetWindowLong(Handle, Win32.GWL_EXSTYLE);
			if (exStyle != oldExStyle)
				Win32.SetWindowLong(Handle, Win32.GWL_EXSTYLE, exStyle);
		}

		/// <summary>
		/// Calculate the correct window style based on our internal style flags.
		/// </summary>
		/// <returns>The correct window style and extended style for this window.</returns>
		protected virtual (uint Style, uint ExStyle) CalculateWindowStyle()
		{
			if (_isChildWindow)
				return (Win32.WS_CHILD, 0);

			uint style =
				  (_hasBorder || _hasTitlebar ? Win32.WS_OVERLAPPED : Win32.WS_POPUP)
				| (_hasTitlebar ? Win32.WS_CAPTION : 0)
				| (_hasTitlebar ? Win32.WS_SYSMENU : 0)
				| (_hasBorder && _canResize ? Win32.WS_THICKFRAME : 0)
				| (_hasTitlebar && _canMinimize? Win32.WS_MINIMIZEBOX : 0)
				| (_hasTitlebar && _canMaximize ? Win32.WS_MAXIMIZEBOX : 0);

			uint exStyle =
				  (_hasBorder || _hasTitlebar ? Win32.WS_EX_WINDOWEDGE : 0)
				| (_hasBorder && _canResize ? Win32.WS_EX_CLIENTEDGE : 0)
				| (_isToolWindow ? Win32.WS_EX_TOOLWINDOW : 0)
				| (_alwaysOnTop ? Win32.WS_EX_TOPMOST : 0);

			return (style, exStyle);
		}

		#endregion

		#region Show, hide, activate, focus, etc.

		public void Activate()
		{
			Win32.ShowWindow(Handle, Win32.SW_SHOW);
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
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

		public void Maximize()
		{
			Win32.ShowWindow(Handle, Win32.SW_SHOWMAXIMIZED);
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
		}

		public void Minimize()
		{
			Win32.ShowWindow(Handle, Win32.SW_SHOWMINIMIZED);
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
		}

		public void Restore()
		{
			Win32.ShowWindow(Handle, Win32.SW_SHOWNORMAL);
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
		}

		public void Focus()
		{
			Win32.SetFocus(Handle);
			if (Marshal.GetLastWin32Error() != 0)
				throw new Win32Exception();
		}

		#endregion

		#region Window-size constraints

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

		#endregion

		#region WindowProc core

		/// <summary>
		/// Retrieve the window-class atom for Onyx Win32 Window objects, creating the atom if
		/// it hasn't been created yet.
		/// </summary>
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
						wndClass.lpfnWndProc = WindowProcInternal;
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

		/// <summary>
		/// All window messages arrive here first.  The only responsibility this method
		/// has is to find the correct Window instance and then propagate the messages
		/// to it (and handle necessary cleanup when the Window instance is destroyed).
		/// Everything else is done by the (overridable) WindowProc() method.
		/// </summary>
		private static unsafe IntPtr WindowProcInternal(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			// WM_NCCREATE is special, as it's the first real message that the window gets.
			// We use it to set up the GCHandle, which is needed to be able to access the
			// Window object itself.
			IntPtr gcHandleValue;
			if (msg == Win32.WM_NCCREATE)
			{
				Win32.CREATESTRUCT* lpCreateStruct = (Win32.CREATESTRUCT*)lParam;
				gcHandleValue = lpCreateStruct->lpCreateParams;

				Win32.SetWindowLongPtr(hWnd, 0, gcHandleValue);
				Interlocked.Increment(ref _windowCount);
			}
			else
			{
				gcHandleValue = Win32.GetWindowLongPtr(hWnd, 0);
			}

			// Retrieve the Window object for this window.
			Window? window = gcHandleValue != IntPtr.Zero
				? GCHandle.FromIntPtr(gcHandleValue).Target as Window
				: null;

			// WM_NCDESTROY is just as special, and needs to perform critical cleanup
			// on this window before it goes away.  It's implemented here so that even
			// if someone overrides WindowProc() and fails to call the base, we won't
			// leak resources.
			if (msg == Win32.WM_NCDESTROY && window != null)
			{
				window.FreeSurface();
				window.FreeGdiBackBuffer();

				window._gcHandle.Free();
				window._gcHandle = default;
				Win32.SetWindowLongPtr(hWnd, 0, IntPtr.Zero);

				Interlocked.Decrement(ref _windowCount);
			}

			return window?.WindowProc(hWnd, msg, wParam, lParam)
				?? Win32.DefWindowProc(hWnd, msg, wParam, lParam);
		}

		/// <summary>
		/// The Win32 window procedure for this window; all window messages are directed
		/// through this after and including WM_NCCREATE.  This method can be overridden,
		/// but you must always call the base method:  Failure to invoke the base *will*
		/// likely result in unpredictable window behavior.
		/// </summary>
		/// <param name="hWnd">The window handle for this window.</param>
		/// <param name="msg">The message.</param>
		/// <param name="wParam">The first message parameter.</param>
		/// <param name="lParam">The second message parameter.</param>
		/// <returns>The message response.</returns>
		protected virtual unsafe IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			switch (msg)
			{
				case Win32.WM_NCCREATE:
					OnWin32_NcCreate(ref *(Win32.CREATESTRUCT*)lParam);
					break;

				case Win32.WM_NCDESTROY:
					OnWin32_NcDestroy();
					break;

				case Win32.WM_CREATE:
					OnWin32_Create(ref *(Win32.CREATESTRUCT*)lParam);
					break;

				case Win32.WM_DESTROY:
					OnWin32_Destroy();
					break;

				case Win32.WM_SHOWWINDOW:
					OnWin32_ShowWindow(wParam != 0, (int)lParam);
					break;

				case Win32.WM_ACTIVATE:
					OnWin32_Activate((int)wParam, lParam);
					break;

				case Win32.WM_WINDOWPOSCHANGING:
					return OnWin32_WindowPosChanging(ref *(Win32.WINDOWPOS*)lParam);

				case Win32.WM_WINDOWPOSCHANGED:
					return OnWin32_WindowPosChanged(ref *(Win32.WINDOWPOS*)lParam);

				case Win32.WM_PAINT:
					return OnWin32_Paint();

				case Win32.WM_CLOSE:
					OnClose();
					return IntPtr.Zero;

				case Win32.WM_MOUSEMOVE:
					OnMouseMove((ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;

				case Win32.WM_LBUTTONDOWN:
					OnMouseButton(MouseButton.Left, MouseButtonAction.Press, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_LBUTTONUP:
					OnMouseButton(MouseButton.Left, MouseButtonAction.Release, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_LBUTTONDBLCLK:
					OnMouseButton(MouseButton.Left, MouseButtonAction.DoubleClick, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;

				case Win32.WM_RBUTTONDOWN:
					OnMouseButton(MouseButton.Right, MouseButtonAction.Press, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_RBUTTONUP:
					OnMouseButton(MouseButton.Right, MouseButtonAction.Release, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_RBUTTONDBLCLK:
					OnMouseButton(MouseButton.Right, MouseButtonAction.DoubleClick, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;

				case Win32.WM_MBUTTONDOWN:
					OnMouseButton(MouseButton.Middle, MouseButtonAction.Press, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_MBUTTONUP:
					OnMouseButton(MouseButton.Middle, MouseButtonAction.Release, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_MBUTTONDBLCLK:
					OnMouseButton(MouseButton.Middle, MouseButtonAction.DoubleClick, (ModifierKeys)wParam, LParamToVector2i(lParam));
					return IntPtr.Zero;

				case Win32.WM_XBUTTONDOWN:
					OnMouseButton((MouseButton)((int)MouseButton.X1 + ((wParam >> 16) & 0xFFFF)),
						MouseButtonAction.Press, (ModifierKeys)(wParam & 0xFFFF), LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_XBUTTONUP:
					OnMouseButton((MouseButton)((int)MouseButton.X1 + ((wParam >> 16) & 0xFFFF)),
						MouseButtonAction.Release, (ModifierKeys)(wParam & 0xFFFF), LParamToVector2i(lParam));
					return IntPtr.Zero;
				case Win32.WM_XBUTTONDBLCLK:
					OnMouseButton((MouseButton)((int)MouseButton.X1 + ((wParam >> 16) & 0xFFFF)),
						MouseButtonAction.DoubleClick, (ModifierKeys)(wParam & 0xFFFF), LParamToVector2i(lParam));
					return IntPtr.Zero;

				case Win32.WM_KEYDOWN:
					OnKey((int)wParam, (lParam & (1 << 30)) != 0 ? KeyAction.Repeat : KeyAction.Press);
					return IntPtr.Zero;
				case Win32.WM_KEYUP:
					OnKey((int)wParam, KeyAction.Release);
					return IntPtr.Zero;
				case Win32.WM_CHAR:
					OnChar((char)wParam);
					return IntPtr.Zero;

				case Win32.WM_SETFOCUS:
					OnWin32_Focus(true);
					return IntPtr.Zero;
				case Win32.WM_KILLFOCUS:
					OnWin32_Focus(false);
					return IntPtr.Zero;
			}

			return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
		}

		/// <summary>
		/// A tiny helper that turns mouse LPARAM values into Vector2i structs.
		/// </summary>
		private static Vector2i LParamToVector2i(IntPtr lParam)
			=> new Vector2i((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));

		#endregion

		#region Win32 window messages
		
		// These are Windows-specific handlers for Windows-specific behavior.  We don't
		// attempt to make them portable, and a lot of them take WPARAM and LPARAM values
		// directly, or something that's very thinly-disguised WPARAM and LPARAM values.
		//
		// Code that uses these handlers is generally *not* portable to other UI platforms,
		// and overriding these methods may produce unexpected behavior if the base
		// method is not invoked.

		protected virtual void OnWin32_Focus(bool focus)
		{
			if (_isFocused != focus)
			{
				_isFocused = focus;
				FocusChanged?.Invoke(this, focus);
			}
		}

		protected virtual void OnWin32_Activate(int activated, IntPtr windowHandle)
		{
		}

		protected unsafe virtual void OnWin32_ShowWindow(bool shown, int status)
		{
			IsMinimized = (status == Win32.SW_SHOWMINIMIZED
				|| status == Win32.SW_SHOWMINNOACTIVE
				|| status == Win32.SW_MINIMIZE
				|| status == Win32.SW_FORCEMINIMIZE);

			IsMaximized = (status == Win32.SW_SHOWMAXIMIZED);

			if (_isVisible != shown)
			{
				_isVisible = shown;
				VisibleChanged?.Invoke(this, shown);
			}
		}

		protected unsafe virtual void OnWin32_NcCreate(ref Win32.CREATESTRUCT createStruct)
		{
		}

		protected unsafe virtual void OnWin32_NcDestroy()
		{
			if (_windowCount == 0)
				WindowsMessageQueue.Quit();
		}

		protected unsafe virtual void OnWin32_Create(ref Win32.CREATESTRUCT createStruct)
		{
		}

		protected unsafe virtual void OnWin32_Destroy()
		{
		}

		protected unsafe virtual IntPtr OnWin32_WindowPosChanged(ref Win32.WINDOWPOS lpWindowPos)
		{
			Rect2i oldRect = _rect;
			Rect2i newRect = new Rect2i(lpWindowPos.x, lpWindowPos.y, lpWindowPos.cx, lpWindowPos.cy);

			if (oldRect != newRect)
			{
				_rect = newRect;
				Win32.RECT clientRect;
				Win32.GetClientRect(Handle, out clientRect);
				_clientRect = new Rect2i(clientRect.Left, clientRect.Top,
					clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);
				OnRectChanged(oldRect, newRect);
			}

			if (oldRect.Point != newRect.Point)
				OnMoved(oldRect, newRect);

			if (oldRect.Size != newRect.Size)
				OnSized(oldRect, newRect);

			return IntPtr.Zero;
		}

		protected unsafe virtual IntPtr OnWin32_WindowPosChanging(ref Win32.WINDOWPOS windowPos)
		{
			Rect2i oldRect = _rect;
			Rect2i newRect = new Rect2i(windowPos.x, windowPos.y, windowPos.cx, windowPos.cy);

			if (oldRect != newRect)
			{
				Rect2i? modifiedRect;
				if (!(modifiedRect = OnRectChanging(oldRect, newRect)).HasValue)
				{
					windowPos.flags |= Win32.SWP_NOACTIVATE
						| Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOREPOSITION
						| Win32.SWP_NOOWNERZORDER | Win32.SWP_NOZORDER;
				}
				else
				{
					windowPos.x = modifiedRect.Value.X;
					windowPos.y = modifiedRect.Value.Y;
					windowPos.cx = modifiedRect.Value.Width;
					windowPos.cy = modifiedRect.Value.Height;
				}
			}

			return IntPtr.Zero;
		}

		protected unsafe virtual IntPtr OnWin32_Paint()
		{
			Win32.PAINTSTRUCT paintStruct = default;

			IntPtr hdc = Win32.BeginPaint(Handle, ref paintStruct);

			Render(hdc, new Rect2i(paintStruct.rcPaint.Left, paintStruct.rcPaint.Top,
				paintStruct.rcPaint.Right - paintStruct.rcPaint.Left,
				paintStruct.rcPaint.Bottom - paintStruct.rcPaint.Top));

			Win32.EndPaint(Handle, ref paintStruct);

			return IntPtr.Zero;
		}

		#endregion

		#region Mid-level window messages

		// These message handlers are still fairly Win32-oriented, but they're still
		// designed to be more general and reusable than the true OnWin32_*() handlers.
		//
		// Code written that uses these *may* be somewhat portable to other UI platforms,
		// with some mild massaging.

		protected virtual void OnClose()
		{
			CancelEventArgs cancelEventArgs = new CancelEventArgs();
			CloseClicked?.Invoke(this, cancelEventArgs);

			if (!cancelEventArgs.Cancel)
				Dispose(true);
		}

		protected virtual void OnChar(char ch)
		{
		}

		protected virtual void OnKey(int virtualKey, KeyAction action)
		{
		}

		protected virtual void OnMouseButton(MouseButton button, MouseButtonAction action,
			ModifierKeys keys, Vector2i mousePos)
		{
		}

		protected virtual void OnMouseMove(ModifierKeys keys, Vector2i mousePos)
		{
		}

		#endregion

		#region High-level messages

		// These messages are generally portable to all Window-like platform implementations, and
		// even to many plaform implementations that are not (like full-screen kiosk implementations).
		// These are fairly abstract events and should be usable regardless of platform.

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

		protected virtual void OnDisposing(bool isDisposing)
			=> Disposing?.Invoke(this, new DisposeEventArgs(isDisposing));

		protected virtual void OnDisposed(bool isDisposing)
			=> Disposed?.Invoke(this, new DisposeEventArgs(isDisposing));

		#endregion

		#region Rendering logic

		private void Render(IntPtr hdc, Rect2i paintRect)
		{
			SkiaRenderer skiaRenderer = GetRenderer();
			IRenderables renderables = skiaRenderer;
			IRenderer renderer = skiaRenderer;

			//---- TODO: Render any dirty parts of Document that intersect paintRect.

			renderer.Clear(DefaultBackgroundColor);

			using IFont? font = renderables.CreateFont(new FontInfo("Segoe UI", 14), false);

			// linear-gradient(to bottom, #b8e1fc 0%,#a9d2f3 10%,#90bae4 25%,#90bcea 37%,#90bff0 50%,#6ba8e5 51%,#a2daf5 83%,#bdf3fd 100%)
			LinearGradient blueGradient = new LinearGradient
			{
				UsesTo = true,
				SideOrCorner = SideOrCorner.Bottom,
				ColorStops =
				[
					new ColorStop(new Color32(0xb8, 0xe1, 0xfc), new Measure(Units.Percent, 0)),
					new ColorStop(new Color32(0xa9, 0xd2, 0xf3), new Measure(Units.Percent, 10)),
					new ColorStop(new Color32(0x90, 0xba, 0xe4), new Measure(Units.Percent, 25)),
					new ColorStop(new Color32(0x90, 0xbc, 0xea), new Measure(Units.Percent, 37)),
					new ColorStop(new Color32(0x90, 0xbf, 0xf0), new Measure(Units.Percent, 50)),
					new ColorStop(new Color32(0x6b, 0xa8, 0xe5), new Measure(Units.Percent, 51)),
					new ColorStop(new Color32(0xa2, 0xda, 0xf5), new Measure(Units.Percent, 83)),
					new ColorStop(new Color32(0xbd, 0xf3, 0xfd), new Measure(Units.Percent, 100))
				]
			};

			using IBrush gradientBrush = new SkiaLinearGradientBrush(blueGradient);

			DrawStyle style = DrawStyle.Default.WithFont(font!);

			renderer.FillRect(new Rect2d(100, 100, 200, 100), style.WithColor(Color32.CadetBlue));

			for (int i = 0; i < 50; i++)
			{
				Rect2d rect = new Rect2d(10 + i * 10, 10 + i * 10, 200, 100);
				renderer.FillRoundRect(rect, new CornerRadii(24),
					style.WithBrush(gradientBrush, rect));
				renderer.DrawRoundRect(rect, new CornerRadii(24),
					style.WithColor(new Color32(0x6b, 0xa8, 0xe5)).WithLineThickness(1.5));
			}

			DrawStyle blackStyle = style.WithColor(Color32.Black);

			Vector2d lineStart = new Vector2d(10, 250);
			renderer.DrawText(lineStart, "Hello, World.", blackStyle);
			lineStart += new Vector2d(0, font!.FontMetrics.LineHeight);
			renderer.DrawText(lineStart, "Goodbye, World.", blackStyle);

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
				pixelBuffer, size.Width * 4,
				new SKSurfaceProperties(SKPixelGeometry.RgbHorizontal));
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
