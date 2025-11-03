using System.Diagnostics.CodeAnalysis;

namespace Onyx.Css.Types
{
	public readonly struct SizeConstraints : IEquatable<SizeConstraints>
	{
		private readonly Units _widthUnits, _heightUnits;
		private readonly Units _minWidthUnits, _minHeightUnits;
		private readonly Units _maxWidthUnits, _maxHeightUnits;

		private readonly double _widthValue, _heightValue;
		private readonly double _minWidthValue, _minHeightValue;
		private readonly double _maxWidthValue, _maxHeightValue;

		public Measure Width => new Measure(_widthUnits, _widthValue);
		public Measure Height => new Measure(_heightUnits, _heightValue);
		public Measure MinWidth => new Measure(_minWidthUnits, _minWidthValue);
		public Measure MinHeight => new Measure(_minHeightUnits, _minHeightValue);
		public Measure MaxWidth => new Measure(_maxWidthUnits, _maxWidthValue);
		public Measure MaxHeight => new Measure(_maxHeightUnits, _maxHeightValue);

		public static SizeConstraints Default { get; } = new SizeConstraints(
			PseudoMeasures.Auto, PseudoMeasures.Auto,
			Measure.Zero, Measure.Zero,
			PseudoMeasures.None, PseudoMeasures.None);

		public SizeConstraints(Measure width, Measure height,
			Measure minWidth, Measure minHeight,
			Measure maxWidth, Measure maxHeight)
		{
			_widthUnits = width.Units;
			_heightUnits = height.Units;
			_minWidthUnits = minWidth.Units;
			_minHeightUnits = minHeight.Units;
			_maxWidthUnits = maxWidth.Units;
			_maxHeightUnits = maxHeight.Units;

			_widthValue = width.Value;
			_heightValue = height.Value;
			_minWidthValue = minWidth.Value;
			_minHeightValue = minHeight.Value;
			_maxWidthValue = maxWidth.Value;
			_maxHeightValue = maxHeight.Value;
		}

		public SizeConstraints WithWidth(Measure width)
			=> new SizeConstraints(width, Height, MinWidth, MinHeight, MaxWidth, MaxHeight);
		public SizeConstraints WithHeight(Measure height)
			=> new SizeConstraints(Width, height, MinWidth, MinHeight, MaxWidth, MaxHeight);
		public SizeConstraints WithMinWidth(Measure minWidth)
			=> new SizeConstraints(Width, Height, minWidth, MinHeight, MaxWidth, MaxHeight);
		public SizeConstraints WithMinHeight(Measure minHeight)
			=> new SizeConstraints(Width, Height, MinWidth, minHeight, MaxWidth, MaxHeight);
		public SizeConstraints WithMaxWidth(Measure maxWidth)
			=> new SizeConstraints(Width, Height, MinWidth, MinHeight, maxWidth, MaxHeight);
		public SizeConstraints WithMaxHeight(Measure maxHeight)
			=> new SizeConstraints(Width, Height, MinWidth, MinHeight, MaxWidth, maxHeight);

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is SizeConstraints other && Equals(other);

		public bool Equals(SizeConstraints other)
			=> _widthUnits == other._widthUnits
				&& _heightUnits == other._heightUnits
				&& _minWidthUnits == other._minWidthUnits
				&& _minHeightUnits == other._minHeightUnits
				&& _maxWidthUnits == other._maxWidthUnits
				&& _maxHeightUnits == other._maxHeightUnits

				&& _widthValue == other._widthValue
				&& _heightValue == other._heightValue
				&& _minWidthValue == other._minWidthValue
				&& _minHeightValue == other._minHeightValue
				&& _maxWidthValue == other._maxWidthValue
				&& _maxHeightValue == other._maxHeightValue;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;

				hashCode = hashCode * 65599 + (int)_widthUnits;
				hashCode = hashCode * 65599 + (int)_heightUnits;
				hashCode = hashCode * 65599 + (int)_minWidthUnits;
				hashCode = hashCode * 65599 + (int)_minHeightUnits;
				hashCode = hashCode * 65599 + (int)_maxWidthUnits;
				hashCode = hashCode * 65599 + (int)_maxHeightUnits;

				hashCode = hashCode * 65599 + _widthValue.GetHashCode();
				hashCode = hashCode * 65599 + _heightValue.GetHashCode();
				hashCode = hashCode * 65599 + _minWidthValue.GetHashCode();
				hashCode = hashCode * 65599 + _minHeightValue.GetHashCode();
				hashCode = hashCode * 65599 + _maxWidthValue.GetHashCode();
				hashCode = hashCode * 65599 + _maxHeightValue.GetHashCode();

				return hashCode;
			}
		}

		public static bool operator ==(SizeConstraints a, SizeConstraints b)
			=> a.Equals(b);

		public static bool operator !=(SizeConstraints a, SizeConstraints b)
			=> !a.Equals(b);

		public override string ToString()
			=> $"{Width} x {Height}, min {MinWidth} x {MinHeight}, max {MaxWidth} x {MaxHeight}";

	}
}
