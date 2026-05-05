using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly ShopContext _context;

        public ShipmentController(ShopContext context)
        {
            _context = context;
        }

        public record ShipRequest(string OrderId);

        // POST: shipment  (customer only)
        // Initiates shipment for a paid order and updates its status to "shipped".
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Ship([FromBody] ShipRequest request)
        {
            var customerId = User.FindFirst("user_id")?.Value;
            if (customerId == null) return Unauthorized();

            var order = await _context.Order
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.CustomerId == customerId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.OrderStatus != "paid")
                return BadRequest("Order must be paid before it can be shipped.");

            order.OrderStatus = "shipped";
            order.OrderDeliveredCarrierDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("shipment in process");
        }
    }
}
