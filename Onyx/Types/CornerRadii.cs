using System.Diagnostics.CodeAnalysis;

namespace Onyx.Types
{
	public readonly struct CornerRadii : IEquatable<CornerRadii>
	{
		public Vector2d TopLeft { get; }
		public Vector2d TopRight { get; }
		public Vector2d BottomRight { get; }
		public Vector2d BottomLeft { get; }

		private const double Epsilon = 0.000001;

		public bool IsEffectivelyZero =>
			   TopLeft.X < Epsilon && TopLeft.Y < Epsilon
			&& TopRight.X < Epsilon && TopRight.Y < Epsilon
			&& BottomLeft.X < Epsilon && BottomLeft.Y < Epsilon
			&& BottomRight.X < Epsilon && BottomRight.Y < Epsilon;

		public bool IsEffectivelyUniform =>
			   Math.Abs(TopLeft.X - TopRight.X) < Epsilon
			&& Math.Abs(TopLeft.Y - TopRight.Y) < Epsilon
			&& Math.Abs(TopLeft.X - BottomRight.X) < Epsilon
			&& Math.Abs(TopLeft.Y - BottomRight.Y) < Epsilon
			&& Math.Abs(TopLeft.X - BottomLeft.X) < Epsilon
			&& Math.Abs(TopLeft.Y - BottomLeft.Y) < Epsilon;

		public bool IsEffectivelyCircular =>
			   Math.Abs(TopLeft.X - TopLeft.Y) < Epsilon
			&& Math.Abs(TopRight.X - TopRight.Y) < Epsilon
			&& Math.Abs(BottomLeft.X - BottomLeft.Y) < Epsilon
			&& Math.Abs(BottomRight.X - BottomRight.Y) < Epsilon;

		public CornerRadii(double radius)
		{
			TopLeft = new Vector2d(radius);
			TopRight = new Vector2d(radius);
			BottomLeft = new Vector2d(radius);
			BottomRight = new Vector2d(radius);
		}

		public CornerRadii(double topLeft, double topRight, double bottomRight, double bottomLeft)
		{
			TopLeft = new Vector2d(topLeft);
			TopRight = new Vector2d(topRight);
			BottomLeft = new Vector2d(bottomLeft);
			BottomRight = new Vector2d(bottomRight);
		}

		public CornerRadii(Vector2d topLeft, Vector2d topRight, Vector2d bottomRight, Vector2d bottomLeft)
		{
			TopLeft = topLeft;
			TopRight = topRight;
			BottomLeft = bottomLeft;
			BottomRight = bottomRight;
		}

		public bool Equals(CornerRadii other)
			=> TopLeft == other.TopLeft
				&& TopRight == other.TopRight
				&& BottomRight == other.BottomRight
				&& BottomLeft == other.BottomLeft;

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is CornerRadii other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = (hashCode * 65599) + TopLeft.GetHashCode();
				hashCode = (hashCode * 65599) + TopRight.GetHashCode();
				hashCode = (hashCode * 65599) + BottomRight.GetHashCode();
				hashCode = (hashCode * 65599) + BottomLeft.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(CornerRadii a, CornerRadii b)
			=> a.Equals(b);

		public static bool operator !=(CornerRadii a, CornerRadii b)
			=> !a.Equals(b);

		public override string ToString()
			=> $"({TopLeft}, {TopRight}, {BottomRight}, {BottomLeft})";
	}
}
