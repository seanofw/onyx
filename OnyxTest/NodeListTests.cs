using NUnit.Framework;
using Onyx.Html.Dom;

namespace Onyx.Tests
{
	[TestFixture]
	public class NodeListTests
	{
		[Test]
		public void CanCreateAnEmptyNodeList()
		{
			NodeList<Node> nodes = new NodeList<Node>();
			Assert.That(nodes.Count, Is.EqualTo(0));
			Assert.That(nodes.Count(), Is.EqualTo(0));
		}

		private static List<Node> EnumerateNodes(NodeList<Node> nodeList)
		{
			List<Node> nodes = new List<Node>();

			foreach (Node node in nodeList)
			{
				nodes.Add(node);
			}

			return nodes;
		}

		[Test]
		public void CanAddNodesToAListIndividually1()
		{
			NodeList<Node> nodes = new NodeList<Node>();

			Element p;
			nodes.Add(p = new Element("p"));

			Assert.That(nodes.Count, Is.EqualTo(1));
			Assert.That(nodes.Count(), Is.EqualTo(1));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { p };

			Assert.That(collection.Count, Is.EqualTo(1));
			Assert.That(other.Count, Is.EqualTo(1));
			Assert.That(ReferenceEquals(collection[0], other[0]));

			Assert.That(ReferenceEquals(nodes[0], p), Is.True);
		}

		[Test]
		public void CanAddNodesToAListIndividually2()
		{
			NodeList<Node> nodes = new NodeList<Node>();

			Element p, b;
			nodes.Add(p = new Element("p"));
			nodes.Add(b = new Element("b"));

			Assert.That(nodes.Count, Is.EqualTo(2));
			Assert.That(nodes.Count(), Is.EqualTo(2));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { p, b };

			Assert.That(collection.Count, Is.EqualTo(2));
			Assert.That(other.Count, Is.EqualTo(2));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));

