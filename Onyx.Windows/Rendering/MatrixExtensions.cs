using Onyx.Types;
using SkiaSharp;

namespace Onyx.Windows.Rendering
{
	internal static class MatrixExtensions
	{
		public static SKMatrix ToSKMatrix(this Matrix3x2d matrix)
			=> new SKMatrix(
				(float) matrix.M11, (float) matrix.M12, (float) matrix.M13,
				(float) matrix.M21, (float) matrix.M22, (float) matrix.M23,
				0, 0, 1
			);

		public static SKMatrix ToSKMatrix(this Matrix3d matrix)
			=> new SKMatrix(
				(float)matrix.M11, (float)matrix.M12, (float)matrix.M13,
				(float)matrix.M21, (float)matrix.M22, (float)matrix.M23,
				(float)matrix.M31, (float)matrix.M32, (float)matrix.M33
			);
	}
}
