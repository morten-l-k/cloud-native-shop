using backend.Data;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly StockService _stockService;

        public PaymentController(ShopContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        public record PayRequest(string OrderId);

        // POST: payment  (customer only)
        // Processes payment for an order and updates its status to "paid".
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Pay([FromBody] PayRequest request)
        {
            var customerId = User.FindFirst("user_id")?.Value;
            if (customerId == null) return Unauthorized();

            var order = await _context.Order
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.CustomerId == customerId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.OrderStatus != "created")
                return BadRequest("Order cannot be paid in its current status.");

            // 5 % of the payment requests get declined
            if (Random.Shared.NextDouble() < 0.05)
            {
                foreach (var item in order.OrderItems)
                    await _stockService.RestoreAsync(item.ProductId!, item.OrderItemQuantity ?? 0);
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
                return StatusCode(402, "Payment declined. Please try again.");
            }

            order.OrderStatus = "paid";
            order.OrderApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("payment successful");
        }
    }
}
