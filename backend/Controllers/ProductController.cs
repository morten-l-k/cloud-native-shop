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

        public ProductController(ShopContext context)
        {
            _context = context;
        }

        // GET: Product
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await (from p in _context.Product
                                  select p).Take(20).ToListAsync();

            var response = products.Select(MapToResponse).ToList();
            return Ok(response);
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

            return Ok(MapToResponse(productModel));
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
        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.ProductId,
                // Product-XXXX
                Name = $"Product-{Random.Shared.Next(1000, 9999)}",
                // Double between 5.0 and 505.0
                Price = decimal.Round((decimal)(Random.Shared.NextDouble() * 500.0 + 5.0), 2)
            };
        }
    }
}
