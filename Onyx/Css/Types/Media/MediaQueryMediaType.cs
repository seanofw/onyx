using System.Linq.Expressions;
using Onyx.Extensions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryMediaType : MediaQuery
	{
		public MediaType Type { get; }

		public MediaQueryMediaType(MediaType type)
			: base(MediaQueryKind.MediaType)
		{
			Type = type;
		}

		public override string ToString()
			=> Type.ToString().Hyphenize();

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Equal(
				Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo),
					_mediaInfo_Type
				),
				Expression.Constant(Type)
			);

		public override bool? Eval(MediaQueryContext context)
			=> context.MediaInfo.Type == Type;
	}
}
