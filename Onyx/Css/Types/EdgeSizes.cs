using System.Diagnostics.CodeAnalysis;

namespace Onyx.Css.Types
{
	public readonly struct EdgeSizes : IEquatable<EdgeSizes>
	{
		private readonly Units _topUnits, _rightUnits, _bottomUnits, _leftUnits;
		private readonly double _topValue, _rightValue, _bottomValue, _leftValue;

		public Measure Top => new Measure(_topUnits, _topValue);
		public Measure Right => new Measure(_rightUnits, _rightValue);
		public Measure Bottom => new Measure(_bottomUnits, _bottomValue);
		public Measure Left => new Measure(_leftUnits, _leftValue);

		public static EdgeSizes Zero { get; } = new EdgeSizes(
			Measure.Zero, Measure.Zero, Measure.Zero, Measure.Zero);

		public EdgeSizes(Measure top, Measure right, Measure bottom, Measure left)
		{
			_topUnits = top.Units;
			_rightUnits = right.Units;
			_bottomUnits = bottom.Units;
			_leftUnits = left.Units;

			_topValue = top.Value;
			_rightValue = right.Value;
			_bottomValue = bottom.Value;
			_leftValue = left.Value;
		}

		public EdgeSizes WithTop(Measure top)
			=> new EdgeSizes(top, Right, Bottom, Left);
		public EdgeSizes WithRight(Measure right)
			=> new EdgeSizes(Top, right, Bottom, Left);
		public EdgeSizes WithBottom(Measure bottom)
			=> new EdgeSizes(Top, Right, bottom, Left);
		public EdgeSizes WithLeft(Measure left)
			=> new EdgeSizes(Top, Right, Bottom, left);

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is EdgeSizes other && Equals(other);

		public bool Equals(EdgeSizes other)
			=> _topUnits == other._topUnits
				&& _rightUnits == other._rightUnits
				&& _bottomUnits == other._bottomUnits
				&& _leftUnits == other._leftUnits
				&& _topValue == other._topValue
				&& _rightValue == other._rightValue
				&& _bottomValue == other._bottomValue
				&& _leftValue == other._leftValue;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = hashCode * 65599 + (int)_topUnits;
				hashCode = hashCode * 65599 + (int)_rightUnits;
				hashCode = hashCode * 65599 + (int)_bottomUnits;
				hashCode = hashCode * 65599 + (int)_leftUnits;
				hashCode = hashCode * 65599 + _topValue.GetHashCode();
				hashCode = hashCode * 65599 + _rightValue.GetHashCode();
				hashCode = hashCode * 65599 + _bottomValue.GetHashCode();
				hashCode = hashCode * 65599 + _leftValue.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(EdgeSizes a, EdgeSizes b)
			=> a.Equals(b);

		public static bool operator !=(EdgeSizes a, EdgeSizes b)
			=> !a.Equals(b);

		public override string ToString()
			=> $"{Top} {Right} {Bottom} {Left}";
	}
}
