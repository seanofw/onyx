using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Onyx.Css.Types.Media
{
	/// <summary>
	/// A query against a document's media, represented as a tree of feature comparisons.
	/// This class is the base "node" type from which all of the other more specific "media query"
	/// objects inherit.
	/// </summary>
	public abstract class MediaQuery
	{
		/// <summary>
		/// What kind of "media query node" this class is.
		/// </summary>
		public MediaQueryKind Kind { get; }

		/// <summary>
		/// Whether this subtree requires the media's dimensions (width/height/aspect/orientation)
		/// to be able to be resolved.  Media queries that do not require dimensions can be calculated
		/// once, permanently; while media queries that require dimensions may need to be reevaluated
		/// if the media's dimensions change.
		/// </summary>
		public bool UsesDimensions { get; }

		/// <summary>
		/// Whether this subtree has error nodes in it.
		/// </summary>
		public bool HasErrors { get; }

		/// <summary>
		/// An evaluator for the current media context.
		/// </summary>
		private Func<MediaQueryContext, bool?>? _eval;

		/// <summary>
		/// Construct a new media query.
		/// </summary>
		/// <param name="kind">What kind of "media query node" this class is.</param>
		/// <param name="usesDimensions">Whether this subtree requires the media's dimensions
		/// (width/height/aspect/orientation) to be able to be resolved.  Media queries that do not
		/// require dimensions can be calculated once, permanently; while media queries that require
		/// dimensions may need to be reevaluated if the media's dimensions change.</param>
		/// <param name="hasErrors">Whether this subtree has error nodes in it.</param>
		protected MediaQuery(MediaQueryKind kind, bool usesDimensions, bool hasErrors)
		{
			Kind = kind;
			UsesDimensions = usesDimensions;
			HasErrors = hasErrors;
		}

		/// <summary>
		/// Write a representation of this media query to a string.  This attempts to match
		/// the original CSS where possible.
		/// </summary>
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			ToString(stringBuilder);
			return stringBuilder.ToString();
		}

		/// <summary>
		/// Write a representation of this media query to a string.  This attempts to match
		/// the original CSS where possible.
		/// </summary>
		/// <param name="dest">The destination StringBuilder to write this media query to.</param>
		public abstract void ToString(StringBuilder dest);

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

		/// <summary>
		/// Get a compiled function that can quickly evaluate this media query within its
		/// current media context.
		/// </summary>
		/// <returns>An evaluator function, constructed once and cached.</returns>
		public Func<MediaQueryContext, bool?> GetEval()
		{
			if (_eval != null)
				return _eval;

			ParameterExpression param = Expression.Parameter(typeof(MediaQueryContext), "x");
			Expression body = GetExpression(param);
			Expression<Func<MediaQueryContext, bool?>> evalExpr = Expression.Lambda<Func<MediaQueryContext, bool?>>(body, param);

			_eval = evalExpr.Compile();
			return _eval;
		}
	}
}
