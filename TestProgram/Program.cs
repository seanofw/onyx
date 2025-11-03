using Onyx.Css;
using Onyx.Css.Computed;
using Onyx.Css.Parsing;
using Onyx.Html.Dom;

namespace TestProgram
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			//Element div = new Element("div");
			//div.InnerHtml = "<em><strong>This</strong> is a test</em>";

			//CompoundSelector[] compoundSelectors =
			//[
			//	CompoundSelector.Parse("input.foo[type], input.foo[type=text]")!,
			//	CompoundSelector.Parse(".foo")!,
			//	CompoundSelector.Parse("input")!,
			//];

			//Selector[] ordered = compoundSelectors
			//	.SelectMany(c => c.Selectors)
			//	.OrderBy(s => s.Specificity)
			//	.ToArray();

			Document document = new Document(
@"<div class='foo'>
	<div class='bar' id='frob'>
		<span class='qux'>Alice</span>
		<span class='foo'>Bill</span>
	</div>
	<div class='bar'>Charles</div>
</div>

<button id='foo'>foo</button>

<div class='bar'>Dave</div>
<div class='foo'>Emily</div>
<div class='foo'>Frank</div>");

			IEnumerable<Node> nodes = document.Find(".bar .foo").Closest(".bar");
			IEnumerable<Node> nodes2 = document.Find("#frob .foo").Where("span").Closest("#frob");
			IEnumerable<Node> nodes3 = document.Find("#frob").Find(".foo");

			const string StylesheetText = @"
window {
	display: flex;
	background: white;
	font: 14px Arial;
	color: green;
}

input[type=text] {
	border: 1px solid #CCC;
	background: white;
	font: 14px Arial;
}

.foo .foo {
	color: orange;
}

.foo {
	color: red;
	background: green;
}
";

			CssParser parser = new CssParser();
			Stylesheet stylesheet = parser.Parse(StylesheetText, "<inline>");
			document.AddStylesheet(stylesheet);

			Element? foo = document.Get("#foo");

			foreach (Element element in document.Find(".foo"))
			{
				ComputedStyle computedStyle = element.GetComputedStyle();
			}
		}
	}
}
