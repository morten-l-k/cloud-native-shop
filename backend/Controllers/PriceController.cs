using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PriceController(PriceService priceService) : ControllerBase
    {
        public record SetPriceRequest(decimal Price);

        [HttpPut("{productId}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SetPrice(string productId, [FromBody] SetPriceRequest req)
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();
            bool updated = await priceService.SetPriceAsync(productId, sellerId, req.Price);
            if (!updated) return NotFound();
            return Ok(new { productId, price = req.Price });
        }
    }
}
