using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StockController(StockService stockService) : ControllerBase
    {
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetStock(string productId)
        {
            int stock = await stockService.GetStockAsync(productId);
            return Ok(new { productId, stock });
        }

        public record SetStockRequest(int Stock);

        [HttpPut("{productId}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SetStock(string productId, [FromBody] SetStockRequest req)
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();
            bool updated = await stockService.SetStockAsync(productId, sellerId, req.Stock);
            if (!updated) return NotFound();
            return Ok(new { productId, stock = req.Stock });
        }
    }
}
