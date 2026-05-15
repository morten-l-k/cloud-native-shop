using backend.Services;
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
    }
}
