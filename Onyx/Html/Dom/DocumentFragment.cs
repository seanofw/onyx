using System.Text;

namespace Onyx.Html.Dom
{
	/// <summary>
	/// A DocumentFragment is like a lightweight Document, a tree root that can be used
	/// to host nodes without much overhead.  Note that selector operations on Elements attached
	/// to a DocumentFragment will still work, but will be much less efficient than when the
	/// Element is hosted in a Document; and that Elements within a DocumentFragment cannot be
	/// styled or rendered.
	/// </summary>
	public class DocumentFragment : ContainerNode
	{
		public override string NodeName => "/";

		public override NodeType NodeType => NodeType.DocumentFragment;

		/// <summary>
		/// This property is a simple shorthand proxy for reading and writing the InnerHtml
		/// property, but reads better when working with a Document.
		/// </summary>
		public string Html
		{
			get => InnerHtml;
			set => InnerHtml = value;
		}

		public DocumentFragment(string? content = null)
		{
			Root = this;

			if (!string.IsNullOrEmpty(content))
			{
				Html = content;
			}
		}

		public override Node CloneNode(bool deep = false)
		{
			DocumentFragment clone = new DocumentFragment();
			clone.SourceLocation = SourceLocation;
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
	}
}
