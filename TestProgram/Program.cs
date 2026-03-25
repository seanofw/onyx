using Onyx.Css;
using Onyx.Css.Computed;
using Onyx.Css.Parsing;
using Onyx.Css.Types;
using Onyx.Css.Types.Media;
using Onyx.Html.Dom;
using Onyx.Types;
using Onyx.Windows;

namespace TestProgram
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			//Window window = new Window(
			//	title: "Hello, World",
			//	size: new Size2i(640, 480),
			//	minSize: new Size2i(160, 120),
			//	maxSize: new Size2i(800, 600)
			//);
			//window.Show();

			//WindowsMessageQueue.Run();

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
input[type=text] {
	border: 1px solid #CCC;
	background: white;
	font: 14px Arial;
}

window {
	display: flex;
	background: white;
	font: 14px Arial;
	color: green;
}

.foo .foo {
	color: orange;
}

@media screen and (min-width: 640px) {
	.foo {
		color: red;
		background: green;
	}
}
";

			CssParser parser = new CssParser();
			Stylesheet stylesheet = parser.Parse(StylesheetText, "<inline>");
			document.AddStylesheet(stylesheet);

			document.MediaInfo = new MediaInfo(MediaType.Screen);

			document.MediaDimensions = new MediaDimensions(
				width: new Measure(Units.Pixels, 320),
				height: new Measure(Units.Pixels, 480)
			);

			Element? foo = document.Get("#foo");

			List<ComputedStyle> computedStyles = new List<ComputedStyle>();
			foreach (Element element in document.Find(".foo"))
			{
				ComputedStyle computedStyle = element.GetComputedStyle();
				computedStyles.Add(computedStyle);
			}
		}
	}
}
