using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudNativeShop.Backend.Models
{
    [Table("products")]  // Map to existing 'products' table
    public class Product
    {
        [Key]
        [Column("product_id")]
        public string ProductId { get; set; } = string.Empty;

        [Column("product_name")]
        public string? ProductName { get; set; }
        
        [Column("product_category_name")]
        public string? ProductCategoryName { get; set; }

        [Column("product_description")]
        public string? ProductDescription { get; set; }

        [Column("product_weight_g")]
        public int? ProductWeightG { get; set; }
        
        [Column("product_length_cm")]
        public int? ProductLengthCm { get; set; }
        
        [Column("product_height_cm")]
        public int? ProductHeightCm { get; set; }
        
        [Column("product_width_cm")]
        public int? ProductWidthCm { get; set; }

        [Column("product_price")]
        public decimal? ProductPrice { get; set; }

        [Column("product_stock")]
        public int? ProductStock { get; set; }
    }
}