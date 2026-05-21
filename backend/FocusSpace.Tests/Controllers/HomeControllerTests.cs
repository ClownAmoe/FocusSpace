using FocusSpace.Api.Controllers;
using FocusSpace.Domain.Entities;
using FocusSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using SystemTask = System.Threading.Tasks.Task;

namespace FocusSpace.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="HomeController"/>.
    /// </summary>
    public class HomeControllerTests
    {
        private static HomeController CreateController()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Planets.Add(new Planet
            {
                Id = 3,
                Name = "Earth",
                OrderNumber = 3,
                Description = "Our home planet"
            });
            context.SaveChanges();

            var userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            var controller = new HomeController(context, userManagerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };
            return controller;
        }

        [Fact]
        public async SystemTask Index_ReturnsView_WithCorrectViewName()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Default view name is null (uses action name)
        }

        [Fact]
        public async SystemTask Index_ReturnsViewResultType_IsIActionResult()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsAssignableFrom<IActionResult>(result);
        }

        [Fact]
        public void HomeController_CanBeInstantiated_Successfully()
        {
            // Act
            var controller = CreateController();

            // Assert
            Assert.NotNull(controller);
            Assert.IsAssignableFrom<Controller>(controller);
        }
    }
}
