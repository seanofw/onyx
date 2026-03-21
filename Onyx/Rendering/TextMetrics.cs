using Onyx.Types;

namespace Onyx.Rendering
{
	public readonly struct TextMetrics : IEquatable<TextMetrics>
	{
		#region Properties

		public Rect2d Bounds { get; }
		public Vector2d Advance { get; }

		public Vector2d Offset => Bounds.TopLeft;
		public Size2d Size => Bounds.Size;

		#endregion

		#region Construction

		public TextMetrics(Rect2d bounds, Vector2d advance)
		{
			Advance = advance;
			Bounds = bounds;
		}

		#endregion

		#region Equality and GetHashCode()

		public bool Equals(TextMetrics other)
			=> Bounds == other.Bounds && Advance == other.Advance;

		public override bool Equals(object? obj)
			=> obj is TextMetrics other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = (hashCode * 65599) + Bounds.GetHashCode();
				hashCode = (hashCode * 65599) + Advance.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(TextMetrics left, TextMetrics right)
			=> left.Equals(right);
		public static bool operator !=(TextMetrics left, TextMetrics right)
			=> !left.Equals(right);

		#endregion

		public override string ToString()
			=> $"{Bounds}, advance {Advance}";
	}
}
