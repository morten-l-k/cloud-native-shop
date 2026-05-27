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
    public class SellerControllerTests
    {
        private (ShopContext context, SellerController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var controller = new SellerController(context);

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
        public async Task Me_SellerExists_ReturnsOk()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Seller.Add(new Seller { SellerId = "s1" });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Me();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var seller = Assert.IsType<Seller>(okResult.Value);
            Assert.Equal("s1", seller.SellerId);
        }
    }
}
