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
    public class CustomerControllerTests
    {
        private (ShopContext context, CustomerController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var controller = new CustomerController(context);

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
        public async Task Me_CustomerExists_ReturnsOk()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Customer.Add(new Customer { CustomerId = "c1", EmailAddress = "test@example.com" });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Me();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var customer = Assert.IsType<Customer>(okResult.Value);
            Assert.Equal("c1", customer.CustomerId);
        }

        [Fact]
        public async Task Me_CustomerNotFound_ReturnsNotFound()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (_, controller) = CreateController(connection);

            // Act
            var result = await controller.Me();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
