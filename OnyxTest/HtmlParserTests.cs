using NUnit.Framework;
using Onyx.Html.Dom;
using Onyx.Html.Parsing;

namespace Onyx.Tests
{
    [TestFixture]
    public class HtmlParserTests
    {
        [Test]
        public void CanParseAnEmptyDocument()
        {
            Document fragment = new HtmlParser().Parse("", "test.html");
        }

		[Test]
		public void CanParseAnElementContainingText()
		{
			Document fragment = new HtmlParser().Parse("<p>Hello, World.</p>", "test.html");
            Assert.That(fragment.ChildNodes.Count, Is.EqualTo(1));

            Element? p = fragment.ChildNodes[0] as Element;
            Assert.That(p, Is.Not.Null);
            Assert.That(p!.ChildNodes.Count, Is.EqualTo(1));

            TextNode? text = p.ChildNodes[0] as TextNode;
			Assert.That(text, Is.Not.Null);
            Assert.That(text!.Value, Is.EqualTo("Hello, World."));
		}

		[Test]
		public void CanParseAnElementContainingStyledText()
		{
			Document fragment = new HtmlParser().Parse("<p><b>Hello</b>, <i>World</i>.</p>", "test.html");
			Assert.That(fragment.ChildNodes.Count, Is.EqualTo(1));

			Element? p = fragment.ChildNodes[0] as Element;
			Assert.That(p, Is.Not.Null);
			Assert.That(p!.ChildNodes.Count, Is.EqualTo(4));

			Element? b = p.ChildNodes[0] as Element;
			Assert.That(b, Is.Not.Null);
			Assert.That(b!.ChildNodes.Count, Is.EqualTo(1));

			TextNode? text1 = b.ChildNodes[0] as TextNode;
			Assert.That(text1, Is.Not.Null);
			Assert.That(text1!.Value, Is.EqualTo("Hello"));

			TextNode? text2 = p.ChildNodes[1] as TextNode;
			Assert.That(text2, Is.Not.Null);
			Assert.That(text2!.Value, Is.EqualTo(", "));

			Element? i = p.ChildNodes[2] as Element;
			Assert.That(i, Is.Not.Null);
			Assert.That(i!.ChildNodes.Count, Is.EqualTo(1));

			TextNode? text3 = i.ChildNodes[0] as TextNode;
			Assert.That(text3, Is.Not.Null);
			Assert.That(text3!.Value, Is.EqualTo("World"));

			TextNode? text4 = p.ChildNodes[3] as TextNode;
			Assert.That(text4, Is.Not.Null);
			Assert.That(text4!.Value, Is.EqualTo("."));
		}

		[Test]
		public void CanParseAnElementContainingMismatchedTags()
		{
			Document fragment = new HtmlParser().Parse("<p><b>Hello<i> World.</b></p>", "test.html");
			Assert.That(fragment.ChildNodes.Count, Is.EqualTo(1));

			Element? p = fragment.ChildNodes[0] as Element;
			Assert.That(p, Is.Not.Null);
			Assert.That(p!.ChildNodes.Count, Is.EqualTo(1));

			Element? b = p.ChildNodes[0] as Element;
			Assert.That(b, Is.Not.Null);
			Assert.That(b!.ChildNodes.Count, Is.EqualTo(2));

			TextNode? text1 = b.ChildNodes[0] as TextNode;
			Assert.That(text1, Is.Not.Null);
			Assert.That(text1!.Value, Is.EqualTo("Hello"));

			Element? i = b.ChildNodes[1] as Element;
			Assert.That(i, Is.Not.Null);
			Assert.That(i!.ChildNodes.Count, Is.EqualTo(1));

			TextNode? text3 = i.ChildNodes[0] as TextNode;
			Assert.That(text3, Is.Not.Null);
			Assert.That(text3!.Value, Is.EqualTo(" World."));
		}

		[Test]
		public void CanParseNakedStyledText()
		{
			Document fragment = new HtmlParser().Parse("<b>Hello</b>, <i>World</i>.", "test.html");
			Assert.That(fragment.ChildNodes.Count, Is.EqualTo(4));

			Element? b = fragment.ChildNodes[0] as Element;
			Assert.That(b, Is.Not.Null);
			Assert.That(b!.ChildNodes.Count, Is.EqualTo(1));

			TextNode? text1 = b.ChildNodes[0] as TextNode;
			Assert.That(text1, Is.Not.Null);
			Assert.That(text1!.Value, Is.EqualTo("Hello"));

			TextNode? text2 = fragment.ChildNodes[1] as TextNode;
			Assert.That(text2, Is.Not.Null);
			Assert.That(text2!.Value, Is.EqualTo(", "));

			Element? i = fragment.ChildNodes[2] as Element;
			Assert.That(i, Is.Not.Null);
			Assert.That(i!.ChildNodes.Count, Is.EqualTo(1));

			TextNode? text3 = i.ChildNodes[0] as TextNode;
			Assert.That(text3, Is.Not.Null);
			Assert.That(text3!.Value, Is.EqualTo("World"));

			TextNode? text4 = fragment.ChildNodes[3] as TextNode;
			Assert.That(text4, Is.Not.Null);
			Assert.That(text4!.Value, Is.EqualTo("."));
		}

		[Test]
		public void CanParseNakedStyledTextContainingMismatchedTags()
		{
			Document fragment = new HtmlParser().Parse("<b>Hello<i> World.</b>", "test.html");
			Assert.That(fragment.ChildNodes.Count, Is.EqualTo(1));

			Element? b = fragment.ChildNodes[0] as Element;
			Assert.That(b, Is.Not.Null);
			Assert.That(b!.ChildNodes.Count, Is.EqualTo(2));

			TextNode? text1 = b.ChildNodes[0] as TextNode;
			Assert.That(text1, Is.Not.Null);
			Assert.That(text1!.Value, Is.EqualTo("Hello"));

			Element? i = b.ChildNodes[1] as Element;
			Assert.That(i, Is.Not.Null);
			Assert.That(i!.ChildNodes.Count, Is.EqualTo(1));

			TextNode? text3 = i.ChildNodes[0] as TextNode;
			Assert.That(text3, Is.Not.Null);
			Assert.That(text3!.Value, Is.EqualTo(" World."));
		}

		[Test]
		public void CanReStringifyHtmlGarbage()
		{
			Document fragment = new HtmlParser().Parse("<p><b>Hello<i> World.</b></p>", "test.html");

			Assert.That(fragment.ToString(), Is.EqualTo("<p><b>Hello<i> World.</i></b></p>"));
		}
	}
}
