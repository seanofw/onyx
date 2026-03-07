using Onyx.Types;

namespace Onyx.Rendering
{
	public interface IClipper : IDisposable
	{
		IClipper Union(IEnumerable<IClipper> others);
		IClipper Intersect(IEnumerable<IClipper> others);
		IClipper Transform(Matrix3x2d transform);
	}
}
