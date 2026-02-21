using System.Linq.Expressions;
using System.Text;
using Onyx.Extensions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryMediaType : MediaQuery
	{
		public MediaType Type { get; }

		public MediaQueryMediaType(MediaType type)
			: base(MediaQueryKind.MediaType, usesDimensions: false, hasErrors: false)
		{
			Type = type;
		}

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

		public override void ToString(StringBuilder dest)
		{
			dest.Append(Type.ToString().Hyphenize());
		}
	}
}
