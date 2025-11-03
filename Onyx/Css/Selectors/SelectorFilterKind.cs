namespace Onyx.Css.Selectors
{
	public enum SelectorFilterKind
	{
		None = 0,

		Id,
		Class,

		Attrib = 0x10,

		AttribEq = 0x10,
		AttribIncludes,
		AttribDashMatch,
		AttribStartsWith,
		AttribEndsWith,
		AttribContains,

		HasAttrib = 0x1F,

		CaseSensitive = 0x20,

		CSAttribEq = 0x20,
		CSAttribIncludes,
		CSAttribDashMatch,
		CSAttribStartsWith,
		CSAttribEndsWith,
		CSAttribContains,

		CaseInsensitive = 0x30,

		CIAttribEq = 0x30,
		CIAttribIncludes,
		CIAttribDashMatch,
		CIAttribStartsWith,
		CIAttribEndsWith,
		CIAttribContains,

		Pseudo = 0x40,

		PseudoIs,
		PseudoNot,
		PseudoLang,

		PseudoFirstChild,
		PseudoLastChild,

		PseudoLink,
		PseudoVisited,

		PseudoHover,
		PseudoActive,
		PseudoFocus,
		PseudoEnabled,
		PseudoDisabled,
		PseudoEmpty,

		PseudoChecked,
		PseudoIndeterminate,

		PseudoFirstLine,
		PseudoFirstLetter,
		PseudoAfter,
		PseudoBefore,

		PseudoUnknown,
		PseudoUnknownFunc,
	}
}
