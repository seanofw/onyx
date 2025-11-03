using System.Runtime.CompilerServices;

namespace Onyx.Css.Parsing
{
	public readonly struct CssLexerPosition
	{
		internal readonly int Ptr;
		internal readonly int Line;
		internal readonly int LineStart;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal CssLexerPosition(int ptr, int line, int lineStart)
		{
			Ptr = ptr;
			Line = line;
			LineStart = lineStart;
		}
	}
}