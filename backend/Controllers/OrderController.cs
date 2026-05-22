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

        public record PlaceOrderItem(string ProductId, int Quantity, decimal Price);
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

        // GET: order/me/{orderId}  (customer only)
        // Returns full details of a specific order including product info, scoped to the logged-in customer.
        [HttpGet("me/{orderId}")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> MyOrderDetail(string orderId)
        {
            var customerId = User.FindFirst("user_id")?.Value;
            if (customerId == null) return Unauthorized();

            var order = await _context.Order
                .Where(o => o.OrderId == orderId && o.CustomerId == customerId)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();
            return Ok(order);
        }

        // GET: order/seller  (seller only)
        // Returns a summary of orders that contain at least one item from the logged-in seller.
        // months=6 (default) limits to the last N months; months=0 returns all time.
        [HttpGet("seller")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SellerOrders([FromQuery] int months = 6)
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var query = _context.Order
                .Where(o => o.OrderItems.Any(i => i.Product!.SellerId == sellerId));

            if (months > 0)
                query = query.Where(o => o.OrderPurchaseTimestamp >= DateTime.UtcNow.AddMonths(-months));

            var orders = await query
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderStatus,
                    o.OrderPurchaseTimestamp,
                    o.OrderEstimatedDeliveryDate,
                    ItemCount = o.OrderItems.Count(i => i.Product!.SellerId == sellerId),
                    TotalValue = o.OrderItems
                        .Where(i => i.Product!.SellerId == sellerId)
                        .Sum(i => i.Price * i.OrderItemQuantity ?? 0)
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: order/seller/analytics  (seller only)
        // Returns aggregated revenue and order stats for the logged-in seller.
        // months=6 (default) limits to the last N months; months=0 returns all time.
        [HttpGet("seller/analytics")]
        [Authorize(Roles = "seller")]
        public async Task<IActionResult> SellerAnalytics([FromQuery] int months = 6)
        {
            var sellerId = User.FindFirst("user_id")?.Value;
            if (sellerId == null) return Unauthorized();

            var itemsQuery = _context.OrderItem
                .Where(i => i.Product!.SellerId == sellerId);

            if (months > 0)
                itemsQuery = itemsQuery.Where(i => i.Order.OrderPurchaseTimestamp >= DateTime.UtcNow.AddMonths(-months));

            var items = await itemsQuery
                .Include(i => i.Order)
                .ToListAsync();

            var totalRevenue = items.Sum(i => (i.Price ?? 0m) * (i.OrderItemQuantity ?? 0));
            var totalOrders = items.Select(i => i.OrderId).Distinct().Count();
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

            var monthlyRevenue = items
                .Where(i => i.Order.OrderPurchaseTimestamp.HasValue)
                .GroupBy(i => new { i.Order.OrderPurchaseTimestamp!.Value.Year, i.Order.OrderPurchaseTimestamp!.Value.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Revenue = g.Sum(i => (i.Price ?? 0m) * (i.OrderItemQuantity ?? 0)),
                    OrderCount = g.Select(i => i.OrderId).Distinct().Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            var statusBreakdown = items
                .GroupBy(i => i.Order.OrderStatus ?? "unknown")
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Select(i => i.OrderId).Distinct().Count()
                })
                .ToList();

            return Ok(new
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AvgOrderValue = avgOrderValue,
                MonthlyRevenue = monthlyRevenue,
                StatusBreakdown = statusBreakdown
            });
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
                .Where(o => o.OrderId == orderId && o.OrderItems.Any(i => i.Product!.SellerId == sellerId))
                .Include(o => o.OrderItems.Where(i => i.Product!.SellerId == sellerId))
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();

            var categoryNames = order.OrderItems
                .Select(i => i.Product?.ProductCategoryName)
                .Where(c => c != null)
                .Distinct()
                .ToList();

            var categoryMap = await _context.Category
                .Where(c => categoryNames.Contains(c.ProductCategoryName))
                .ToDictionaryAsync(c => c.ProductCategoryName, c => c.ProductCategoryNameEnglish);

            return Ok(new
            {
                order.OrderId,
                order.OrderStatus,
                order.OrderPurchaseTimestamp,
                order.OrderEstimatedDeliveryDate,
                OrderItems = order.OrderItems.Select(i => new
                {
                    i.ProductId,
                    i.OrderItemQuantity,
                    i.Price,
                    i.FreightValue,
                    Product = i.Product == null ? null : new
                    {
                        i.Product.ProductName,
                        i.Product.ProductCategoryName,
                        ProductCategoryNameEnglish = i.Product.ProductCategoryName != null && categoryMap.TryGetValue(i.Product.ProductCategoryName, out var eng) ? eng : null,
                        i.Product.ProductPrice,
                    }
                })
            });
        }
    }
}
