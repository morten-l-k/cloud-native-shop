using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudNativeShop.Backend.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [Column("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        [Column("order_status")]
        public string? OrderStatus { get; set; }

        [Column("order_purchase_timestamp")]
        public DateTime? OrderPurchaseTimestamp { get; set; }

        [Column("order_approved_at")]
        public DateTime? OrderApprovedAt { get; set; }

        [Column("order_delivered_carrier_date")]
        public DateTime? OrderDeliveredCarrierDate { get; set; }

        [Column("order_delivered_customer_date")]
        public DateTime? OrderDeliveredCustomerDate { get; set; }

        [Column("order_estimated_delivery_date")]
        public DateTime? OrderEstimatedDeliveryDate { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = [];
    }
}
