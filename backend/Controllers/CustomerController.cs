using backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ShopContext _context;

        public CustomerController(ShopContext context)
        {
            _context = context;
        }

        // GET: customer/me
        [HttpGet("me")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> Me()
        {
            var customerId = User.FindFirst("user_id")?.Value;

            if (customerId == null)
                return Unauthorized();

            var customer = await _context.Customer
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }
    }
}
