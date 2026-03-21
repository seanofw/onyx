using Onyx.Types;

namespace Onyx.Rendering
{
	/// <summary>
	/// A composable clip region. Pixels outside the region are not rendered. Clippers
	/// are combined with Union and Intersect and repositioned with Transform; all
	/// operations produce new instances without modifying the originals.
	/// </summary>
	public interface IClipper : IDisposable
	{
		/// <summary>
		/// Return a new clipper that is the boolean union of this clipper and all others.
		/// A pixel passes if it lies inside this clipper or inside any of the others.
		/// </summary>
		/// <param name="others">
		/// The clippers to union with. Must be from the same backend as this clipper;
		/// mixing backends throws. An empty sequence returns a copy of this clipper.
		/// </param>
		/// <returns>A new clipper representing the union. The caller owns and must dispose it.</returns>
		IClipper Union(IEnumerable<IClipper> others);

		/// <summary>
		/// Return a new clipper that is the boolean intersection of this clipper and all
		/// others. A pixel passes only if it lies inside this clipper and every other.
		/// </summary>
		/// <param name="others">
		/// The clippers to intersect with. Must be from the same backend as this clipper;
		/// mixing backends throws. An empty sequence returns a copy of this clipper.
		/// </param>
		/// <returns>A new clipper representing the intersection. The caller owns and must dispose it.</returns>
		IClipper Intersect(IEnumerable<IClipper> others);

		/// <summary>
		/// Return a new clipper with this region transformed by an affine matrix.
		/// </summary>
		/// <param name="transform">
		/// The affine transform to apply. Typically the same transform applied to the
		/// element being clipped, so the clip region moves and scales with it.
		/// </param>
		/// <returns>A new transformed clipper. The caller owns and must dispose it.</returns>
		IClipper Transform(Matrix3x2d transform);
	}
}
