namespace CloudNativeShop.Backend.Models
{
    // current easy response model of our product
    public class ProductResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; } = null;
        public decimal Price { get; set; } = 0m;
        public string ImageUrl { get; set; } = "https://images.pexels.com/photos/9582578/pexels-photo-9582578.jpeg?auto=compress&cs=tinysrgb&h=350";
    }
}
