using backend.Controllers;
using backend.Data;
using CloudNativeShop.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.ControllerTests
{
    public class CategoryControllerTests
    {
        private (ShopContext context, CategoryController controller) CreateController(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<ShopContext>()
                .UseSqlite(connection)
                .Options;

            var context = new ShopContext(options);
            context.Database.EnsureCreated();

            var controller = new CategoryController(context);
            return (context, controller);
        }

        [Fact]
        public async Task Index_ReturnsAllCategories()
        {
            // Arrange
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var (context, controller) = CreateController(connection);

            context.Category.AddRange(
                new Category { ProductCategoryName = "cat1" },
                new Category { ProductCategoryName = "cat2" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.Index();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
            Assert.Equal(2, categories.Count());
        }
    }
}
