
namespace Onyx.Css.Properties
{
	/// <summary>
	/// What happens when a specific property on an element's style changes.  Each
	/// higher entry here implies that the lower entries are included as part of its
	/// processing.  So "Measure" includes not just a "Measure" phase but also a
	/// "Layout" and "Paint".
	/// </summary>
	public enum UpdateBehavior : byte
	{
		/// <summary>
		/// Changes to this property do not affect presentation.
		/// </summary>
		None = 0,

		/// <summary>
		/// Changes to this property only require an element or a box to be repainted,
		/// and do not involve layout updates or recreation of the box tree.
		/// </summary>
		Paint = 1,

		/// <summary>
		/// Changes to this property require its children to be re-laid-out and then
		/// it and its children should be repainted.
		/// </summary>
		Layout = 2,

		/// <summary>
		/// Changes to this property affect an element or a box's dimensions, so that
		/// it and its descendants and ancesors must be re-measured, then re-laid-out,
		/// and then repainted.
		/// </summary>
		Measure = 3,

		/// <summary>
		/// Changes to this property require an element's boxes to be completely
		/// recreated, then it and its descendants and ancestors must be re-measured,
		/// then re-laid-out, and finally repainted.
		/// </summary>
		FullRefresh = 4,
	}
}
