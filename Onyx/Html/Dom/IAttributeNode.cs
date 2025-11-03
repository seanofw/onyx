namespace Onyx.Html.Dom
{
	public interface IAttributeNode
	{
		string Id { get; }
		string ClassName { get; }
		IReadOnlySet<string> ClassNames { get; }
		NamedNodeMap Attributes { get; }
	}
}