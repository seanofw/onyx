
namespace Onyx.Html.Dom
{
	[Serializable]
	internal class HierarchyException : Exception
	{
		public HierarchyException()
		{
		}

		public HierarchyException(string? message) : base(message)
		{
		}

		public HierarchyException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}