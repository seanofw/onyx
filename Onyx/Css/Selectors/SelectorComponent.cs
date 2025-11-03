using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Onyx.Css.Selectors
{
	public readonly struct SelectorComponent : IEquatable<SelectorComponent>
	{
		public Combinator Combinator { get; }
		public SimpleSelector SimpleSelector { get; }

		public SelectorComponent(Combinator combinator, SimpleSelector simpleSelector)
		{
			Combinator = combinator;
			SimpleSelector = simpleSelector;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is SelectorComponent other && Equals(other);

		public bool Equals(SelectorComponent other)
			=> Combinator == other.Combinator
				&& SimpleSelector == other.SimpleSelector;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)Combinator;
				hashCode = hashCode * 65599 + SimpleSelector.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(SelectorComponent a, SelectorComponent b)
			=> a.Equals(b);

		public static bool operator !=(SelectorComponent a, SelectorComponent b)
			=> !a.Equals(b);


		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		public void ToString(StringBuilder stringBuilder)
		{
			switch (Combinator)
			{
				case Combinator.Descendant:
					stringBuilder.Append(" ");
					break;
				case Combinator.AdjacentSibling:
					stringBuilder.Append(" + ");
					break;
				case Combinator.Child:
					stringBuilder.Append(" > ");
					break;
				case Combinator.Self:
					break;
				default:
					stringBuilder.Append(" ? ");
					break;
			}

			SimpleSelector.ToString(stringBuilder);
		}
	}
}
