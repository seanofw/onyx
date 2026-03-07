using System.Text;
using Onyx.Boxes;
using Onyx.Css;
using Onyx.Css.Types;
using Onyx.Css.Types.Media;
using Onyx.Types;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// The root of the node tree.  Unlike in the JS DOM, this class is designed to be relatively
	/// easy to replace with an alternate root like a DocumentFragment that implements similar
	/// "root-like" functionality (but that may not necessary provide efficient lookups and queries
	/// like this class does).
	/// </summary>
	public class Document : ContainerNode, IElementLookupContainer, IStyleRoot, IViewportRoot
	{
		ElementLookupTables IElementLookupContainer.ElementLookupTables => _elementLookupTables;
		private ElementLookupTables _elementLookupTables = new ElementLookupTables();

		public override string NodeName => "/";

		public override NodeType NodeType => NodeType.Document;

		/// <summary>
		/// This property is a simple shorthand proxy for reading and writing the InnerHtml
		/// property, but reads better when working with a Document.
		/// </summary>
		public string Html
		{
			get => InnerHtml;
			set => InnerHtml = value;
		}

		MediaQueryContext IStyleRoot.MediaQueryContext
			=> new MediaQueryContext(MediaDimensions, MediaInfo);

		public MediaInfo MediaInfo
		{
			get => _mediaInfo;
			set
			{
				if (!object.Equals(_mediaInfo, value))
				{
					_mediaInfo = value;
					InvalidateChildComputedStyles();
				}
			}
		}
		private MediaInfo _mediaInfo = default;

		public MediaDimensions MediaDimensions
		{
			get => _mediaDimensions;
			set
			{
				if (!object.Equals(_mediaDimensions, value))
				{
					_mediaDimensions = value;
					InvalidateChildComputedStyles();
				}
			}
		}
		private MediaDimensions _mediaDimensions = default;

		/// <summary>
		/// Get or change the size of the viewport containing this document, in pixels.
		/// </summary>
		public Rect2d ViewportRect
		{
			get => _viewportRect;
			set
			{
				if (_viewportRect != value)
				{
					_viewportRect = value;
					OnViewportChanged();
				}
			}
		}
		private Rect2d _viewportRect = new Rect2d(0, 0, double.MaxValue, double.MaxValue);

		/// <summary>
		/// Get the calculated dimensions of the document, in pixels.
		/// </summary>
		public Rect2d DocumentRect { get; private set; }

		/// <summary>
		/// The root box of the render tree.
		/// </summary>
		public Box? Box { get; internal set; }

		public IStyleManager StyleManager => _styleManager;
		private readonly StyleManager _styleManager = new StyleManager();

		public IStyleQueue StyleQueue => _styleQueue;
		private readonly StyleQueue _styleQueue = new StyleQueue();

		public IBoxQueue MeasureQueue => _measureQueue;
		private readonly BoxQueue _measureQueue = new BoxQueue();

		public IBoxQueue ArrangeQueue => _arrangeQueue;
		private readonly BoxQueue _arrangeQueue = new BoxQueue();

		public IBoxQueue PaintQueue => _paintQueue;
		private readonly BoxQueue _paintQueue = new BoxQueue();

		public Document(string? content = null)
		{
			Root = this;

			if (!string.IsNullOrEmpty(content))
			{
				Html = content;
			}

			_styleManager.StylesheetsChanged += StyleManager_StylesheetsChanged;
		}

		private void StyleManager_StylesheetsChanged(object? sender, EventArgs e)
		{
			// Invalidate the styles of the entire tree, since the stylesheets have changed.
			InvalidateChildComputedStyles();
		}

		public void AddStylesheet(string text, string filename)
			=> StyleManager.AddStylesheet(text, filename);

		public void AddStylesheet(Stylesheet stylesheet)
			=> StyleManager.AddStylesheet(stylesheet);

		public void RemoveStylesheet(Stylesheet stylesheet)
			=> StyleManager.RemoveStylesheet(stylesheet);

		public void ValidateComputedStyles()
			=> StyleQueue.ProcessQueue();

		public override Node CloneNode(bool deep = false)
		{
			Document clone = new Document();
			clone.SourceLocation = SourceLocation;

			foreach (Stylesheet stylesheet in StyleManager.Stylesheets)
				clone.StyleManager.AddStylesheet(stylesheet);

			if (deep)
				CloneDescendantsTo(clone);

			return clone;
		}

		public override void ToString(StringBuilder stringBuilder)
		{
			foreach (Node child in Children)
			{
				child.ToString(stringBuilder);
			}
		}

		void IElementLookupContainer.AddDescendant(Element element)
			=> _elementLookupTables.AddElement(element);

		void IElementLookupContainer.RemoveDescendant(Element element)
			=> _elementLookupTables.RemoveElement(element);

		public IReadOnlyCollection<Element> GetElementsById(string id)
			=> _elementLookupTables.GetElementsById(id);

		public IReadOnlyCollection<Element> GetElementsByClassname(string classname)
			=> _elementLookupTables.GetElementsByClassname(classname);

		public IReadOnlyCollection<Element> GetElementsByName(string name)
			=> _elementLookupTables.GetElementsByName(name);

		public IReadOnlyCollection<Element> GetElementsByType(string type)
			=> _elementLookupTables.GetElementsByElementType(type);

		public IReadOnlyCollection<Element> GetElementsByTypeAttribute(string name)
			=> _elementLookupTables.GetElementsByTypeAttribute(name);

		protected virtual void OnViewportChanged()
		{
			if (Box != null)
			{
				Box.Flags |= BoxFlags.NeedsArrange;
			}
		}
	}
}
