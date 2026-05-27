using backend.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace backend.Tests.ControllerTests
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsView()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ReturnsView()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}
