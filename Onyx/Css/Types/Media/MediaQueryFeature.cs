using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Onyx.Extensions;

namespace Onyx.Css.Types.Media
{
	public sealed class MediaQueryFeature : MediaQuery
	{
		public MediaFeature Feature { get; }

		public MediaQueryFeature(MediaFeature feature)
			: base(MediaQueryKind.Feature, GetHasDimensions(feature), hasErrors: false)
		{
			Feature = feature;
		}

		public override bool? Eval(MediaQueryContext context)
			=> ResolveValue(GetValue(Feature, context));

		public override Expression GetExpression(ParameterExpression param)
			=> Expression.Call(_resolveValue, GetExpression(Feature, param));

		private static bool? ResolveValue(object? value)
			=> value is double d ? d != 0
				: value is Measure m && m.Units != Units.None ? m.Value != 0
				: null;

		private static MethodInfo _resolveValue = typeof(MediaQueryFeature).GetMethod(nameof(ResolveValue),
			BindingFlags.NonPublic | BindingFlags.Static)!;

		public static Expression GetExpression(MediaFeature feature, ParameterExpression param)
			=> feature switch
			{
				MediaFeature.Width => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaDimensions), _mediaDimensions_Width),
				MediaFeature.Height => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaDimensions), _mediaDimensions_Height),
				MediaFeature.AspectRatio => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaDimensions), _mediaDimensions_AspectRatio),
				MediaFeature.Orientation => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaDimensions), _mediaDimensions_Orientation),

				MediaFeature.Resolution => Expression.Constant(null),
				MediaFeature.Scan => Expression.Constant(null),
				MediaFeature.Grid => Expression.Constant(0),

				MediaFeature.Update => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_UpdateMode),
				MediaFeature.OverflowBlock => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_OverflowBlock),
				MediaFeature.OverflowInline => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_OverflowInline),
				MediaFeature.Color => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_Color),
				MediaFeature.ColorIndex => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_ColorIndex),
				MediaFeature.Monochrome => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_Monochrome),
				MediaFeature.ColorGamut => Expression.Constant(null),
				MediaFeature.Pointer => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_PointerKind),
				MediaFeature.Hover => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_HoverKind),
				MediaFeature.AnyPointer => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_PointerKind),
				MediaFeature.AnyHover => Expression.MakeMemberAccess(
					Expression.MakeMemberAccess(param, _mediaInfo), _mediaInfo_HoverKind),

				_ => throw new NotSupportedException(),
			};

		public static object? GetValue(MediaFeature feature, MediaQueryContext context)
			=> feature switch
			{
				MediaFeature.Width => context.MediaDimensions.Width,
				MediaFeature.Height => context.MediaDimensions.Height,
				MediaFeature.AspectRatio => context.MediaDimensions.AspectRatio,
				MediaFeature.Orientation => context.MediaDimensions.Orientation,

				MediaFeature.Resolution => null,
				MediaFeature.Scan => null,
				MediaFeature.Grid => 0,

				MediaFeature.Update => context.MediaInfo.UpdateMode,
				MediaFeature.OverflowBlock => context.MediaInfo.OverflowBlock,
				MediaFeature.OverflowInline => context.MediaInfo.OverflowInline,
				MediaFeature.Color => context.MediaInfo.Color,
				MediaFeature.ColorIndex => context.MediaInfo.ColorIndex,
				MediaFeature.Monochrome => context.MediaInfo.Monochrome,
				MediaFeature.ColorGamut => null,
				MediaFeature.Pointer => context.MediaInfo.PointerKind,
				MediaFeature.Hover => context.MediaInfo.HoverKind,
				MediaFeature.AnyPointer => context.MediaInfo.PointerKind,
				MediaFeature.AnyHover => context.MediaInfo.HoverKind,

				_ => throw new NotSupportedException(),
			};

		public static Type? GetFeatureType(MediaFeature feature)
			=> (feature & ~(MediaFeature.Min | MediaFeature.Max)) switch
			{
				MediaFeature.Width => typeof(Measure),
				MediaFeature.Height => typeof(Measure),
				MediaFeature.AspectRatio => typeof(double),
				MediaFeature.Orientation => typeof(MediaOrientation),

				MediaFeature.Resolution => null,
				MediaFeature.Scan => null,
				MediaFeature.Grid => null,

				MediaFeature.Update => typeof(MediaUpdateMode),
				MediaFeature.OverflowBlock => typeof(MediaOverflowMode),
				MediaFeature.OverflowInline => typeof(MediaOverflowMode),
				MediaFeature.Color => typeof(double),
				MediaFeature.ColorIndex => typeof(double),
				MediaFeature.Monochrome => typeof(double),
				MediaFeature.ColorGamut => null,
				MediaFeature.Pointer => typeof(MediaPointerKind),
				MediaFeature.Hover => typeof(MediaHoverKind),
				MediaFeature.AnyPointer => typeof(MediaPointerKind),
				MediaFeature.AnyHover => typeof(MediaHoverKind),

				_ => null,
			};

		private static bool GetHasDimensions(MediaFeature feature)
			=> (feature & ~(MediaFeature.Min | MediaFeature.Max)) switch
			{
				MediaFeature.Width => true,
				MediaFeature.Height => true,
				MediaFeature.AspectRatio => true,
				MediaFeature.Orientation => true,
				_ => false,
			};

		public override void ToString(StringBuilder dest)
		{
			dest.Append(Feature.ToString().Hyphenize());
		}
	}
}
