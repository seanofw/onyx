using System.Runtime.CompilerServices;
using Onyx.Css.Properties;
using Onyx.Css.Types;

namespace Onyx.Css.Computed
{
	public class ComputedFlexStyle
	{
		private readonly Units _basisUnits;
		public readonly FlexDirection Direction;
		public readonly FlexWrap Wrap;
		public readonly AlignContentKind AlignContent;
		public readonly AlignItemsKind AlignItems;
		public readonly AlignSelfKind AlignSelf;
		public readonly JustifyContentKind JustifyContent;

		private readonly double _basisValue;
		public readonly double Grow;
		public readonly double Shrink;
		public readonly int Order;

		public Measure Basis => new Measure(_basisUnits, _basisValue);

		public static ComputedFlexStyle Default { get; } = new ComputedFlexStyle(
			FlexDirection.Row, FlexWrap.Wrap, AlignContentKind.Stretch,
			AlignItemsKind.Stretch, AlignSelfKind.Auto, JustifyContentKind.FlexStart,
			PseudoMeasures.Auto, grow: 0, shrink: 1, order: 0);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ComputedFlexStyle(
			FlexDirection direction, FlexWrap wrap, AlignContentKind alignContent,
			AlignItemsKind alignItems, AlignSelfKind alignSelf, JustifyContentKind justifyContent,
			Measure basis, double grow, double shrink, int order)
		{
			_basisUnits = basis.Units;
			Direction = direction;
			Wrap = wrap;
			AlignContent = alignContent;
			AlignItems = alignItems;
			AlignSelf = alignSelf;
			JustifyContent = justifyContent;
			_basisValue = basis.Value;
			Grow = grow;
			Shrink = shrink;
			Order = order;
		}

		public ComputedFlexStyle WithDirection(FlexDirection direction)
			=> new ComputedFlexStyle(direction, Wrap, AlignContent,
				AlignItems, AlignSelf, JustifyContent, Basis, Grow, Shrink, Order);
		public ComputedFlexStyle WithWrap(FlexWrap wrap)
			=> new ComputedFlexStyle(Direction, wrap, AlignContent,
				AlignItems, AlignSelf, JustifyContent, Basis, Grow, Shrink, Order);
		public ComputedFlexStyle WithAlignContent(AlignContentKind alignContent)
			=> new ComputedFlexStyle(Direction, Wrap, alignContent,
				AlignItems, AlignSelf, JustifyContent, Basis, Grow, Shrink, Order);
		public ComputedFlexStyle WithAlignItems(AlignItemsKind alignItems)
			=> new ComputedFlexStyle(Direction, Wrap, AlignContent,
				alignItems, AlignSelf, JustifyContent, Basis, Grow, Shrink, Order);
		public ComputedFlexStyle WithAlignSelf(AlignSelfKind alignSelf)
			=> new ComputedFlexStyle(Direction, Wrap, AlignContent,
				AlignItems, alignSelf, JustifyContent, Basis, Grow, Shrink, Order);
		public ComputedFlexStyle WithJustifyContent(JustifyContentKind justifyContent)
			=> new ComputedFlexStyle(Direction, Wrap, AlignContent,
				AlignItems, AlignSelf, justifyContent, Basis, Grow, Shrink, Order);
		public ComputedFlexStyle WithBasis(Measure basis)
			=> new ComputedFlexStyle(Direction, Wrap, AlignContent,
				AlignItems, AlignSelf, JustifyContent, basis, Grow, Shrink, Order);
		public ComputedFlexStyle WithGrow(double grow)
			=> new ComputedFlexStyle(Direction, Wrap, AlignContent,
				AlignItems, AlignSelf, JustifyContent, Basis, grow, Shrink, Order);
		public ComputedFlexStyle WithShrink(double shrink)
			=> new ComputedFlexStyle(Direction, Wrap, AlignContent,
				AlignItems, AlignSelf, JustifyContent, Basis, Grow, shrink, Order);
		public ComputedFlexStyle WithOrder(int order)
			=> new ComputedFlexStyle(Direction, Wrap, AlignContent,
				AlignItems, AlignSelf, JustifyContent, Basis, Grow, Shrink, order);

		public UInt256 Diff(ComputedFlexStyle other)
		{
			if (this == other)
				return default;

			UInt256 bits = default;
			if (Direction != other.Direction)
				bits = bits.SetBit((int)KnownPropertyKind.Direction);
			if (Wrap != other.Wrap)
				bits = bits.SetBit((int)KnownPropertyKind.FlexWrap);
			if (AlignContent != other.AlignContent)
				bits = bits.SetBit((int)KnownPropertyKind.AlignContent);
			if (AlignItems != other.AlignItems)
				bits = bits.SetBit((int)KnownPropertyKind.AlignItems);
			if (AlignSelf != other.AlignSelf)
				bits = bits.SetBit((int)KnownPropertyKind.AlignSelf);
			if (JustifyContent != other.JustifyContent)
				bits = bits.SetBit((int)KnownPropertyKind.JustifyContent);
			if (Basis != other.Basis)
				bits = bits.SetBit((int)KnownPropertyKind.FlexBasis);
			if (Grow != other.Grow)
				bits = bits.SetBit((int)KnownPropertyKind.FlexGrow);
			if (Shrink != other.Shrink)
				bits = bits.SetBit((int)KnownPropertyKind.FlexShrink);
			if (Order != other.Order)
				bits = bits.SetBit((int)KnownPropertyKind.Order);
			return bits;
		}
	}
}
