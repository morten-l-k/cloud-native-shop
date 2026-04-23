using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudNativeShop.Backend.Models
{
    [Table("sellers")]
    public class Seller
    {
        [Key]
        [Column("seller_id")]
        public string SellerId { get; set; } = string.Empty;

        [Column("seller_zip_code_prefix")]
        public string? SellerZipCodePrefix { get; set; }

        [Column("seller_city")]
        public string? SellerCity { get; set; }

        [Column("seller_state")]
        public string? SellerState { get; set; }
    }
}
