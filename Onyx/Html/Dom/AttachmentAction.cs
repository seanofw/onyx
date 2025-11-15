namespace Onyx.Html.Dom
{
	public delegate void AttachEventHandler(AttachmentAction action, ContainerNode parent);

	/// <summary>
	/// What kind of action is being performed on this node relative to its parent.
	/// </summary>
	public enum AttachmentAction
	{
		/// <summary>
		/// No action (default).
		/// </summary>
		None = 0,

		/// <summary>
		/// This node is about to be attached to a parent node container.
		/// </summary>
		Attaching,

		/// <summary>
		/// This node has been attached to a parent node container.
		/// </summary>
		Attached,

		/// <summary>
		/// This node is about to be detached from a parent node container.
		/// </summary>
		Detaching,

		/// <summary>
		/// This node has been detached from a parent node container.
		/// </summary>
		Detached,
	}
}
