using System.Linq.Expressions;
using System.Reflection;
using Onyx.Html.Dom;

namespace Onyx.Css.Selectors
{
	public class SelectorPseudoStyleFlag : SelectorFilterPseudoClass
	{
		protected override string PseudoClassName => _pseudoClassName;
		private readonly string _pseudoClassName;

		public StyleFlags Mask { get; }
		public StyleFlags Match { get; }

		private SelectorPseudoStyleFlag(SelectorFilterKind kind, string pseudoClassName,
			StyleFlags mask, StyleFlags match)
			: base(kind)
		{
			_pseudoClassName = pseudoClassName;
			Mask = mask;
			Match = match;
		}

		private static readonly FieldInfo _styleFlagsProperty =
			typeof(Node).GetField(nameof(Node.StyleFlags), BindingFlags.Instance | BindingFlags.NonPublic)!;

		public override Expression GetMatchExpression(ParameterExpression element)
			=> Expression.Equal(
				Expression.And(
					Expression.MakeMemberAccess(Expression.Convert(element, typeof(Node)), _styleFlagsProperty),
					Expression.Constant(Mask)),
				Expression.Constant(Match));

		public override bool IsMatch(Element element)
			=> (element.StyleFlags & Mask) == Match;

		public static SelectorPseudoStyleFlag Link { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoLink, "link", StyleFlags.Visited, default);
		public static SelectorPseudoStyleFlag Visited { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoVisited, "visited", StyleFlags.Visited, StyleFlags.Visited);
		public static SelectorPseudoStyleFlag Hover { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoHover, "hover", StyleFlags.Hover, default);
		public static SelectorPseudoStyleFlag Active { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoActive, "active", StyleFlags.Active, default);
		public static SelectorPseudoStyleFlag Focus { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoFocus, "focus", StyleFlags.Focus, default);
		public static SelectorPseudoStyleFlag Enabled { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoDisabled, "enabled", StyleFlags.Disabled, default);
		public static SelectorPseudoStyleFlag Disabled { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoDisabled, "disabled", StyleFlags.Disabled, StyleFlags.Disabled);
		public static SelectorPseudoStyleFlag Checked { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoChecked, "checked", StyleFlags.Checked | StyleFlags.Indeterminate, StyleFlags.Checked);
		public static SelectorPseudoStyleFlag Indeterminate { get; }
			= new SelectorPseudoStyleFlag(SelectorFilterKind.PseudoIndeterminate, "indeterminate", StyleFlags.Checked | StyleFlags.Indeterminate, StyleFlags.Indeterminate);

	}
}
