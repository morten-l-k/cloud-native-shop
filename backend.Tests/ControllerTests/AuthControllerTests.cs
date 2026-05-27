using backend.Controllers;
using backend.Data;
using CloudNativeShop.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace backend.Tests.ControllerTests
{
    public class AuthControllerTests
    {
        private (ShopContext context, AuthController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var configDict = new Dictionary<string, string>
            {
                {"Jwt:Key", "extremely_long_and_secret_key_for_testing_purposes"},
                {"Jwt:Issuer", "test_issuer"},
                {"Jwt:Audience", "test_audience"},
                {"Jwt:ExpiryMinutes", "60"}
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(configDict).Build();

            var controller = new AuthController(context, config);
            return (context, controller);
        }

        [Fact]
        public async Task LoginCustomer_ValidCredentials_ReturnsToken()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Customer.Add(new Customer { CustomerId = "c1", EmailAddress = "test@example.com", CustomerPassword = "password" });
            await context.SaveChangesAsync();

            var req = new AuthController.CustomerLoginRequest("test@example.com", "password");

            // Act
            var result = await controller.LoginCustomer(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthController.LoginResponse>(okResult.Value);
            Assert.NotNull(response.Token);
            Assert.Equal("c1", response.Id);
            Assert.Equal("customer", response.Role);
        }

        [Fact]
        public async Task LoginCustomer_InvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (_, controller) = CreateController(connection);

            var req = new AuthController.CustomerLoginRequest("invalid-email", "password");

            // Act
            var result = await controller.LoginCustomer(req);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task RegisterCustomer_Success_CreatesCustomerAndReturnsToken()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            var req = new AuthController.RegisterCustomerRequest(
                "password", "12345", "City", "ST", "First", "Last", "new@example.com", "Street");

            // Act
            var result = await controller.RegisterCustomer(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthController.LoginResponse>(okResult.Value);
            Assert.NotNull(response.Token);
            Assert.True(await context.Customer.AnyAsync(c => c.EmailAddress == "new@example.com"));
        }

        [Fact]
        public async Task RegisterCustomer_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Customer.Add(new Customer { CustomerId = "c1", EmailAddress = "existing@example.com" });
            await context.SaveChangesAsync();

            var req = new AuthController.RegisterCustomerRequest(
                "password", "12345", "City", "ST", "First", "Last", "existing@example.com", "Street");

            // Act
            var result = await controller.RegisterCustomer(req);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
