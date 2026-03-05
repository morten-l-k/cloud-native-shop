using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CloudNativeShop.Backend.Models;
using backend.Data;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ShopContext _context;


        public CategoryController(ShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Ok(await (from c in _context.Category
                select c).ToListAsync());
        }
        
    }
}


