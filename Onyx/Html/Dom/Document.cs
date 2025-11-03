using System.Data;
using System.Text;
using Onyx.Css;
using Onyx.Html.Parsing;

namespace Onyx.Html.Dom
{
	public class Document : ContainerNode, IElementLookupContainer, IStyleRoot
	{
		ElementLookupTables IElementLookupContainer.ElementLookupTables => _elementLookupTables;
		private ElementLookupTables _elementLookupTables = new ElementLookupTables();

		public override string NodeName => "/";

		public override NodeType NodeType => NodeType.Document;

		public override string? Value
		{
			get => null;
			set => throw new NotSupportedException();
		}

		public override string TextContent
		{
			get => string.Join("", Children.Select(c => c.TextContent));
			set => throw new NotSupportedException();
		}

		public string Html
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (Node child in Children)
				{
					child.ToString(stringBuilder);
				}
				return stringBuilder.ToString();
			}

			set
			{
				HtmlParser.ParseInnerHtml(value, this);
			}
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
			throw new NotImplementedException();
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
