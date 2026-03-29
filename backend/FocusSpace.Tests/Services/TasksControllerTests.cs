using DomainTask = FocusSpace.Domain.Entities.Task;
using FocusSpace.Api.Controllers;
using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FocusSpace.Tests.Controllers
{
    public class TasksControllerTests
    {
        // ── Fixture helpers ───────────────────────────────────────────

        private static UserManager<User> CreateUserManager(User fakeUser)
        {
            var store = new Mock<IUserStore<User>>();

            var userManager = new Mock<UserManager<User>>(
                store.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<User>>().Object,
                new IUserValidator<User>[0],
                new IPasswordValidator<User>[0],
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<User>>>().Object);

            userManager
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(fakeUser);

            return userManager.Object;
        }

        private static (TasksController controller, Mock<ITaskService> serviceMock)
            CreateController(int currentUserId = 5)
        {
            var serviceMock = new Mock<ITaskService>();
            var loggerMock = new Mock<ILogger<TasksController>>();
            var fakeUser = new User { Id = currentUserId, UserName = "testuser" };
            var userManager = CreateUserManager(fakeUser);

            var controller = new TasksController(
                serviceMock.Object,
                userManager,
                loggerMock.Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()) },
                            "TestAuth"))
                }
            };

            controller.TempData = new TempDataDictionary(
                controller.ControllerContext.HttpContext,
                new Mock<ITempDataProvider>().Object);

            return (controller, serviceMock);
        }

        private static TaskDto BuildTaskDto(int id = 1, int userId = 5, string title = "Test") => new()
        {
            Id = id,
            UserId = userId,
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ═════════════════════════════════════════════════════════════
        // Index
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async System.Threading.Tasks.Task Index_Always_ReturnsViewWithTaskList()
        {
            var (controller, serviceMock) = CreateController();
            var tasks = new[] { BuildTaskDto(1), BuildTaskDto(2) };
            serviceMock.Setup(s => s.GetTasksByUserIdAsync(5)).ReturnsAsync(tasks);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TaskDto>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        // ═════════════════════════════════════════════════════════════
        // Details
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async System.Threading.Tasks.Task Details_ExistingOwnTask_ReturnsView()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(BuildTaskDto(1, userId: 5));

            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskDto>(view.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async System.Threading.Tasks.Task Details_TaskBelongsToOtherUser_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(BuildTaskDto(1, userId: 99));

            var result = await controller.Details(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Details_NonExistingId_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(999)).ReturnsAsync((TaskDto?)null);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // Create GET
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var (controller, _) = CreateController();

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // Create POST
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_ValidDto_RedirectsToIndex()
        {
            var (controller, serviceMock) = CreateController();
            var dto = new CreateTaskDto { Title = "New task" };
            serviceMock
                .Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                .ReturnsAsync(BuildTaskDto(10, title: "New task"));

            var result = await controller.Create(dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            serviceMock.Verify(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_InvalidModelState_ReturnsViewWithDto()
        {
            var (controller, _) = CreateController();
            controller.ModelState.AddModelError("Title", "Required");
            var dto = new CreateTaskDto { Title = "" };

            var result = await controller.Create(dto);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(dto, view.Model);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_ServiceThrowsArgumentException_ReturnsViewWithError()
        {
            var (controller, serviceMock) = CreateController();
            var dto = new CreateTaskDto { Title = "Bad" };
            serviceMock
                .Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                .ThrowsAsync(new ArgumentException("Title cannot be empty."));

            var result = await controller.Create(dto);

            var view = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        // ═════════════════════════════════════════════════════════════
        // Edit GET
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_ExistingOwnTask_ReturnsViewWithDto()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(3))
                       .ReturnsAsync(BuildTaskDto(3, userId: 5, title: "My task"));

            var result = await controller.Edit(3);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateTaskDto>(view.Model);
            Assert.Equal(3, model.Id);
            Assert.Equal("My task", model.Title);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NonExistingId_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(999)).ReturnsAsync((TaskDto?)null);

            var result = await controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // Edit POST
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ValidDto_RedirectsToIndex()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            var dto = new UpdateTaskDto { Id = 3, Title = "Updated" };
            serviceMock.Setup(s => s.GetTaskByIdAsync(3)).ReturnsAsync(BuildTaskDto(3, userId: 5));
            serviceMock.Setup(s => s.UpdateTaskAsync(dto)).ReturnsAsync(BuildTaskDto(3, title: "Updated"));

            var result = await controller.Edit(3, dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            var (controller, _) = CreateController();
            var dto = new UpdateTaskDto { Id = 5, Title = "Title" };

            var result = await controller.Edit(99, dto);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_TaskBelongsToOtherUser_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            var dto = new UpdateTaskDto { Id = 3, Title = "Title" };
            serviceMock.Setup(s => s.GetTaskByIdAsync(3)).ReturnsAsync(BuildTaskDto(3, userId: 99));

            var result = await controller.Edit(3, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ServiceReturnsNull_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            var dto = new UpdateTaskDto { Id = 3, Title = "Title" };
            serviceMock.Setup(s => s.GetTaskByIdAsync(3)).ReturnsAsync(BuildTaskDto(3, userId: 5));
            serviceMock.Setup(s => s.UpdateTaskAsync(dto)).ReturnsAsync((TaskDto?)null);

            var result = await controller.Edit(3, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ServiceThrowsArgumentException_ReturnsViewWithError()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            var dto = new UpdateTaskDto { Id = 3, Title = "" };
            serviceMock.Setup(s => s.GetTaskByIdAsync(3)).ReturnsAsync(BuildTaskDto(3, userId: 5));
            serviceMock.Setup(s => s.UpdateTaskAsync(It.IsAny<UpdateTaskDto>()))
                       .ThrowsAsync(new ArgumentException("Title cannot be empty."));

            var result = await controller.Edit(3, dto);

            var view = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        // ═════════════════════════════════════════════════════════════
        // Delete GET
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async System.Threading.Tasks.Task Delete_Get_ExistingOwnTask_ReturnsView()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(2)).ReturnsAsync(BuildTaskDto(2, userId: 5));

            var result = await controller.Delete(2);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<TaskDto>(view.Model);
        }

        [Fact]
        public async System.Threading.Tasks.Task Delete_Get_NonExistingId_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(999)).ReturnsAsync((TaskDto?)null);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Delete_Get_TaskBelongsToOtherUser_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(2)).ReturnsAsync(BuildTaskDto(2, userId: 99));

            var result = await controller.Delete(2);

            Assert.IsType<NotFoundResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // DeleteConfirmed POST
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async System.Threading.Tasks.Task DeleteConfirmed_ExistingOwnTask_RedirectsToIndex()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(4)).ReturnsAsync(BuildTaskDto(4, userId: 5));
            serviceMock.Setup(s => s.DeleteTaskAsync(4)).ReturnsAsync(true);

            var result = await controller.DeleteConfirmed(4);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            serviceMock.Verify(s => s.DeleteTaskAsync(4), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteConfirmed_TaskBelongsToOtherUser_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(4)).ReturnsAsync(BuildTaskDto(4, userId: 99));

            var result = await controller.DeleteConfirmed(4);

            Assert.IsType<NotFoundResult>(result);
            serviceMock.Verify(s => s.DeleteTaskAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteConfirmed_NonExistingId_ReturnsNotFound()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(999)).ReturnsAsync((TaskDto?)null);

            var result = await controller.DeleteConfirmed(999);

            Assert.IsType<NotFoundResult>(result);
            serviceMock.Verify(s => s.DeleteTaskAsync(It.IsAny<int>()), Times.Never);
        }
    }
}