using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace CloudNativeShop.Backend.Models
{
    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        [Column("order_item_id")]
        public int OrderItemId { get; set; }

        [Column("order_id")]
        public string OrderId { get; set; } = string.Empty;

        [Column("order_item_quantity")]
        public int? OrderItemQuantity { get; set; }

        [Column("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [Column("shipping_limit_date")]
        public DateTime? ShippingLimitDate { get; set; }

        [Column("price")]
        public decimal? Price { get; set; }

        [Column("freight_value")]
        public decimal? FreightValue { get; set; }

        [JsonIgnore]
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }
    }
}
