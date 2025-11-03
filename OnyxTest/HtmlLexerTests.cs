using NUnit.Framework;
using Onyx.Html.Parsing;

namespace Onyx.Tests
{
	[TestFixture]
	public class HtmlLexerTests
	{
		[Test]
		public void CanLexEmptyString()
		{
			HtmlLexer lexer = new HtmlLexer("", "test.html");
			Assert.That(lexer.Next().Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexWhitespaceAsText()
		{
			HtmlLexer lexer = new HtmlLexer("    \t    \r\n   \n    \r    \n\r    \t    ", "test.html");
			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("    \t    \r\n   \n    \r    \n\r    \t    "));
			Assert.That(token.SourceLocation.Line, Is.EqualTo(1));
			Assert.That(lexer.Line, Is.EqualTo(5));
		}

		[Test]
		public void CanLexComments()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<!-- This is <comment-content> -->\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Comment));
			Assert.That(token.Text, Is.EqualTo(" This is <comment-content> "));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexBadComments()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<!-- This is <comment-content> \r\n   \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Comment));
			Assert.That(token.Text, Is.EqualTo(" This is <comment-content> \r\n   \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAStartTag()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanPeek()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Peek();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAStartTag2()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo    >\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexBasicText()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<p>Hello, World.</p>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("p"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("Hello, World."));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.EndTag));
			Assert.That(token.Text, Is.EqualTo("p"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void BrokenTagsShouldJustBeText()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<  foo    >\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("<"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("  foo    >\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void BrokenTagsShouldJustBeText2()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<123>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("<"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("123>\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void BrokenTagsShouldJustBeText3()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("<"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void BrokenTagsShouldJustBeText4()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n</", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("</"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void BrokenTagsShouldJustBeText5()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n</123>\n    \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("</"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("123>\n    \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAnEndTag()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n</foo>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.EndTag));
			Assert.That(token.Text, Is.EqualTo("foo"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void AnEndTagWithAnythingInItShouldBeAnError()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n</foo bar>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.EndTag));
			Assert.That(token.Text, Is.EqualTo("foo"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo(" bar>\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAnEmptyAttribute()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo bar>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));
			Assert.That(token.Attributes, Is.EqualTo(new[] { new KeyValuePair<string, string?>("bar", null) }));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexEmptyAttributes()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo bar baz qux>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));
			Assert.That(token.Attributes, Is.EqualTo(new[]
			{
				new KeyValuePair<string, string?>("bar", null),
				new KeyValuePair<string, string?>("baz", null),
				new KeyValuePair<string, string?>("qux", null)
			}));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexWeirdlyEmptyAttributes()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo bar baz=\"\" qux=>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));
			Assert.That(token.Attributes, Is.EqualTo(new[]
			{
				new KeyValuePair<string, string?>("bar", null),
				new KeyValuePair<string, string?>("baz", ""),
				new KeyValuePair<string, string?>("qux", null)
			}));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAttributesWithValues()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo bar=gronk baz=\"flarb\" qux=plooz>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));
			Assert.That(token.Attributes, Is.EqualTo(new[]
			{
				new KeyValuePair<string, string?>("bar", "gronk"),
				new KeyValuePair<string, string?>("baz", "flarb"),
				new KeyValuePair<string, string?>("qux", "plooz")
			}));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAnUnclosedTag()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo bar=gronk baz=\"flarb\" qux=plooz", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));
			Assert.That(token.Attributes, Is.EqualTo(new[]
			{
				new KeyValuePair<string, string?>("bar", "gronk"),
				new KeyValuePair<string, string?>("baz", "flarb"),
				new KeyValuePair<string, string?>("qux", "plooz")
			}));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexATagWithTrashInIt()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<foo bar=gronk \"baz=\"flarb\" qux=plooz>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("foo"));
			Assert.That(token.Attributes, Is.EqualTo(new[]
			{
				new KeyValuePair<string, string?>("bar", "gronk"),
				new KeyValuePair<string, string?>("baz", "flarb"),
				new KeyValuePair<string, string?>("qux", "plooz")
			}));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CasesShouldNotChange()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<FOO BAR=gronk BAZ=\"flarb\" QUX=plooz>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("FOO"));
			Assert.That(token.Attributes, Is.EqualTo(new[]
			{
				new KeyValuePair<string, string?>("BAR", "gronk"),
				new KeyValuePair<string, string?>("BAZ", "flarb"),
				new KeyValuePair<string, string?>("QUX", "plooz")
			}));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAPseduoSelfClosingAttribute1()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<br/>\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("br"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void CanLexAPseduoSelfClosingAttribute2()
		{
			HtmlLexer lexer = new HtmlLexer("\t  \n<br />\n  \t", "test.html");

			HtmlToken token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\t  \n"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.StartTag));
			Assert.That(token.Text, Is.EqualTo("br"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("\n  \t"));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}

		[Test]
		public void EntitiesShouldBeDecodedInText()
		{
			HtmlLexer lexer = new HtmlLexer("  &lt;&amp;&gt;  ", "test.html");
			HtmlToken token = lexer.Next();

			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Text));
			Assert.That(token.Text, Is.EqualTo("  <&>  "));

			token = lexer.Next();
			Assert.That(token.Kind, Is.EqualTo(HtmlTokenKind.Eoi));
		}
	}
}
