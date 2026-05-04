using FocusSpace.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FocusSpace.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="HomeController"/>.
    /// </summary>
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsView_WithCorrectViewName()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Default view name is null (uses action name)
        }

        [Fact]
        public void Index_ReturnsViewResultType_IsIActionResult()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
        }

        [Fact]
        public void HomeController_CanBeInstantiated_Successfully()
        {
            // Act
            var controller = new HomeController();

            // Assert
            Assert.NotNull(controller);
            Assert.IsAssignableFrom<Controller>(controller);
        }
    }
}
