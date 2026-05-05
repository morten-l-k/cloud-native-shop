using backend.Data;
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

        public PaymentController(ShopContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.CustomerId == customerId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.OrderStatus != "created")
                return BadRequest("Order cannot be paid in its current status.");

            order.OrderStatus = "paid";
            order.OrderApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("payment successful");
        }
    }
}
