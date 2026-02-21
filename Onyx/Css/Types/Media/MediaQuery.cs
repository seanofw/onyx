using System.Linq.Expressions;
using System.Reflection;

namespace Onyx.Css.Types.Media
{
	public abstract class MediaQuery
	{
		public MediaQueryKind Kind { get; }

		protected MediaQuery(MediaQueryKind kind)
		{
			Kind = kind;
		}

		public override string ToString()
			=> $"[{Kind}]";

		protected static PropertyInfo _mediaInfo = typeof(MediaQueryContext)
			.GetProperty(nameof(MediaQueryContext.MediaInfo), BindingFlags.Public | BindingFlags.Instance)!;

		protected static PropertyInfo _mediaInfo_Type = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.Type), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_UpdateMode = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.UpdateMode), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_Color = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.Color), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_Monochrome = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.Monochrome), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_ColorIndex = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.ColorIndex), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_OverflowInline = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.OverflowInline), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_OverflowBlock = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.OverflowBlock), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_PointerKind = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.PointerKind), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaInfo_HoverKind = typeof(MediaInfo)
			.GetProperty(nameof(MediaInfo.HoverKind), BindingFlags.Public | BindingFlags.Instance)!;

		protected static PropertyInfo _mediaDimensions = typeof(MediaQueryContext)
			.GetProperty(nameof(MediaQueryContext.MediaDimensions), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaDimensions_Width = typeof(MediaDimensions)
			.GetProperty(nameof(MediaDimensions.Width), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaDimensions_Height = typeof(MediaDimensions)
			.GetProperty(nameof(MediaDimensions.Height), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaDimensions_AspectRatio = typeof(MediaDimensions)
			.GetProperty(nameof(MediaDimensions.AspectRatio), BindingFlags.Public | BindingFlags.Instance)!;
		protected static PropertyInfo _mediaDimensions_Orientation = typeof(MediaDimensions)
			.GetProperty(nameof(MediaDimensions.Orientation), BindingFlags.Public | BindingFlags.Instance)!;

		public abstract Expression GetExpression(ParameterExpression param);
		public abstract bool? Eval(MediaQueryContext context);
	}
}
