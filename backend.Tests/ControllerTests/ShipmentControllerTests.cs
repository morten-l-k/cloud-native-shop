using System.Security.Claims;
using backend.Controllers;
using backend.Data;
using CloudNativeShop.Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.ControllerTests
{
    public class ShipmentControllerTests
    {
        private (ShopContext context, ShipmentController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var controller = new ShipmentController(context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("user_id", "c1"),
                new Claim(ClaimTypes.Role, "customer")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return (context, controller);
        }

        [Fact]
        public async Task Ship_PaidOrder_UpdatesStatusToShipped()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            var order = new Order { OrderId = "o1", CustomerId = "c1", OrderStatus = "paid" };
            context.Order.Add(order);
            await context.SaveChangesAsync();

            var req = new ShipmentController.ShipRequest("o1");

            // Act
            var result = await controller.Ship(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("shipment in process", okResult.Value);
            context.ChangeTracker.Clear();
            var updatedOrder = await context.Order.FirstAsync(o => o.OrderId == "o1");
            Assert.Equal("shipped", updatedOrder.OrderStatus);
            Assert.NotNull(updatedOrder.OrderDeliveredCarrierDate);
        }

        [Fact]
        public async Task Ship_UnpaidOrder_ReturnsBadRequest()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            var order = new Order { OrderId = "o1", CustomerId = "c1", OrderStatus = "created" };
            context.Order.Add(order);
            await context.SaveChangesAsync();

            var req = new ShipmentController.ShipRequest("o1");

            // Act
            var result = await controller.Ship(req);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
