using System.Runtime.CompilerServices;
using Onyx.Css.Properties.SyntaxDefinitions;

namespace Onyx.Css.Properties
{
	internal abstract class MiniParser
	{
		public Syntax Syntax { get; }
		public Func<object> MakeNew { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MiniParser(Syntax syntax, Func<object> makeNew)
		{
			Syntax = syntax;
			MakeNew = makeNew;
		}
	}

	internal class MiniParser<TProp> : MiniParser
		where TProp : class
	{
		public new Syntax<TProp> Syntax => (Syntax<TProp>)base.Syntax;
		public new Func<TProp> MakeNew => (Func<TProp>)base.MakeNew;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MiniParser(Syntax<TProp> syntax, Func<TProp> makeNew)
			: base(syntax, makeNew)
		{
		}
	}
}
