using System.Security.Claims;
using backend.Controllers;
using backend.Data;
using backend.Services;
using CloudNativeShop.Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace backend.Tests.ControllerTests
{
    public class ProductControllerTests
    {
        private (ShopContext context, ProductController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var config = new ConfigurationBuilder().Build();
            var stockService = new StockService(context);
            var priceService = new PriceService(context);
            
            var controller = new ProductController(context, config, stockService, priceService);
            
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
        public async Task Index_NoFilters_ReturnsAllActiveProducts()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.AddRange(
                new Product { ProductId = "p1", ProductName = "Active", IsActive = true, SellerId = "s1" },
                new Product { ProductId = "p2", ProductName = "Inactive", IsActive = false, SellerId = "s1" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Index();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProductController.ProductPageResponse>(okResult.Value);
            Assert.Single(response.Items);
            Assert.Equal("Active", response.Items[0].Name);
        }

        [Theory]
        [InlineData(null, "40", 1)] // MaxPrice 40 -> Only Aaa
        [InlineData("20", null, 1)] // MinPrice 20 -> Only Bbb
        [InlineData("20", "40", 0)]  // Range 20-40 -> None
        public async Task Index_PriceFilters_ReturnsMatchingProducts(string? minPriceStr, string? maxPriceStr, int expectedCount)
        {
            // Arrange
            decimal? minPrice = minPriceStr != null ? decimal.Parse(minPriceStr) : null;
            decimal? maxPrice = maxPriceStr != null ? decimal.Parse(maxPriceStr) : null;

            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.AddRange(
                new Product { ProductId = "p1", ProductName = "Aaa", ProductPrice = 10, IsActive = true, SellerId = "s1" },
                new Product { ProductId = "p2", ProductName = "Bbb", ProductPrice = 50, IsActive = true, SellerId = "s1" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Index(minPrice: minPrice, maxPrice: maxPrice);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ProductController.ProductPageResponse>(okResult.Value);
            Assert.Equal(expectedCount, response.Items.Length);
        }

        [Fact]
        public async Task Details_ProductExists_ReturnsOkWithProduct()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", ProductName = "Test Product", IsActive = true, SellerId = "s1" });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Details("p1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var product = Assert.IsType<ProductResponse>(okResult.Value);
            Assert.Equal("p1", product.Id);
        }

        [Fact]
        public async Task Details_ProductDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (_, controller) = CreateController(connection);

            // Act
            var result = await controller.Details("nonexistent");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            var req = new ProductController.CreateProductRequest("New Product", "Cat", "Desc", 10.0m, 5);

            // Act
            var result = await controller.Create(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            context.ChangeTracker.Clear();
            var productInDb = await context.Product.FirstOrDefaultAsync(p => p.ProductName == "New Product");
            Assert.NotNull(productInDb);
            Assert.Equal("s1", productInDb.SellerId);
        }

        [Fact]
        public async Task Update_ValidOwner_ReturnsOkAndUpdatesProduct()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", ProductName = "Old", SellerId = "s1", IsActive = true });
            await context.SaveChangesAsync();

            var req = new ProductController.UpdateProductRequest("New", "Cat", "Desc", 15.0m, 10);

            // Act
            var result = await controller.Update("p1", req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            context.ChangeTracker.Clear();
            var product = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal("New", product.ProductName);
        }

        [Fact]
        public async Task Relist_ValidOwner_ReturnsNoContentAndActivatesProduct()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", SellerId = "s1", IsActive = false });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Relist("p1");

            // Assert
            Assert.IsType<NoContentResult>(result);
            context.ChangeTracker.Clear();
            var product = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.True(product.IsActive);
        }

        [Fact]
        public async Task Delete_ValidOwner_ReturnsNoContentAndSoftDeletesProduct()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Product.Add(new Product { ProductId = "p1", SellerId = "s1", IsActive = true });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Delete("p1");

            // Assert
            Assert.IsType<NoContentResult>(result);
            context.ChangeTracker.Clear();
            var product = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.False(product.IsActive);
        }

        [Fact]
        public async Task SellerProducts_SellerHasOrders_ReturnsProductsWithStats()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            var p1 = new Product { ProductId = "p1", ProductName = "P1", SellerId = "s1", IsActive = true };
            context.Product.Add(p1);
            var o1 = new Order { OrderId = "o1", CustomerId = "c1" };
            context.Order.Add(o1);
            context.OrderItem.Add(new OrderItem { OrderId = "o1", ProductId = "p1", Product = p1, Price = 10, OrderItemQuantity = 2 });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.SellerProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<System.Collections.IEnumerable>(okResult.Value);
            Assert.NotEmpty(items);
        }
    }
}
