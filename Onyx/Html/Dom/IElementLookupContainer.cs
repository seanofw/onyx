
namespace Onyx.Html.Dom
{
	internal interface IElementLookupContainer
	{
		void AddDescendant(Element element);
		void RemoveDescendant(Element element);

		ElementLookupTables ElementLookupTables { get; }

		IReadOnlyCollection<Element> GetElementsById(string id);
		IReadOnlyCollection<Element> GetElementsByName(string name);
		IReadOnlyCollection<Element> GetElementsByClassname(string classname);
		IReadOnlyCollection<Element> GetElementsByType(string type);
		IReadOnlyCollection<Element> GetElementsByTypeAttribute(string value);
	}
}
