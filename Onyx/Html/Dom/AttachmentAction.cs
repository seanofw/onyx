namespace Onyx.Html.Dom
{
	public delegate void AttachEventHandler(AttachmentAction action, ContainerNode parent);

	public enum AttachmentAction
	{
		None = 0,

		Attaching,
		Attached,
		Detaching,
		Detached,
	}
}
