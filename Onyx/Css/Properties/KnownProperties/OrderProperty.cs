using System.Globalization;
using Onyx.Css.Computed;

namespace Onyx.Css.Properties.KnownProperties
{
    public sealed record class OrderProperty : StyleProperty
    {
        public int Order { get; init; }

		public override ComputedStyle Apply(ComputedStyle style)
			=> style.WithOrder(Order);

		public override ComputedStyle CopyProperty(ComputedStyle dest, ComputedStyle source)
			=> dest.WithOrder(source.Order);

		public override string ToString()
            => Order.ToString(CultureInfo.InvariantCulture);
	}
}
