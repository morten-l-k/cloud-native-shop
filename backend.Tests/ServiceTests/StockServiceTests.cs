using backend.Data;
using backend.Services;
using CloudNativeShop.Backend.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.ServiceTests
{
    public class StockServiceTests
    {
        private ShopContext CreateContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetStockAsync_ProductExists_ReturnsCorrectStock()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var context = CreateContext(connection);

            var product = new Product
            {
                ProductId = "p1",
                SellerId = "s1",
                ProductStock = 50,
                IsActive = true
            };
            context.Product.Add(product);
            await context.SaveChangesAsync();

            var service = new StockService(context);

            // Act
            var result = await service.GetStockAsync("p1");

            // Assert
            Assert.Equal(50, result);
        }

        [Fact]
        public async Task CheckAndDecrementAsync_EnoughStock_DecrementsStockAndReturnsTrue()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var context = CreateContext(connection);

            var product = new Product
            {
                ProductId = "p1",
                SellerId = "s1",
                ProductStock = 10,
                IsActive = true
            };
            context.Product.Add(product);
            await context.SaveChangesAsync();

            var service = new StockService(context);

            // Act
            var result = await service.CheckAndDecrementAsync("p1", 4);

            // Assert
            Assert.True(result);
            context.ChangeTracker.Clear();
            var updatedProduct = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(6, updatedProduct.ProductStock);
        }

        [Fact]
        public async Task CheckAndDecrementAsync_InsufficientStock_ReturnsFalseAndDoesNotChangeStock()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var context = CreateContext(connection);

            var product = new Product
            {
                ProductId = "p1",
                SellerId = "s1",
                ProductStock = 3,
                IsActive = true
            };
            context.Product.Add(product);
            await context.SaveChangesAsync();

            var service = new StockService(context);

            // Act
            var result = await service.CheckAndDecrementAsync("p1", 5);

            // Assert
            Assert.False(result);
            context.ChangeTracker.Clear();
            var updatedProduct = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(3, updatedProduct.ProductStock);
        }

        [Fact]
        public async Task RestoreAsync_ValidProduct_IncrementsStock()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var context = CreateContext(connection);

            var product = new Product
            {
                ProductId = "p1",
                SellerId = "s1",
                ProductStock = 10,
                IsActive = true
            };
            context.Product.Add(product);
            await context.SaveChangesAsync();

            var service = new StockService(context);

            // Act
            await service.RestoreAsync("p1", 5);

            // Assert
            context.ChangeTracker.Clear();
            var updatedProduct = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(15, updatedProduct.ProductStock);
        }

        [Fact]
        public async Task SetStockAsync_ValidProduct_UpdatesStockAndReturnsTrue()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var context = CreateContext(connection);

            var product = new Product
            {
                ProductId = "p1",
                SellerId = "s1",
                ProductStock = 10,
                IsActive = true
            };
            context.Product.Add(product);
            await context.SaveChangesAsync();

            var service = new StockService(context);

            // Act
            var result = await service.SetStockAsync("p1", "s1", 100);

            // Assert
            Assert.True(result);
            context.ChangeTracker.Clear();
            var updatedProduct = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(100, updatedProduct.ProductStock);
        }
    }
}
