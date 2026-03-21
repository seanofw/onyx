using Onyx.Rendering;
using Onyx.Types;
using SkiaSharp;

namespace Onyx.Skia
{
	public class SkiaClipper : SkiaDisposable, IClipper
	{
		public SKPath Path { get; private set; }

		public SkiaClipper(ReadOnlySpan<Vector2d> path)
		{
			SKPoint[] points = new SKPoint[path.Length];
			for (int i = 0; i < path.Length; i++)
				points[i] = new SKPoint((float)path[i].X, (float)path[i].Y);

			Path = new SKPath();
			Path.AddPoly(points);
		}

		public SkiaClipper(SKPath path)
		{
			Path = path;
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				Path?.Dispose();
				Path = null!;
			}
		}

		public IClipper Union(IEnumerable<IClipper> others)
		{
			using SKPath.OpBuilder opBuilder = new SKPath.OpBuilder();

			foreach (IClipper other in others)
			{
				if (other is not SkiaClipper skiaClipper)
					throw new ArgumentException($"This method only works with a SkiaClipper, not a {other.GetType().Name}");

				opBuilder.Add(skiaClipper.Path, SKPathOp.Union);
			}

			SKPath result = new SKPath();
			opBuilder.Resolve(result);

			return new SkiaClipper(result);
		}

		public IClipper Intersect(IEnumerable<IClipper> others)
		{
			using SKPath.OpBuilder opBuilder = new SKPath.OpBuilder();

			foreach (IClipper other in others)
			{
				if (other is not SkiaClipper skiaClipper)
					throw new ArgumentException($"This method only works with a SkiaClipper, not a {other.GetType().Name}");

				opBuilder.Add(skiaClipper.Path, SKPathOp.Intersect);
			}

			SKPath result = new SKPath();
			opBuilder.Resolve(result);

			return new SkiaClipper(result);
		}

		public IClipper Transform(Matrix3x2d transform)
		{
			SKPath result = new SKPath(Path);

			result.Transform(new SKMatrix(
				(float)transform.M11, (float)transform.M12, (float)transform.M13,
				(float)transform.M21, (float)transform.M22, (float)transform.M23,
				0, 0, 1
			));

			return new SkiaClipper(result);
		}
	}
}
