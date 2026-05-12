using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudNativeShop.Backend.Models;
using backend.Data;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ShopContext _context;
        public ProductController(ShopContext context, IConfiguration configuration)
        {
            _context = context;
        }

        public record ProductPageResponse(ProductResponse[] Items, int Page, int PageSize, int TotalCount, int TotalPages);

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

        // POST: Product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product productModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Add(productModel);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(Details), new { id = productModel.ProductId }, productModel);
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
                
                ImageUrl = imageUrl
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
