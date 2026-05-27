using backend.Data;
using backend.Services;
using CloudNativeShop.Backend.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.ServiceTests
{
    public class PriceServiceTests
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
        public async Task SetPriceAsync_ValidProduct_UpdatesPriceAndReturnsTrue()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var context = CreateContext(connection);

            var product = new Product
            {
                ProductId = "p1",
                SellerId = "s1",
                ProductPrice = 10.0m,
                IsActive = true
            };
            context.Product.Add(product);
            await context.SaveChangesAsync();

            var service = new PriceService(context);

            // Act
            var result = await service.SetPriceAsync("p1", "s1", 20.0m);

            // Assert
            Assert.True(result);
            // Verify database update
            context.ChangeTracker.Clear();
            var updatedProduct = await context.Product.FirstAsync(p => p.ProductId == "p1");
            Assert.Equal(20.0m, updatedProduct.ProductPrice);
        }

        [Fact]
        public async Task SetPriceAsync_InvalidProduct_ReturnsFalse()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var context = CreateContext(connection);

            var service = new PriceService(context);

            // Act
            var result = await service.SetPriceAsync("nonexistent", "s1", 20.0m);

            // Assert
            Assert.False(result);
        }
    }
}
