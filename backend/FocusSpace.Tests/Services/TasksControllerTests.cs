using FocusSpace.Api.Controllers;
using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FocusSpace.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="TasksController"/>.
    /// Uses Moq to isolate the controller from ITaskService.
    /// </summary>
    public class TasksControllerTests
    {
        // ── Fixture helpers ───────────────────────────────────────────

        private static (TasksController controller, Mock<ITaskService> serviceMock) CreateController()
        {
            var serviceMock = new Mock<ITaskService>();
            var loggerMock = new Mock<ILogger<TasksController>>();
            var controller = new TasksController(serviceMock.Object, loggerMock.Object);
            return (controller, serviceMock);
        }

        private static TaskDto BuildTaskDto(int id = 1, string title = "Test") => new()
        {
            Id = id,
            UserId = 1,
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ═════════════════════════════════════════════════════════════
        // Index
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Index_Always_ReturnsViewWithTaskList()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            var tasks = new[] { BuildTaskDto(1, "A"), BuildTaskDto(2, "B") };
            serviceMock.Setup(s => s.GetTasksByUserIdAsync(It.IsAny<int>())).ReturnsAsync(tasks);

            // Act
            var result = await controller.Index();

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TaskDto>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        // ═════════════════════════════════════════════════════════════
        // Details
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Details_ExistingId_ReturnsViewWithTask()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(5)).ReturnsAsync(BuildTaskDto(5));

            // Act
            var result = await controller.Details(5);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TaskDto>(view.Model);
            Assert.Equal(5, model.Id);
        }

        [Fact]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskDto?)null);

            // Act
            var result = await controller.Details(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // Create GET
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Arrange
            var (controller, _) = CreateController();

            // Act
            var result = controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // Create POST
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Create_Post_ValidDto_RedirectsToIndex()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            var dto = new CreateTaskDto { Title = "New task" };
            serviceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                       .ReturnsAsync(BuildTaskDto(10, "New task"));

            // Act
            var result = await controller.Create(dto);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            serviceMock.Verify(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()), Times.Once);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithDto()
        {
            // Arrange
            var (controller, _) = CreateController();
            controller.ModelState.AddModelError("Title", "Required");
            var dto = new CreateTaskDto { Title = "" };

            // Act
            var result = await controller.Create(dto);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(dto, view.Model);
        }

        [Fact]
        public async Task Create_Post_ServiceThrowsArgumentException_ReturnsViewWithError()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            var dto = new CreateTaskDto { Title = "Bad" };
            serviceMock.Setup(s => s.CreateTaskAsync(It.IsAny<CreateTaskDto>()))
                       .ThrowsAsync(new ArgumentException("Title cannot be empty."));

            // Act
            var result = await controller.Create(dto);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        // ═════════════════════════════════════════════════════════════
        // Edit GET
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Edit_Get_ExistingId_ReturnsViewWithDto()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(3)).ReturnsAsync(BuildTaskDto(3, "My task"));

            // Act
            var result = await controller.Edit(3);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateTaskDto>(view.Model);
            Assert.Equal(3, model.Id);
            Assert.Equal("My task", model.Title);
        }

        [Fact]
        public async Task Edit_Get_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskDto?)null);

            // Act
            var result = await controller.Edit(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // Edit POST
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Edit_Post_ValidDto_RedirectsToIndex()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            var dto = new UpdateTaskDto { Id = 3, Title = "Updated" };
            serviceMock.Setup(s => s.UpdateTaskAsync(dto)).ReturnsAsync(BuildTaskDto(3, "Updated"));

            // Act
            var result = await controller.Edit(3, dto);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_IdMismatch_ReturnsBadRequest()
        {
            // Arrange
            var (controller, _) = CreateController();
            var dto = new UpdateTaskDto { Id = 5, Title = "Title" };

            // Act
            var result = await controller.Edit(99, dto);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            var dto = new UpdateTaskDto { Id = 3, Title = "Title" };
            serviceMock.Setup(s => s.UpdateTaskAsync(dto)).ReturnsAsync((TaskDto?)null);

            // Act
            var result = await controller.Edit(3, dto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithDto()
        {
            // Arrange
            var (controller, _) = CreateController();
            controller.ModelState.AddModelError("Title", "Required");
            var dto = new UpdateTaskDto { Id = 3, Title = "" };

            // Act
            var result = await controller.Edit(3, dto);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(dto, view.Model);
        }

        // ═════════════════════════════════════════════════════════════
        // Delete GET
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Delete_Get_ExistingId_ReturnsViewWithTask()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(2)).ReturnsAsync(BuildTaskDto(2));

            // Act
            var result = await controller.Delete(2);

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<TaskDto>(view.Model);
        }

        [Fact]
        public async Task Delete_Get_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<int>())).ReturnsAsync((TaskDto?)null);

            // Act
            var result = await controller.Delete(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // ═════════════════════════════════════════════════════════════
        // DeleteConfirmed POST
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task DeleteConfirmed_ExistingTask_RedirectsToIndex()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.DeleteTaskAsync(4)).ReturnsAsync(true);

            // Act
            var result = await controller.DeleteConfirmed(4);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            serviceMock.Verify(s => s.DeleteTaskAsync(4), Times.Once);
        }

        [Fact]
        public async Task DeleteConfirmed_NonExistingTask_ReturnsNotFound()
        {
            // Arrange
            var (controller, serviceMock) = CreateController();
            serviceMock.Setup(s => s.DeleteTaskAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await controller.DeleteConfirmed(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