			Assert.That(ReferenceEquals(nodes[0], p), Is.True);
			Assert.That(ReferenceEquals(nodes[1], b), Is.True);
		}

		[Test]
		public void CanAddNodesToAListIndividually3()
		{
			NodeList<Node> nodes = new NodeList<Node>();

			Element p, b, i;
			nodes.Add(p = new Element("p"));
			nodes.Add(b = new Element("b"));
			nodes.Add(i = new Element("i"));

			Assert.That(nodes.Count, Is.EqualTo(3));
			Assert.That(nodes.Count(), Is.EqualTo(3));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { p, b, i };

			Assert.That(collection.Count, Is.EqualTo(3));
			Assert.That(other.Count, Is.EqualTo(3));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));
			Assert.That(ReferenceEquals(collection[2], other[2]));

			Assert.That(ReferenceEquals(nodes[0], p), Is.True);
			Assert.That(ReferenceEquals(nodes[1], b), Is.True);
			Assert.That(ReferenceEquals(nodes[2], i), Is.True);
		}

		[Test]
		public void CanAddNodesToAListInSmallBulk()
		{
			NodeList<Node> nodes = new NodeList<Node>();

			Element p, b, i;
			nodes.AddRange(new Node[]
			{
				p = new Element("p"),
				b = new Element("b"),
				i = new Element("i"),
			});

			Assert.That(nodes.Count, Is.EqualTo(3));
			Assert.That(nodes.Count(), Is.EqualTo(3));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { p, b, i };

			Assert.That(collection.Count, Is.EqualTo(3));
			Assert.That(other.Count, Is.EqualTo(3));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));
			Assert.That(ReferenceEquals(collection[2], other[2]));

			Assert.That(ReferenceEquals(nodes[0], p), Is.True);
			Assert.That(ReferenceEquals(nodes[1], b), Is.True);
			Assert.That(ReferenceEquals(nodes[2], i), Is.True);
		}

		[Test]
		public void CanPrecreateANodeListInSmallBulk()
		{
			Element p, b, i;
			NodeList<Node> nodes = new(
			[
				p = new Element("p"),
				b = new Element("b"),
				i = new Element("i"),
			]);

			Assert.That(nodes.Count, Is.EqualTo(3));
			Assert.That(nodes.Count(), Is.EqualTo(3));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { p, b, i };

			Assert.That(collection.Count, Is.EqualTo(3));
			Assert.That(other.Count, Is.EqualTo(3));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));
			Assert.That(ReferenceEquals(collection[2], other[2]));

			Assert.That(ReferenceEquals(nodes[0], p), Is.True);
			Assert.That(ReferenceEquals(nodes[1], b), Is.True);
			Assert.That(ReferenceEquals(nodes[2], i), Is.True);
		}

		[Test]
		public void CanPrecreateANodeListInLargeBulk()
		{
			Element div1, div2, div3, div4, div5, p, b, i, u;
			NodeList<Node> nodes = new(
			[
				div1 = new Element("div"),
				div2 = new Element("div"),
				div3 = new Element("div"),
				div4 = new Element("div"),
				div5 = new Element("div"),
				p = new Element("p"),
				b = new Element("b"),
				i = new Element("i"),
				u = new Element("u"),
			]);

			Assert.That(nodes.Count, Is.EqualTo(9));
			Assert.That(nodes.Count(), Is.EqualTo(9));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { div1, div2, div3, div4, div5, p, b, i, u };

			Assert.That(collection.Count, Is.EqualTo(9));
			Assert.That(other.Count, Is.EqualTo(9));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));
			Assert.That(ReferenceEquals(collection[2], other[2]));
			Assert.That(ReferenceEquals(collection[3], other[3]));
			Assert.That(ReferenceEquals(collection[4], other[4]));
			Assert.That(ReferenceEquals(collection[5], other[5]));
			Assert.That(ReferenceEquals(collection[6], other[6]));
			Assert.That(ReferenceEquals(collection[7], other[7]));
			Assert.That(ReferenceEquals(collection[8], other[8]));

			Assert.That(ReferenceEquals(nodes[0], div1), Is.True);
			Assert.That(ReferenceEquals(nodes[1], div2), Is.True);
			Assert.That(ReferenceEquals(nodes[2], div3), Is.True);
			Assert.That(ReferenceEquals(nodes[3], div4), Is.True);
			Assert.That(ReferenceEquals(nodes[4], div5), Is.True);
			Assert.That(ReferenceEquals(nodes[5], p), Is.True);
			Assert.That(ReferenceEquals(nodes[6], b), Is.True);
			Assert.That(ReferenceEquals(nodes[7], i), Is.True);
			Assert.That(ReferenceEquals(nodes[8], u), Is.True);
		}

		[Test]
		public void CanAddToANodeListInLargeBulk()
		{
			Element div1, div2, div3, div4, div5, p, b, i, u;
			NodeList<Node> nodes = new NodeList<Node>();

			nodes.AddRange(new Node[]
			{
				div1 = new Element("div"),
				div2 = new Element("div"),
				div3 = new Element("div"),
				div4 = new Element("div"),
				div5 = new Element("div"),
				p = new Element("p"),
				b = new Element("b"),
				i = new Element("i"),
				u = new Element("u"),
			});

			Assert.That(nodes.Count, Is.EqualTo(9));
			Assert.That(nodes.Count(), Is.EqualTo(9));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { div1, div2, div3, div4, div5, p, b, i, u };

			Assert.That(collection.Count, Is.EqualTo(9));
			Assert.That(other.Count, Is.EqualTo(9));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));
			Assert.That(ReferenceEquals(collection[2], other[2]));
			Assert.That(ReferenceEquals(collection[3], other[3]));
			Assert.That(ReferenceEquals(collection[4], other[4]));
			Assert.That(ReferenceEquals(collection[5], other[5]));
			Assert.That(ReferenceEquals(collection[6], other[6]));
			Assert.That(ReferenceEquals(collection[7], other[7]));
			Assert.That(ReferenceEquals(collection[8], other[8]));

			Assert.That(ReferenceEquals(nodes[0], div1), Is.True);
			Assert.That(ReferenceEquals(nodes[1], div2), Is.True);
			Assert.That(ReferenceEquals(nodes[2], div3), Is.True);
			Assert.That(ReferenceEquals(nodes[3], div4), Is.True);
			Assert.That(ReferenceEquals(nodes[4], div5), Is.True);
			Assert.That(ReferenceEquals(nodes[5], p), Is.True);
			Assert.That(ReferenceEquals(nodes[6], b), Is.True);
			Assert.That(ReferenceEquals(nodes[7], i), Is.True);
			Assert.That(ReferenceEquals(nodes[8], u), Is.True);
		}

		[Test]
		public void CanPreSwitchToANodeListDuringBulkUpdates()
		{
			Element div1, div2, div3, div4, div5, p, b, i, u;
			NodeList<Node> nodes = new NodeList<Node>();

			nodes.Add(div1 = new Element("div"));
			nodes.Add(div2 = new Element("div"));

			nodes.AddRange(new Node[]
			{
				div3 = new Element("div"),
				div4 = new Element("div"),
				div5 = new Element("div"),
				p = new Element("p"),
				b = new Element("b"),
				i = new Element("i"),
				u = new Element("u"),
			});

			Assert.That(nodes.Count, Is.EqualTo(9));
			Assert.That(nodes.Count(), Is.EqualTo(9));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { div1, div2, div3, div4, div5, p, b, i, u };

			Assert.That(collection.Count, Is.EqualTo(9));
			Assert.That(other.Count, Is.EqualTo(9));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));
			Assert.That(ReferenceEquals(collection[2], other[2]));
			Assert.That(ReferenceEquals(collection[3], other[3]));
			Assert.That(ReferenceEquals(collection[4], other[4]));
			Assert.That(ReferenceEquals(collection[5], other[5]));
			Assert.That(ReferenceEquals(collection[6], other[6]));
			Assert.That(ReferenceEquals(collection[7], other[7]));
			Assert.That(ReferenceEquals(collection[8], other[8]));

			Assert.That(ReferenceEquals(nodes[0], div1), Is.True);
			Assert.That(ReferenceEquals(nodes[1], div2), Is.True);
			Assert.That(ReferenceEquals(nodes[2], div3), Is.True);
			Assert.That(ReferenceEquals(nodes[3], div4), Is.True);
			Assert.That(ReferenceEquals(nodes[4], div5), Is.True);
			Assert.That(ReferenceEquals(nodes[5], p), Is.True);
			Assert.That(ReferenceEquals(nodes[6], b), Is.True);
			Assert.That(ReferenceEquals(nodes[7], i), Is.True);
			Assert.That(ReferenceEquals(nodes[8], u), Is.True);
		}

		[Test]
		public void CanGrowANodeListFromSmallToLarge()
		{
			Element div1, div2, div3, div4, div5, p, b, i, u;
			NodeList<Node> nodes = new NodeList<Node>();

			nodes.Add(div1 = new Element("div"));
			nodes.Add(div2 = new Element("div"));
			nodes.Add(div3 = new Element("div"));
			nodes.Add(div4 = new Element("div"));
			nodes.Add(div5 = new Element("div"));
			nodes.Add(p = new Element("p"));
			nodes.Add(b = new Element("b"));
			nodes.Add(i = new Element("i"));
			nodes.Add(u = new Element("u"));

			Assert.That(nodes.Count, Is.EqualTo(9));
			Assert.That(nodes.Count(), Is.EqualTo(9));

			List<Node> collection = EnumerateNodes(nodes);
			List<Node> other = new List<Node> { div1, div2, div3, div4, div5, p, b, i, u };

			Assert.That(collection.Count, Is.EqualTo(9));
			Assert.That(other.Count, Is.EqualTo(9));
			Assert.That(ReferenceEquals(collection[0], other[0]));
			Assert.That(ReferenceEquals(collection[1], other[1]));
			Assert.That(ReferenceEquals(collection[2], other[2]));
			Assert.That(ReferenceEquals(collection[3], other[3]));
			Assert.That(ReferenceEquals(collection[4], other[4]));
			Assert.That(ReferenceEquals(collection[5], other[5]));
			Assert.That(ReferenceEquals(collection[6], other[6]));
			Assert.That(ReferenceEquals(collection[7], other[7]));
			Assert.That(ReferenceEquals(collection[8], other[8]));

			Assert.That(ReferenceEquals(nodes[0], div1), Is.True);
			Assert.That(ReferenceEquals(nodes[1], div2), Is.True);
			Assert.That(ReferenceEquals(nodes[2], div3), Is.True);
			Assert.That(ReferenceEquals(nodes[3], div4), Is.True);
			Assert.That(ReferenceEquals(nodes[4], div5), Is.True);
			Assert.That(ReferenceEquals(nodes[5], p), Is.True);
			Assert.That(ReferenceEquals(nodes[6], b), Is.True);
			Assert.That(ReferenceEquals(nodes[7], i), Is.True);
			Assert.That(ReferenceEquals(nodes[8], u), Is.True);
		}
	}
}
