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
    }
}
