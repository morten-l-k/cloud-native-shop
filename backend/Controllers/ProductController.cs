using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudNativeShop.Backend.Models;
using backend.Data;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly StockService _stockService;
        private readonly PriceService _priceService;
        public ProductController(ShopContext context, IConfiguration configuration, StockService stockService, PriceService priceService)
        {
            _context = context;
            _stockService = stockService;
            _priceService = priceService;
        }

        public record ProductPageResponse(ProductResponse[] Items, int Page, int PageSize, int TotalCount, int TotalPages);

        // GET: product/seller  (seller only)
        // Returns all products belonging to this seller, with sales stats from order history.
        [HttpGet("seller")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SellerProducts()
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var products = await _context.Product
                .Where(p => p.SellerId == sellerId)
                .ToListAsync();

            var productIds = products.Select(p => p.ProductId).ToHashSet();

            var soldStats = await _context.OrderItem
                .Where(i => productIds.Contains(i.ProductId))
                .GroupBy(i => i.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(i => i.OrderItemQuantity ?? 0),
                    TotalRevenue = g.Sum(i => (i.Price ?? 0m) * (i.OrderItemQuantity ?? 0))
                })
                .ToListAsync();

            var statsMap = soldStats.ToDictionary(s => s.ProductId);

            var categoryNames = products
                .Select(p => p.ProductCategoryName)
                .Where(c => c != null)
                .Distinct()
                .ToList();

            var categoryMap = await _context.Category
                .Where(c => categoryNames.Contains(c.ProductCategoryName))
                .ToDictionaryAsync(c => c.ProductCategoryName, c => c.ProductCategoryNameEnglish);

            var result = products.Select(p => new
            {
                p.ProductId,
                ProductName = p.ProductName ?? "Unknown",
                p.ProductCategoryName,
                ProductCategoryNameEnglish = p.ProductCategoryName != null && categoryMap.TryGetValue(p.ProductCategoryName, out var eng) ? eng : null,
                p.ProductDescription,
                ProductPrice = p.ProductPrice ?? 0m,
                ProductStock = p.ProductStock ?? 0,
                TotalSold = statsMap.TryGetValue(p.ProductId, out var s) ? s.TotalSold : 0,
                TotalRevenue = statsMap.TryGetValue(p.ProductId, out var r) ? r.TotalRevenue : 0m,
            });

            return Ok(result);
        }

        // GET: Product?page=1&minPrice=10&maxPrice=100&category=electronics&sort=price_asc&search=shirt
        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? category = null,
            [FromQuery] string? sort = null,
            [FromQuery] string? search = null)
        {
            const int pageSize = 10;
            page = Math.Max(1, page);

            var query = _context.Product.AsQueryable();

            if (minPrice.HasValue)
                query = query.Where(p => p.ProductPrice >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.ProductPrice <= maxPrice.Value);
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.ProductCategoryName == category);
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.ProductName != null && p.ProductName.ToLower().Contains(search.ToLower()));

            query = sort switch
            {
                "price_asc"  => query.OrderBy(p => p.ProductPrice),
                "price_desc" => query.OrderByDescending(p => p.ProductPrice),
                _            => query
            };

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = await Task.WhenAll(products.Select(MapToResponseAsync));

            return Ok(new ProductPageResponse(items, page, pageSize, totalCount, totalPages));
        }

        // GET: Product/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var productModel = await (from p in _context.Product
                          where p.ProductId == id
                          select p).FirstOrDefaultAsync();
            
            if (productModel == null)
            {
                return NotFound();
            }

            // Converts the Product type into ProductResponse type, removing unnecessary data
            return Ok(await MapToResponseAsync(productModel));
        }

        public record CreateProductRequest(
            string Name,
            string? Category,
            string? Description,
            decimal Price,
            int Stock
        );

        // POST: Product  (seller only)
        [HttpPost]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var product = new Product
            {
                ProductId = Guid.NewGuid().ToString("N"),
                ProductName = req.Name,
                ProductCategoryName = req.Category,
                ProductDescription = req.Description,
                ProductPrice = req.Price,
                ProductStock = req.Stock,
                SellerId = sellerId
            };

            _context.Product.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Details), new { id = product.ProductId }, new
            {
                product.ProductId,
                ProductName = product.ProductName,
                product.ProductCategoryName,
                ProductPrice = product.ProductPrice ?? 0m,
                ProductStock = product.ProductStock ?? 0
            });
        }

        public record UpdateProductRequest(
            string Name,
            string? Category,
            string? Description,
            decimal Price,
            int Stock
        );

        // PUT: Product/{id}  (seller only, must own the product)
        [HttpPut("{id}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateProductRequest req)
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return NotFound();
            if (product.SellerId != sellerId) return Forbid();

            product.ProductName = req.Name;
            product.ProductCategoryName = req.Category;
            product.ProductDescription = req.Description;

            await _context.SaveChangesAsync();
            await _stockService.SetStockAsync(id, sellerId, req.Stock);
            await _priceService.SetPriceAsync(id, sellerId, req.Price);

            return Ok(new
            {
                product.ProductId,
                ProductName = product.ProductName,
                product.ProductCategoryName,
                ProductPrice = req.Price,
                ProductStock = req.Stock,
                TotalSold = 0,
                TotalRevenue = 0m,
            });
        }

        // DELETE: Product/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var productModel = await (from p in _context.Product
                          where p.ProductId == id
                          select p).FirstOrDefaultAsync();
            
            if (productModel == null)
            {
                return NotFound();
            }

            _context.Product.Remove(productModel);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        // converts the product type into the currents easy response model of our product
        // see Models/ProductResponse.cs
        private async Task<ProductResponse> MapToResponseAsync(Product product)
        {
            string imageUrl = await GenerateImageUrl(product.ProductName ?? "product");

            return new ProductResponse
            {
                Id = product.ProductId,

                Name = product.ProductName ?? "No name available.",

                Description = product.ProductDescription ?? "No description available.",

                Category = product.ProductCategoryName,

                Price = product.ProductPrice ?? 5.00m,
                
                ImageUrl = imageUrl,

                Stock = product.ProductStock ?? 0
            };
        }

        private async Task<string> GenerateImageUrl(string productName)
        {
            var url = $"https://api.pexels.com/v1/search?query={Uri.EscapeDataString(productName)}&per_page=1";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", "VJWCMspXFn1QKt5cqPwTScVFpJsOl35a6x9KRYThPZ8VjPhv6o2trkBv");

            using var response = await new HttpClient().SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var json = await System.Text.Json.JsonDocument.ParseAsync(stream);

            var photos = json.RootElement.GetProperty("photos");
            if (photos.GetArrayLength() == 0)
            {
                    return "https://images.pexels.com/photos/9582578/pexels-photo-9582578.jpeg?auto=compress&cs=tinysrgb&h=350";
                }

            return photos[0]
                .GetProperty("src")
                .GetProperty("medium")
                .GetString() ?? "https://images.pexels.com/photos/9582578/pexels-photo-9582578.jpeg?auto=compress&cs=tinysrgb&h=350";
        }
    }
}
