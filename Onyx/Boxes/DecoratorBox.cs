namespace Onyx.Boxes
{
	/// <summary>
	/// A decorator box holds exactly one child box.
	/// </summary>
	public abstract class DecoratorBox : Box
	{
		/// <summary>
		/// The single child.
		/// </summary>
		public Box? Child { get; internal set; }
	}
}
