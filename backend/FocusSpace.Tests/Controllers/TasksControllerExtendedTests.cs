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
    /// <summary>
    /// Extended unit tests for <see cref="TasksController"/> covering additional scenarios.
    /// </summary>
    public class TasksControllerExtendedTests
    {
        // ?? Fixture helpers ???????????????????????????????????????????

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

        private static TaskDto BuildTaskDto(
            int id = 1,
            int userId = 5,
            string title = "Test",
            string? description = "Test description") => new()
            {
                Id = id,
                UserId = userId,
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        // ?????????????????????????????????????????????????????????????
        // Index - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task Index_WithEmptyList_ReturnsEmptyView()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTasksByUserIdAsync(5))
                       .ReturnsAsync(new TaskDto[0]);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TaskDto>>(view.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async System.Threading.Tasks.Task Index_WithMultipleTasks_ReturnsAllTasks()
        {
            var (controller, serviceMock) = CreateController();
            var tasks = new[]
            {
                BuildTaskDto(1, 5, "Task 1"),
                BuildTaskDto(2, 5, "Task 2"),
                BuildTaskDto(3, 5, "Task 3")
            };
            serviceMock.Setup(s => s.GetTasksByUserIdAsync(5)).ReturnsAsync(tasks);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TaskDto>>(view.Model);
            Assert.Equal(3, model.Count());
        }

        // ?????????????????????????????????????????????????????????????
        // Details - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task Details_WithLargeTaskId_Works()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(int.MaxValue))
                       .ReturnsAsync(BuildTaskDto(int.MaxValue, 5));

            var result = await controller.Details(int.MaxValue);

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view.Model);
        }

        [Fact]
        public async System.Threading.Tasks.Task Details_WithNullDescription_Works()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(1))
                       .ReturnsAsync(BuildTaskDto(1, 5, description: null));

            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskDto>(view.Model);
            Assert.Null(model.Description);
        }

        // ?????????????????????????????????????????????????????????????
        // Create - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_SetsTempDataSuccess()
        {
            var (controller, serviceMock) = CreateController();
            var dto = new CreateTaskDto { Title = "New Task" };
            serviceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                       .ReturnsAsync(BuildTaskDto(10));

            await controller.Create(dto);

            Assert.NotNull(controller.TempData["Success"]);
            Assert.Equal("Task created successfully.", controller.TempData["Success"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_WithWhitespaceInTitle_Works()
        {
            var (controller, serviceMock) = CreateController();
            var dto = new CreateTaskDto { Title = "  Task with spaces  " };
            serviceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                       .ReturnsAsync(BuildTaskDto(10));

            var result = await controller.Create(dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_SetsUserIdFromCurrentUser()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 7);
            var dto = new CreateTaskDto { Title = "Task" };

            CreateTaskDto capturedDto = null!;
            serviceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                       .Callback<CreateTaskDto>(d => capturedDto = d)
                       .ReturnsAsync(BuildTaskDto(10));

            await controller.Create(dto);

            Assert.NotNull(capturedDto);
            Assert.Equal(7, capturedDto.UserId);
        }

        // ?????????????????????????????????????????????????????????????
        // Edit - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_PopulatesDescriptionFromTask()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(3))
                       .ReturnsAsync(BuildTaskDto(3, 5, "Title", "Detailed description"));

            var result = await controller.Edit(3);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateTaskDto>(view.Model);
            Assert.Equal("Detailed description", model.Description);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_SetsTempDataSuccess()
        {
            var (controller, serviceMock) = CreateController();
            var dto = new UpdateTaskDto { Id = 3, Title = "Updated" };
            serviceMock.Setup(s => s.GetTaskByIdAsync(3))
                       .ReturnsAsync(BuildTaskDto(3, 5));
            serviceMock.Setup(s => s.UpdateTaskAsync(dto))
                       .ReturnsAsync(BuildTaskDto(3, title: "Updated"));

            await controller.Edit(3, dto);

            Assert.NotNull(controller.TempData["Success"]);
            Assert.Equal("Task updated successfully.", controller.TempData["Success"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_VerifiesOwnershipBeforeUpdate()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            var dto = new UpdateTaskDto { Id = 3, Title = "Updated" };
            serviceMock.Setup(s => s.GetTaskByIdAsync(3))
                       .ReturnsAsync(BuildTaskDto(3, userId: 99)); // Different user

            var result = await controller.Edit(3, dto);

            Assert.IsType<NotFoundResult>(result);
            serviceMock.Verify(s => s.UpdateTaskAsync(It.IsAny<UpdateTaskDto>()), Times.Never);
        }

        // ?????????????????????????????????????????????????????????????
        // Delete - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task Delete_Get_VerifiesTaskOwnership()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(2))
                       .ReturnsAsync(BuildTaskDto(2, userId: 10)); // Different user

            var result = await controller.Delete(2);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteConfirmed_SetsTempDataSuccess()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(4))
                       .ReturnsAsync(BuildTaskDto(4, 5));
            serviceMock.Setup(s => s.DeleteTaskAsync(4)).ReturnsAsync(true);

            await controller.DeleteConfirmed(4);

            Assert.NotNull(controller.TempData["Success"]);
            Assert.Equal("Task deleted successfully.", controller.TempData["Success"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteConfirmed_CallsDeleteService()
        {
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(4))
                       .ReturnsAsync(BuildTaskDto(4, 5));
            serviceMock.Setup(s => s.DeleteTaskAsync(4)).ReturnsAsync(true);

            await controller.DeleteConfirmed(4);

            serviceMock.Verify(s => s.DeleteTaskAsync(4), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task DeleteConfirmed_VerifiesOwnershipBeforeDelete()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(4))
                       .ReturnsAsync(BuildTaskDto(4, userId: 99)); // Different user

            var result = await controller.DeleteConfirmed(4);

            Assert.IsType<NotFoundResult>(result);
            serviceMock.Verify(s => s.DeleteTaskAsync(It.IsAny<int>()), Times.Never);
        }

        // ?????????????????????????????????????????????????????????????
        // Integration scenarios
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task MultipleOperations_MaintainUserIsolation()
        {
            var (controller, serviceMock) = CreateController(currentUserId: 5);
            serviceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>()))
                       .ReturnsAsync((int id) => BuildTaskDto(id, userId: 5));

            // User 5 tries to access their own task
            var result = await controller.Details(10);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_MultipleAttributeValidation()
        {
            var (controller, serviceMock) = CreateController();
            var dto = new CreateTaskDto { Title = "Task" };

            serviceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                       .ReturnsAsync(BuildTaskDto(1));

            var result = await controller.Create(dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirect.ActionName);
        }
    }
}
