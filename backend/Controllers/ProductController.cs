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
            return Ok(await (from p in _context.Product
                 select p).Take(10).ToListAsync());
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

            return Ok(productModel);
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
    }
}
