using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using FocusSpace.Application.Services;
using Moq;
using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="TaskService"/>.
    /// Follows Arrange / Act / Assert pattern and the AAA naming convention:
    /// MethodName_StateUnderTest_ExpectedBehavior
    /// </summary>
    public class TaskServiceTests
    {
        // ── Fixture helpers ───────────────────────────────────────────

        private static TaskService CreateService(Mock<ITaskRepository> repoMock) =>
            new(repoMock.Object);

        private static DomainTask BuildTask(int id = 1, int userId = 10, string title = "Test task") =>
            new()
            {
                Id = id,
                UserId = userId,
                Title = title,
                Description = "Some description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        // ═════════════════════════════════════════════════════════════
        // GetTasksByUserIdAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetTasksByUserIdAsync_ValidUserId_ReturnsMappedDtos()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetAllByUserIdAsync(10))
                    .ReturnsAsync(new[] { BuildTask(1, 10, "Task A"), BuildTask(2, 10, "Task B") });

            var service = CreateService(repoMock);

            // Act
            var result = (await service.GetTasksByUserIdAsync(10)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Task A", result[0].Title);
            Assert.Equal("Task B", result[1].Title);
            Assert.All(result, dto => Assert.Equal(10, dto.UserId));
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_EmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetAllByUserIdAsync(99))
                    .ReturnsAsync(Array.Empty<DomainTask>());

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetTasksByUserIdAsync(99);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task GetTasksByUserIdAsync_InvalidUserId_ThrowsArgumentException(int invalidId)
        {
            // Arrange
            var service = CreateService(new Mock<ITaskRepository>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetTasksByUserIdAsync(invalidId));
        }

        // ═════════════════════════════════════════════════════════════
        // GetTaskByIdAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetTaskByIdAsync_ExistingId_ReturnsMappedDto()
        {
            // Arrange
            var task = BuildTask(5, 10, "My Task");
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(task);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetTaskByIdAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("My Task", result.Title);
        }

        [Fact]
        public async Task GetTaskByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((DomainTask?)null);

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetTaskByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetTaskByIdAsync_InvalidId_ThrowsArgumentException(int invalidId)
        {
            var service = CreateService(new Mock<ITaskRepository>());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetTaskByIdAsync(invalidId));
        }

        // ═════════════════════════════════════════════════════════════
        // CreateTaskAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task CreateTaskAsync_ValidDto_CreatesAndReturnsMappedDto()
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 10, Title = "New task", Description = "Details" };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => { t.Id = 42; return t; });

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateTaskAsync(dto);

            // Assert
            Assert.Equal(42, result.Id);
            Assert.Equal("New task", result.Title);
            Assert.Equal(10, result.UserId);
            repoMock.Verify(r => r.CreateAsync(It.IsAny<DomainTask>()), Times.Once);
        }

        [Fact]
        public async Task CreateTaskAsync_TitleWithWhitespace_TrimsTitle()
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 10, Title = "  Padded Title  " };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.CreateAsync(It.Is<DomainTask>(t => t.Title == "Padded Title")))
                    .ReturnsAsync((DomainTask t) => { t.Id = 1; return t; });

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateTaskAsync(dto);

            // Assert
            Assert.Equal("Padded Title", result.Title);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CreateTaskAsync_EmptyTitle_ThrowsArgumentException(string? emptyTitle)
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 10, Title = emptyTitle! };
            var service = CreateService(new Mock<ITaskRepository>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(dto));
        }

        [Fact]
        public async Task CreateTaskAsync_NullDto_ThrowsArgumentNullException()
        {
            var service = CreateService(new Mock<ITaskRepository>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateTaskAsync(null!));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CreateTaskAsync_InvalidUserId_ThrowsArgumentException(int badUserId)
        {
            var dto = new CreateTaskDto { UserId = badUserId, Title = "Valid title" };
            var service = CreateService(new Mock<ITaskRepository>());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(dto));
        }

        // ═════════════════════════════════════════════════════════════
        // UpdateTaskAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task UpdateTaskAsync_ExistingTask_UpdatesAndReturnsDto()
        {
            // Arrange
            var existing = BuildTask(7, 10, "Old title");
            var dto = new UpdateTaskDto { Id = 7, Title = "New title", Description = "Updated desc" };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(existing);
            repoMock.Setup(r => r.UpdateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => t);

            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateTaskAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New title", result.Title);
            Assert.Equal("Updated desc", result.Description);
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<DomainTask>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_NonExistingTask_ReturnsNull()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((DomainTask?)null);
            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateTaskAsync(new UpdateTaskDto { Id = 99, Title = "Anything" });

            // Assert
            Assert.Null(result);
            repoMock.Verify(r => r.UpdateAsync(It.IsAny<DomainTask>()), Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public async Task UpdateTaskAsync_EmptyTitle_ThrowsArgumentException(string emptyTitle)
        {
            var service = CreateService(new Mock<ITaskRepository>());
            var dto = new UpdateTaskDto { Id = 1, Title = emptyTitle };

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateTaskAsync(dto));
        }

        // ═════════════════════════════════════════════════════════════
        // DeleteTaskAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task DeleteTaskAsync_ExistingTask_DeletesAndReturnsTrue()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.ExistsAsync(3)).ReturnsAsync(true);
            repoMock.Setup(r => r.DeleteAsync(3)).Returns(Task.CompletedTask);

            var service = CreateService(repoMock);

            // Act
            var result = await service.DeleteTaskAsync(3);

            // Assert
            Assert.True(result);
            repoMock.Verify(r => r.DeleteAsync(3), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_NonExistingTask_ReturnsFalse()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            var service = CreateService(repoMock);

            // Act
            var result = await service.DeleteTaskAsync(999);

            // Assert
            Assert.False(result);
            repoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteTaskAsync_InvalidId_ThrowsArgumentException(int badId)
        {
            var service = CreateService(new Mock<ITaskRepository>());

            await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteTaskAsync(badId));
        }
    }
}
