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
    public class StockControllerTests
    {
        private (ShopContext context, StockController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var stockService = new StockService(context);
            var controller = new StockController(stockService);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("user_id", "s1"),
                new Claim(ClaimTypes.Role, "seller")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return (context, controller);
        }

        [Fact]
        public async Task GetStock_ProductExists_ReturnsStock()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", ProductStock = 10 });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetStock("p1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Use dynamic or anonymous type check
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SetStock_ValidSeller_UpdatesStock()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", SellerId = "s1", ProductStock = 5 });
            await context.SaveChangesAsync();

            var req = new StockController.SetStockRequest(20);

            // Act
            var result = await controller.SetStock("p1", req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            context.ChangeTracker.Clear();
            var product = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(20, product.ProductStock);
        }
    }
}
