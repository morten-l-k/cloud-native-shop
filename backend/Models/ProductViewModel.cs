using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudNativeShop.Backend.Models
{
    [Table("products")]  // Map to existing 'products' table
    public class ProductViewModel
    {
        [Key]
        [Column("product_id")]
        public string ProductId { get; set; } = string.Empty;
        
        [Column("product_category_name")]
        public string? ProductCategoryName { get; set; }
        
        [Column("product_name_length")]
        public int? ProductNameLength { get; set; }
        
        [Column("product_description_length")]
        public int? ProductDescriptionLength { get; set; }
        
        [Column("product_photos_qty")]
        public int? ProductPhotosQty { get; set; }
        
        [Column("product_weight_g")]
        public int? ProductWeightG { get; set; }
        
        [Column("product_length_cm")]
        public int? ProductLengthCm { get; set; }
        
        [Column("product_height_cm")]
        public int? ProductHeightCm { get; set; }
        
        [Column("product_width_cm")]
        public int? ProductWidthCm { get; set; }
    }
}