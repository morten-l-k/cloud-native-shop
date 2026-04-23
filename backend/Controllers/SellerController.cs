using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SellerController : ControllerBase
    {
        private readonly ShopContext _context;

        public SellerController(ShopContext context)
        {
            _context = context;
        }

        // GET: seller/me
        [HttpGet("me")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> Me()
        {
            var sellerId = User.FindFirst("user_id")?.Value;

            if (sellerId == null)
                return Unauthorized();

            var seller = await _context.Seller
                .FirstOrDefaultAsync(s => s.SellerId == sellerId);

            if (seller == null)
                return NotFound();

            return Ok(seller);
        }
    }
}
