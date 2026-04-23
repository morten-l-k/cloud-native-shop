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

        [Column("password")]
        public string? CustomerPassword { get; set; }

        [Column("customer_zip_code_prefix")]
        public string? CustomerZipCodePrefix { get; set; }

        [Column("customer_city")]
        public string? CustomerCity { get; set; }

        [Column("customer_state")]
        public string? CustomerState { get; set; }
        
        [Column("first_name")]
        public string? FirstName { get; set; }
        
        [Column("last_name")]
        public string? LastName { get; set; }
        
        [Column("email_address")]
        public string? EmailAddress { get; set; }

        [Column("street_address")]
        public string? StreetAddress { get; set; }
    }
}
