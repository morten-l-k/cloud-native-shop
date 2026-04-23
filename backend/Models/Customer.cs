using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudNativeShop.Backend.Models
{
    [Table("customers")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public string CustomerId { get; set; } = string.Empty;

        [Column("customer_unique_id")]
        public string? CustomerUniqueId { get; set; }

        [Column("customer_zip_code_prefix")]
        public string? CustomerZipCodePrefix { get; set; }

        [Column("customer_city")]
        public string? CustomerCity { get; set; }

        [Column("customer_state")]
        public string? CustomerState { get; set; }
    }
}
