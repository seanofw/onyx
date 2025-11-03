using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Onyx.Css.Parsing;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class CompoundSelector : IEquatable<CompoundSelector>
	{
		public IReadOnlyList<Selector> Selectors => _selectors;
		private readonly Selector[] _selectors;

		public Specificity Specificity => _specificity ??= CalculateSpecificity();
		private Specificity? _specificity;

		private static readonly CssSelectorParser _cssParser = new CssSelectorParser();

		public CompoundSelector(params Selector[] selectors)
			: this((IEnumerable<Selector>)selectors)
		{
		}

		public CompoundSelector(IEnumerable<Selector> selectors)
		{
			_selectors = selectors?.ToArray() ?? Array.Empty<Selector>();
		}

		private CompoundSelector(Selector[] selectors, Specificity specificity)
		{
			_selectors = selectors?.ToArray() ?? Array.Empty<Selector>();
			_specificity = specificity;
		}

		public CompoundSelector AsStylesheetRule(int stylesheet, int ruleIndex)
			=> new CompoundSelector(_selectors,
				Specificity.WithoutLocation() | new Specificity(stylesheet: stylesheet, ruleIndex: ruleIndex));

		public CompoundSelector AsInlineStyle(int stylesheet, int ruleIndex)
			=> new CompoundSelector(_selectors,
				Specificity.WithoutLocation() | new Specificity(isInlineStyle: true));

		private Specificity CalculateSpecificity()
		{
			if (_selectors.Length == 0)
				return Specificity.Zero;
			else if (_selectors.Length == 1)
				return _selectors[0].Specificity;
			else
			{
				Specificity specificity = Specificity.Zero;
				foreach (Selector selector in _selectors)
					specificity += selector.Specificity;
				return specificity;
			}
		}

		public bool IsMatch(Node? node)
		{
			if (!(node is Element element))
				return false;

			foreach (Selector selector in Selectors)
			{
				if (selector.IsMatch(element))
					return true;
			}
			return false;
		}

		/// <summary>
		/// Find all elements at or under the given root node that match this compound selector,
		/// and add them to the given result set.
		/// </summary>
		/// <param name="root">The root of the subtree to search under.</param>
		/// <returns>The result set.  No guarantees are made as to the order in
		/// which the elements will be returned, only that the correct elements will be
		/// returned.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the root is null, or if
		/// it is somehow malformed such that it does not have an actual Root of its own.</exception>
		public IReadOnlySet<Element> Find(Node root)
		{
			HashSet<Element> result = new HashSet<Element>();
			foreach (Selector selector in Selectors)
			{
				selector.Find(root, result);
			}
			return result;
		}

		/// <summary>
		/// Find all elements at or under the given root node that match this compound selector,
		/// and add them to the given result set.
		/// </summary>
		/// <param name="root">The root of the subtree to search under.</param>
		/// <param name="result">The result set to append to.  No guarantees are made
		/// as to the order in which the elements will be added, only that the correct
		/// elements will be added.</param>
		/// <exception cref="ArgumentNullException">Thrown if the root is null, or if
		/// it is somehow malformed such that it does not have an actual Root of its own.</exception>
		public int Find(Node root, ISet<Element> result)
		{
			int count = 0;
			foreach (Selector selector in Selectors)
			{
				count += selector.Find(root, result);
			}
			return count;
		}

		/// <summary>
		/// Find the closest ancestor (including this node) that matches this compound selector.
		/// </summary>
		/// <param name="node">The starting node.</param>
		/// <returns>The closest matching ancestor element, or null if no ancestors match.</returns>
		public Element? Closest(Node node)
		{
			foreach (Selector selector in Selectors)
			{
				Element? ancestor = selector.Closest(node);
				if (ancestor != null)
					return ancestor;
			}
			return null;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is Selector other && Equals(other);

		public bool Equals(CompoundSelector? other)
			=> ReferenceEquals(other, null) ? false
				: ReferenceEquals(this, other) ? true
				: _selectors.SequenceEqual(other._selectors);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				foreach (Selector selector in _selectors)
					hashCode = hashCode * 65599 + selector.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(CompoundSelector? a, CompoundSelector? b)
			=> ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);

		public static bool operator !=(CompoundSelector? a, CompoundSelector? b)
			=> ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !a.Equals(b);

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		public void ToString(StringBuilder stringBuilder)
		{
			bool isFirst = true;
			foreach (Selector selector in _selectors)
			{
				if (!isFirst)
					stringBuilder.Append(", ");
				selector.ToString(stringBuilder);
				isFirst = false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CompoundSelector Parse(string text)
			=> _cssParser.ParseCompoundSelector(new CssLexer(text, "<no source>"),
				expectEoi: true, throwOnError: true)!;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CompoundSelector Parse(CssLexer lexer)
			=> _cssParser.ParseCompoundSelector(lexer,
				expectEoi: true, throwOnError: true)!;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string text, [MaybeNullWhen(false)] out CompoundSelector selector)
			=> (selector = _cssParser.ParseCompoundSelector(new CssLexer(text, "<no source>"))) != null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(CssLexer lexer, [MaybeNullWhen(false)] out CompoundSelector selector)
			=> (selector = _cssParser.ParseCompoundSelector(lexer)) != null;

		public object SelectMany()
		{
			throw new NotImplementedException();
		}
	}
}
