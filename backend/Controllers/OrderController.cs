using backend.Data;
using backend.Services;
using CloudNativeShop.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ShopContext _context;
        private readonly StockService _stockService;

        public OrderController(ShopContext context, StockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        public record PlaceOrderItem(string ProductId, string SellerId, int Quantity, decimal Price);
        public record PlaceOrderRequest(List<PlaceOrderItem> Items);

        // POST: order  (customer only)
        // Places a new order for the logged-in customer.
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var customerId = User.FindFirst("user_id")?.Value;
            if (customerId == null) return Unauthorized();

            if (request.Items == null || request.Items.Count == 0)
                return BadRequest("Order must contain at least one item.");

            // Verify all products exist
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var existingProductIds = await _context.Product
                .Where(p => productIds.Contains(p.ProductId))
                .Select(p => p.ProductId)
                .ToHashSetAsync();

            var missingProduct = request.Items.FirstOrDefault(i => !existingProductIds.Contains(i.ProductId));
            if (missingProduct != null)
                return BadRequest($"Product '{missingProduct.ProductId}' not found.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            foreach (var item in request.Items)
            {
                if (!await _stockService.CheckAndDecrementAsync(item.ProductId, item.Quantity))
                {
                    await tx.RollbackAsync();
                    return Conflict($"Insufficient stock for product '{item.ProductId}'.");
                }
            }

            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                CustomerId = customerId,
                OrderStatus = "created",
                OrderPurchaseTimestamp = DateTime.UtcNow,
                OrderEstimatedDeliveryDate = DateTime.UtcNow.AddDays(7),
                OrderItems = request.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    SellerId = i.SellerId,
                    OrderItemQuantity = i.Quantity,
                    Price = i.Price,
                    FreightValue = 0
                }).ToList()
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return CreatedAtAction(nameof(MyOrders), new { }, order);
        }

        // GET: order/me  (customer only)
        // Returns all orders placed by the logged-in customer.
        [HttpGet("me")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> MyOrders()
        {
            var customerId = User.FindFirst("user_id")?.Value;
            if (customerId == null) return Unauthorized();

            var orders = await _context.Order
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderItems)
                .ToListAsync();

            return Ok(orders);
        }

        // GET: order/seller  (seller only)
        // Returns a summary of all orders that contain at least one item from the logged-in seller.
        [HttpGet("seller")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SellerOrders()
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var orders = await _context.Order
                .Where(o => o.OrderItems.Any(i => i.SellerId == sellerId))
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderStatus,
                    o.OrderPurchaseTimestamp,
                    o.OrderEstimatedDeliveryDate,
                    ItemCount = o.OrderItems.Count(i => i.SellerId == sellerId),
                    TotalValue = o.OrderItems
                        .Where(i => i.SellerId == sellerId)
                        .Sum(i => i.Price * i.OrderItemQuantity ?? 0)
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: order/seller/{orderId}  (seller only)
        // Returns full details of a specific order including items and product info, filtered to the seller's items.
        [HttpGet("seller/{orderId}")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SellerOrderDetail(string orderId)
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var order = await _context.Order
                .Where(o => o.OrderId == orderId && o.OrderItems.Any(i => i.SellerId == sellerId))
                .Include(o => o.OrderItems.Where(i => i.SellerId == sellerId))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            return Ok(order);
        }
    }
}
