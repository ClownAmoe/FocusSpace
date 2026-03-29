using FocusSpace.Api.Controllers;
using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FocusSpace.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="SessionController"/>.
    /// </summary>
    public class SessionControllerTests
    {
        private static SessionController CreateController(Mock<ISessionService>? serviceMock = null)
        {
            serviceMock ??= new Mock<ISessionService>();
            return new SessionController(serviceMock.Object);
        }

        // ═════════════════════════════════════════════════════════════
        // Index
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void Index_ReturnsView()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        // ═════════════════════════════════════════════════════════════
        // Start
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Start_ValidDto_ReturnsOkWithSessionId()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var dto = new CreateSessionDto { UserId = 1, TaskId = 1, PlannedDuration = TimeSpan.FromSeconds(3600) };
            serviceMock.Setup(s => s.StartSessionAsync(It.IsAny<CreateSessionDto>()))
                .ReturnsAsync(1);

            var controller = CreateController(serviceMock);

            // Act
            var result = await controller.Start(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            serviceMock.Verify(s => s.StartSessionAsync(It.IsAny<CreateSessionDto>()), Times.Once);
        }

        [Fact]
        public async Task Start_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var controller = CreateController(serviceMock);
            controller.ModelState.AddModelError("UserId", "UserId is required");
            var dto = new CreateSessionDto();

            // Act
            var result = await controller.Start(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Start_NullDto_CompletesWithoutError()
        {
            // Arrange
            var controller = CreateController();
            CreateSessionDto? dto = null;

            // Act & Assert
            // The mocked service will return a successful result even with null dto
#pragma warning disable CS8604
            var result = await controller.Start(dto);
#pragma warning restore CS8604

            // Verify the action completed and returned a result
            Assert.NotNull(result);
        }

        // ═════════════════════════════════════════════════════════════
        // Complete
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Complete_ValidDto_ReturnsOk()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var dto = new UpdateSessionDto { Id = 1, Status = "Completed", EndTime = DateTime.UtcNow };
            serviceMock.Setup(s => s.CompleteSessionAsync(It.IsAny<UpdateSessionDto>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController(serviceMock);

            // Act
            var result = await controller.Complete(dto);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            serviceMock.Verify(s => s.CompleteSessionAsync(It.IsAny<UpdateSessionDto>()), Times.Once);
        }

        [Fact]
        public async Task Complete_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var controller = CreateController(serviceMock);
            controller.ModelState.AddModelError("SessionId", "SessionId is required");
            var dto = new UpdateSessionDto();

            // Act
            var result = await controller.Complete(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        // ═════════════════════════════════════════════════════════════
        // Pause
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Pause_ValidSessionId_ReturnsOk()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            int sessionId = 1;
            serviceMock.Setup(s => s.PauseSessionAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController(serviceMock);

            // Act
            var result = await controller.Pause(sessionId);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            serviceMock.Verify(s => s.PauseSessionAsync(sessionId), Times.Once);
        }

        [Fact]
        public async Task Pause_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var controller = CreateController(serviceMock);
            controller.ModelState.AddModelError("sessionId", "Invalid session ID");
            int sessionId = 0;

            // Act
            var result = await controller.Pause(sessionId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        // ═════════════════════════════════════════════════════════════
        // Resume
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task Resume_ValidSessionId_ReturnsOk()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            int sessionId = 1;
            serviceMock.Setup(s => s.ResumeSessionAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController(serviceMock);

            // Act
            var result = await controller.Resume(sessionId);

            // Assert
            var okResult = Assert.IsType<OkResult>(result);
            serviceMock.Verify(s => s.ResumeSessionAsync(sessionId), Times.Once);
        }

        [Fact]
        public async Task Resume_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var serviceMock = new Mock<ISessionService>();
            var controller = CreateController(serviceMock);
            controller.ModelState.AddModelError("sessionId", "Invalid session ID");
            int sessionId = 0;

            // Act
            var result = await controller.Resume(sessionId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }
    }
}
