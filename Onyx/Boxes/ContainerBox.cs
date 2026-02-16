namespace Onyx.Boxes
{
	/// <summary>
	/// A container box can hold multiple children.
	/// </summary>
	public abstract class ContainerBox : Box
	{
		/// <summary>
		/// The collection of children.
		/// </summary>
		public IReadOnlyList<Box> Children => _children;
		private List<Box> _children = new List<Box>();

		/// <summary>
		/// The collection of children, as a mutable data structure.
		/// </summary>
		internal List<Box> ChildrenMutable => _children;
	}
}
