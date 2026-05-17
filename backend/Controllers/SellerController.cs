using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SellerController(ShopContext context) : ControllerBase
    {
        // GET: seller/me
        [HttpGet("me")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> Me()
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var seller = await context.Seller
                .FirstOrDefaultAsync(s => s.SellerId == sellerId);

            return seller == null ? NotFound() : Ok(seller);
        }
    }
}
