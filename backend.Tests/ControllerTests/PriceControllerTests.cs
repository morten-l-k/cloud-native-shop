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
    public class PriceControllerTests
    {
        private (ShopContext context, PriceController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var priceService = new PriceService(context);
            var controller = new PriceController(priceService);

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
        public async Task SetPrice_ValidSeller_UpdatesPrice()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", SellerId = "s1", ProductPrice = 10.0m });
            await context.SaveChangesAsync();

            var req = new PriceController.SetPriceRequest(25.0m);

            // Act
            var result = await controller.SetPrice("p1", req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            context.ChangeTracker.Clear();
            var product = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(25.0m, product.ProductPrice);
        }
    }
}
