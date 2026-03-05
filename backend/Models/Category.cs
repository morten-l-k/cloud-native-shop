using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudNativeShop.Backend.Models
{
    [Table("product_category_translation")] 
    public class Category
    {
        [Key]
        [Column("product_category_name")]
        public string ProductCategoryName { get; set; } = string.Empty;
        
        [Column("product_category_name_english")]
        public string? ProductCategoryNameEnglish { get; set; }

    }
}