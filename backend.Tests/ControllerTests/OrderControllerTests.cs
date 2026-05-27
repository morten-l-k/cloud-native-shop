using System.Security.Claims;
using backend.Controllers;
using backend.Data;
using backend.Services;
using CloudNativeShop.Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.ControllerTests
{
    public class OrderControllerTests
    {
        private (ShopContext context, OrderController controller) CreateController(SqliteConnection connection, string role = "customer", string userId = "c1")
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var stockService = new StockService(context);
            var controller = new OrderController(context, stockService);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("user_id", userId),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return (context, controller);
        }

        [Fact]
        public async Task PlaceOrder_ValidRequest_CreatesOrderAndDecrementsStock()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", ProductName = "Test", ProductStock = 10, IsActive = true, SellerId = "s1" });
            await context.SaveChangesAsync();

            var items = new List<OrderController.PlaceOrderItem> { new OrderController.PlaceOrderItem("p1", 2, 10.0m) };
            var req = new OrderController.PlaceOrderRequest(items);

            // Act
            var result = await controller.PlaceOrder(req);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            context.ChangeTracker.Clear();
            var product = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(8, product.ProductStock);
            Assert.Equal(1, await context.Order.CountAsync());
        }

        [Fact]
        public async Task PlaceOrder_InsufficientStock_ReturnsConflictAndRollsBack()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", ProductName = "Test", ProductStock = 5, IsActive = true, SellerId = "s1" });
            await context.SaveChangesAsync();

            var items = new List<OrderController.PlaceOrderItem> { new OrderController.PlaceOrderItem("p1", 10, 10.0m) };
            var req = new OrderController.PlaceOrderRequest(items);

            // Act
            var result = await controller.PlaceOrder(req);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
            context.ChangeTracker.Clear();
            var product = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(5, product.ProductStock); // Stock should not have changed
            Assert.Equal(0, await context.Order.CountAsync());
        }

        [Fact]
        public async Task MyOrders_ReturnsOnlyCustomerOrders()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection, "customer", "c1");

            context.Order.AddRange(
                new Order { OrderId = "o1", CustomerId = "c1" },
                new Order { OrderId = "o2", CustomerId = "c2" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.MyOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var orders = Assert.IsAssignableFrom<IEnumerable<Order>>(okResult.Value);
            Assert.Single(orders);
            Assert.Equal("o1", orders.First().OrderId);
        }

        [Fact]
        public async Task SellerOrders_ReturnsOnlyOrdersWithSellerItems()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection, "seller", "s1");

            var p1 = new Product { ProductId = "p1", SellerId = "s1" };
            var p2 = new Product { ProductId = "p2", SellerId = "s2" };
            context.Product.AddRange(p1, p2);

            var o1 = new Order { OrderId = "o1", CustomerId = "c1", OrderPurchaseTimestamp = DateTime.UtcNow };
            o1.OrderItems = new List<OrderItem> { new OrderItem { ProductId = "p1", Product = p1, Price = 10, OrderItemQuantity = 1 } };

            var o2 = new Order { OrderId = "o2", CustomerId = "c2", OrderPurchaseTimestamp = DateTime.UtcNow };
            o2.OrderItems = new List<OrderItem> { new OrderItem { ProductId = "p2", Product = p2, Price = 20, OrderItemQuantity = 1 } };

            context.Order.AddRange(o1, o2);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.SellerOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var orders = Assert.IsAssignableFrom<System.Collections.IEnumerable>(okResult.Value);
            var orderList = new List<object>();
            foreach (var o in orders) orderList.Add(o);
            Assert.Single(orderList);
        }
    }
}
