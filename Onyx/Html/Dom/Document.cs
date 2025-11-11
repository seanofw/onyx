using System.Text;
using Onyx.Css;
using Onyx.Html.Parsing;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// The root of the node tree.  Unlike in the JS DOM, this class is designed to be relatively
	/// easy to replace with an alternate root like a DocumentFragment that implements similar
	/// "root-like" functionality (but that may not necessary provide efficient lookups and queries
	/// like this class does).
	/// </summary>
	public class Document : ContainerNode, IElementLookupContainer, IStyleRoot
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

		public IStyleManager StyleManager => _styleManager;
		private readonly StyleManager _styleManager = new StyleManager();

		public IStyleQueue StyleQueue => _styleQueue;
		private readonly StyleQueue _styleQueue = new StyleQueue();

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
	}
}
