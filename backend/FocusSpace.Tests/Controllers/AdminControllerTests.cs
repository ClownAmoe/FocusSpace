using FocusSpace.Api.Controllers;
using FocusSpace.Domain.Enums;
using FocusSpace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using DomainTask = FocusSpace.Domain.Entities.Task;
using User = FocusSpace.Domain.Entities.User;

namespace FocusSpace.Tests.Controllers
{
sdalsakdalkasl
    /// <summary>
    /// Unit tests for <see cref="AdminController"/>.
    /// </summary>
    public class AdminControllerTests
    {
        // ── Fixture helpers ───────────────────────────────────────────

        private static DbContextOptions<AppDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private static UserManager<FocusSpace.Domain.Entities.User> CreateUserManager(FocusSpace.Domain.Entities.User user)
        {
            var store = new Mock<IUserStore<FocusSpace.Domain.Entities.User>>();
            var userManager = new Mock<UserManager<FocusSpace.Domain.Entities.User>>(
                store.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            if (user != null)
            {
                userManager
                    .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                    .ReturnsAsync(user);

                userManager
                    .Setup(m => m.FindByIdAsync(user.Id.ToString()))
                    .ReturnsAsync(user);
            }

            return userManager.Object;
        }

        private static AdminController CreateController(
            AppDbContext context,
            UserManager<FocusSpace.Domain.Entities.User> userManager,
            FocusSpace.Domain.Entities.User? currentUser = null)
        {
            var loggerMock = new Mock<ILogger<AdminController>>();
            var controller = new AdminController(context, userManager, loggerMock.Object);

            // Set up user context
            if (currentUser != null)
            {
                var claims = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, currentUser!.UserName!) }));
                controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
                {
                    HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = claims }
                };
            }

            return controller;
        }

        private static FocusSpace.Domain.Entities.User BuildUser(int id = 1, string email = "test@example.com", bool isApproved = false, bool isBlocked = false)
        {
            return new FocusSpace.Domain.Entities.User
            {
                Id = id,
                UserName = $"user{id}",
                Email = email,
                IsApproved = isApproved,
                IsBlocked = isBlocked,
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                Role = UserRole.User
            };
        }

        // ═════════════════════════════════════════════════════════════
        // ApproveUser
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task ApproveUser_UserExists_ApprovesAndReturnsOk()
        {
            // Arrange
            var user = BuildUser(1, "user1@example.com", false);

            var userManager = CreateUserManager(user);
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            var successResult = IdentityResult.Success;
            mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            mockUserManager
                .Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(successResult);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, mockUserManager.Object, user);

            // Act
            var result = await controller.ApproveUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ApproveUser_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, mockUserManager.Object);

            // Act
            var result = await controller.ApproveUser(999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        // ═════════════════════════════════════════════════════════════
        // BlockUser
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task BlockUser_ValidUser_BlocksAndReturnsOk()
        {
            // Arrange
            var targetUser = BuildUser(2, "target@example.com");
            var currentUser = BuildUser(1, "admin@example.com");

            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(targetUser);
            mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);
            mockUserManager
                .Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            mockUserManager
                .Setup(m => m.UpdateSecurityStampAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, mockUserManager.Object, currentUser);

            // Act
            var result = await controller.BlockUser(2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task BlockUser_CurrentUser_ReturnsBadRequest()
        {
            // Arrange
            var currentUser = BuildUser(1, "admin@example.com");

            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(currentUser);
            mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, mockUserManager.Object, currentUser);

            // Act
            var result = await controller.BlockUser(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        // ═════════════════════════════════════════════════════════════
        // UnblockUser
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task UnblockUser_BlockedUser_UnblocksAndReturnsOk()
        {
            // Arrange
            var blockedUser = BuildUser(2, "blocked@example.com", isBlocked: true);
            var currentUser = BuildUser(1, "admin@example.com");

            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(blockedUser);
            mockUserManager
                .Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, mockUserManager.Object, currentUser);

            // Act
            var result = await controller.UnblockUser(2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        // ═════════════════════════════════════════════════════════════
        // PromoteToAdmin
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task PromoteToAdmin_ValidUser_PromotesAndReturnsOk()
        {
            // Arrange
            var targetUser = BuildUser(2, "user@example.com");
            var currentUser = BuildUser(1, "admin@example.com");

            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(targetUser);
            mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);
            mockUserManager
                .Setup(m => m.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(false);
            mockUserManager
                .Setup(m => m.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            mockUserManager
                .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            mockUserManager
                .Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, mockUserManager.Object, currentUser);

            // Act
            var result = await controller.PromoteToAdmin(2);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task PromoteToAdmin_CurrentUser_ReturnsBadRequest()
        {
            // Arrange
            var currentUser = BuildUser(1, "admin@example.com");

            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            mockUserManager
                .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(currentUser);
            mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, mockUserManager.Object, currentUser);

            // Act
            var result = await controller.PromoteToAdmin(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        // ═════════════════════════════════════════════════════════════
        // Index
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Index_ReturnsViewWithStatistics()
        {
            // Arrange
            var context = new AppDbContext(CreateInMemoryOptions());
            var currentUser = BuildUser(999, "admin@example.com", true);
            var userManager = CreateUserManager(currentUser);
            var controller = CreateController(context, userManager, currentUser);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
            Assert.NotNull(viewResult.ViewData);
        }

        [Fact]
        public async Task Index_SetsCorrectViewBagValues()
        {
            // Arrange
            var context = new AppDbContext(CreateInMemoryOptions());
            var currentUser = BuildUser(999, "admin@example.com", true);
            var userManager = CreateUserManager(currentUser);
            var controller = CreateController(context, userManager, currentUser);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["TotalUsers"]);
            Assert.NotNull(viewResult.ViewData["ActiveTasks"]);
            Assert.NotNull(viewResult.ViewData["ActiveSessions"]);
            Assert.NotNull(viewResult.ViewData["BlockedUsers"]);
            Assert.NotNull(viewResult.ViewData["PendingApproval"]);
        }

        // ═════════════════════════════════════════════════════════════
        // GetUsers
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetUsers_NoUsers_ReturnsEmptyArray()
        {
            // Arrange
            var context = new AppDbContext(CreateInMemoryOptions());
            var currentUser = BuildUser(999, "admin@example.com", true);
            var userManager = CreateUserManager(currentUser);
            var controller = CreateController(context, userManager, currentUser);

            // Act
            var result = await controller.GetUsers();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        [Fact]
        public async Task GetUsers_WithUsers_ReturnsUserData()
        {
            // Arrange
            var context = new AppDbContext(CreateInMemoryOptions());
            var currentUser = BuildUser(999, "admin@example.com", true);
            var userManager = CreateUserManager(currentUser);
            var user = BuildUser(100, "user@example.com", false);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = CreateController(context, userManager, currentUser);

            // Act
            var result = await controller.GetUsers();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }

        // ═════════════════════════════════════════════════════════════
        // Additional Edge Cases
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task ApproveUser_UpdateFails_ReturnsBadRequest()
        {
            // Arrange
            var user = BuildUser(2, "user2@example.com");
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

            userManagerMock
                .Setup(m => m.FindByIdAsync("2"))
                .ReturnsAsync(user);

            var errorResult = IdentityResult.Failed(new IdentityError { Description = "Update failed" });
            userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(errorResult);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, userManagerMock.Object);

            // Act
            var result = await controller.ApproveUser(2);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task BlockUser_ValidUser_UpdatesIsBlocked()
        {
            // Arrange
            var targetUser = BuildUser(3, "target@example.com");
            var currentUser = BuildUser(1, "admin@example.com");

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

            userManagerMock
                .Setup(m => m.FindByIdAsync("3"))
                .ReturnsAsync(targetUser);

            userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);

            userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .Returns(Task.FromResult(IdentityResult.Success));

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, userManagerMock.Object, currentUser);

            // Act
            var result = await controller.BlockUser(3);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UnblockUser_AlreadyUnblocked_ReturnsOk()
        {
            // Arrange
            var unblockedUser = BuildUser(4, "unblocked@example.com");
            unblockedUser.IsBlocked = false;
            var currentUser = BuildUser(1, "admin@example.com");

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

            userManagerMock
                .Setup(m => m.FindByIdAsync("4"))
                .ReturnsAsync(unblockedUser);

            userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var context = new AppDbContext(CreateInMemoryOptions());
            var controller = CreateController(context, userManagerMock.Object, currentUser);

            // Act
            var result = await controller.UnblockUser(4);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}

