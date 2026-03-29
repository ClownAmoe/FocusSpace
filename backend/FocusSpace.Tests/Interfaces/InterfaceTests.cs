using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using Moq;
using Xunit;

namespace FocusSpace.Tests.Interfaces
{
    /// <summary>
    /// Tests for interfaces to ensure they are properly defined and mockable.
    /// </summary>
    public class InterfaceTests
    {
        // ?????????????????????????????????????????????????????????????
        // ITaskService
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public void ITaskService_CanBeMocked()
        {
            // Arrange & Act
            var mockService = new Mock<ITaskService>();

            // Assert
            Assert.NotNull(mockService);
            Assert.NotNull(mockService.Object);
        }

        [Fact]
        public void ITaskService_HasAllRequiredMethods()
        {
            // Arrange
            var mockService = new Mock<ITaskService>();

            // Assert - Methods exist and can be called
            var methods = typeof(ITaskService).GetMethods();
            Assert.NotEmpty(methods);
            Assert.Contains(methods, m => m.Name == "GetTasksByUserIdAsync");
            Assert.Contains(methods, m => m.Name == "GetTaskByIdAsync");
            Assert.Contains(methods, m => m.Name == "CreateTaskAsync");
            Assert.Contains(methods, m => m.Name == "UpdateTaskAsync");
            Assert.Contains(methods, m => m.Name == "DeleteTaskAsync");
        }

        // ?????????????????????????????????????????????????????????????
        // ISessionService
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public void ISessionService_CanBeMocked()
        {
            // Arrange & Act
            var mockService = new Mock<ISessionService>();

            // Assert
            Assert.NotNull(mockService);
            Assert.NotNull(mockService.Object);
        }

        [Fact]
        public void ISessionService_HasAllRequiredMethods()
        {
            // Arrange
            var methods = typeof(ISessionService).GetMethods();

            // Assert
            Assert.NotEmpty(methods);
            Assert.Contains(methods, m => m.Name == "StartSessionAsync");
            Assert.Contains(methods, m => m.Name == "CompleteSessionAsync");
            Assert.Contains(methods, m => m.Name == "PauseSessionAsync");
            Assert.Contains(methods, m => m.Name == "ResumeSessionAsync");
        }

        // ?????????????????????????????????????????????????????????????
        // ITaskRepository
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public void ITaskRepository_CanBeMocked()
        {
            // Arrange & Act
            var mockRepo = new Mock<ITaskRepository>();

            // Assert
            Assert.NotNull(mockRepo);
            Assert.NotNull(mockRepo.Object);
        }

        [Fact]
        public void ITaskRepository_HasAllRequiredMethods()
        {
            // Arrange
            var methods = typeof(ITaskRepository).GetMethods();

            // Assert
            Assert.NotEmpty(methods);
            Assert.Contains(methods, m => m.Name == "GetAllByUserIdAsync");
            Assert.Contains(methods, m => m.Name == "GetByIdAsync");
            Assert.Contains(methods, m => m.Name == "CreateAsync");
            Assert.Contains(methods, m => m.Name == "UpdateAsync");
            Assert.Contains(methods, m => m.Name == "DeleteAsync");
            Assert.Contains(methods, m => m.Name == "ExistsAsync");
        }

        // ?????????????????????????????????????????????????????????????
        // ISessionRepository
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public void ISessionRepository_CanBeMocked()
        {
            // Arrange & Act
            var mockRepo = new Mock<ISessionRepository>();

            // Assert
            Assert.NotNull(mockRepo);
            Assert.NotNull(mockRepo.Object);
        }

        [Fact]
        public void ISessionRepository_HasRequiredMethods()
        {
            // Arrange
            var methods = typeof(ISessionRepository).GetMethods();

            // Assert
            Assert.NotEmpty(methods);
            Assert.Contains(methods, m => m.Name == "GetByIdAsync");
            Assert.Contains(methods, m => m.Name == "AddAsync");
            Assert.Contains(methods, m => m.Name == "SaveChangesAsync");
        }

        // ?????????????????????????????????????????????????????????????
        // IEmailService
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public void IEmailService_CanBeMocked()
        {
            // Arrange & Act
            var mockService = new Mock<IEmailService>();

            // Assert
            Assert.NotNull(mockService);
            Assert.NotNull(mockService.Object);
        }

        [Fact]
        public void IEmailService_HasRequiredMethod()
        {
            // Arrange
            var methods = typeof(IEmailService).GetMethods();

            // Assert
            Assert.NotEmpty(methods);
            Assert.Contains(methods, m => m.Name == "SendAsync");
            Assert.Contains(methods, m => m.Name == "SendConfirmationEmailAsync");
            Assert.Contains(methods, m => m.Name == "SendPasswordResetEmailAsync");
        }
    }

    /// <summary>
    /// Tests for mock behavior and verification.
    /// </summary>
    public class MockBehaviorTests
    {
        [Fact]
        public async System.Threading.Tasks.Task TaskRepository_Mock_VerifyMethodCalls()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>();
            mockRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            // Act
            await mockRepo.Object.ExistsAsync(1);

            // Assert
            mockRepo.Verify(r => r.ExistsAsync(1), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task TaskRepository_Mock_VerifyMethodNeverCalled()
        {
            // Arrange
            var mockRepo = new Mock<ITaskRepository>();

            // Act - method not called

            // Assert
            mockRepo.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async System.Threading.Tasks.Task SessionRepository_Mock_SetupMultipleCalls()
        {
            // Arrange
            var mockRepo = new Mock<ISessionRepository>();
            var session = new Session { Id = 1 };

            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
            mockRepo.Setup(r => r.SaveChangesAsync()).Returns(System.Threading.Tasks.Task.CompletedTask);

            // Act
            var result = await mockRepo.Object.GetByIdAsync(1);
            await mockRepo.Object.SaveChangesAsync();

            // Assert
            Assert.NotNull(result);
            mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async System.Threading.Tasks.Task Service_Mock_VerifyArgumentsPassed()
        {
            // Arrange
            var mockService = new Mock<ITaskService>();
            var mockRepo = new Mock<ITaskRepository>();

            var taskEntity = new FocusSpace.Domain.Entities.Task { Id = 1, Title = "Test" };
            mockRepo.Setup(r => r.CreateAsync(It.IsAny<FocusSpace.Domain.Entities.Task>()))
                    .ReturnsAsync((FocusSpace.Domain.Entities.Task t) => { t.Id = 1; return t; });

            // Act - simulating a call with specific arguments
            await mockRepo.Object.CreateAsync(taskEntity);

            // Assert
            mockRepo.Verify(
                r => r.CreateAsync(It.Is<FocusSpace.Domain.Entities.Task>(t => t.Title == "Test")),
                Times.Once);
        }
    }
}
