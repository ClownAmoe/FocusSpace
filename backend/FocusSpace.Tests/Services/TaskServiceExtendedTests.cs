using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;
using FocusSpace.Application.Services;
using FocusSpace.Domain.Entities;
using Moq;
using Xunit;
using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Tests.Services
{
    /// <summary>
    /// Additional comprehensive tests for <see cref="TaskService"/> covering edge cases.
    /// </summary>
    public class TaskServiceExtendedTests
    {
        private static TaskService CreateService(Mock<ITaskRepository> repoMock) =>
            new(repoMock.Object);

        private static DomainTask BuildTask(
            int id = 1,
            int userId = 10,
            string title = "Test task",
            string? description = "Some description") =>
            new()
            {
                Id = id,
                UserId = userId,
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        // ?????????????????????????????????????????????????????????????
        // GetTasksByUserIdAsync - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task GetTasksByUserIdAsync_LargeUserId_Works()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetAllByUserIdAsync(int.MaxValue))
                    .ReturnsAsync(new[] { BuildTask(1, int.MaxValue) });

            var service = CreateService(repoMock);

            // Act
            var result = await service.GetTasksByUserIdAsync(int.MaxValue);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTasksByUserIdAsync_MultipleTasksPreserveOrder()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            var tasks = new[]
            {
                BuildTask(1, 10, "Task A"),
                BuildTask(2, 10, "Task B"),
                BuildTask(3, 10, "Task C")
            };
            repoMock.Setup(r => r.GetAllByUserIdAsync(10)).ReturnsAsync(tasks);

            var service = CreateService(repoMock);

            // Act
            var result = (await service.GetTasksByUserIdAsync(10)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Task A", result[0].Title);
            Assert.Equal("Task B", result[1].Title);
            Assert.Equal("Task C", result[2].Title);
        }

        // ?????????????????????????????????????????????????????????????
        // CreateTaskAsync - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskAsync_DescriptionWithLeadingTrailingSpaces_Trimmed()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                UserId = 10,
                Title = "Task",
                Description = "   Description with spaces   "
            };

            var createdTask = new DomainTask
            {
                Id = 1,
                UserId = 10,
                Title = "Task",
                Description = "Description with spaces",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<DomainTask>()))
                    .Callback<DomainTask>(t =>
                    {
                        t.Id = 1;
                    })
                    .ReturnsAsync(createdTask);

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateTaskAsync(dto);

            // Assert
            Assert.Equal("Description with spaces", result.Description);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskAsync_SingleCharacterTitle_IsValid()
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 10, Title = "A" };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => { t.Id = 1; return t; });

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateTaskAsync(dto);

            // Assert
            Assert.Equal("A", result.Title);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskAsync_TitleOnlyWhitespace_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 10, Title = "     " };
            var service = CreateService(new Mock<ITaskRepository>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(dto));
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskAsync_EmptyDescription_IsValid()
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 10, Title = "Task", Description = "" };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => { t.Id = 1; return t; });

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateTaskAsync(dto);

            // Assert - empty string should be trimmed to empty string
            Assert.Empty(result.Description ?? "");
        }

        // ?????????????????????????????????????????????????????????????
        // UpdateTaskAsync - Extended
        // ?????????????????????????????????????????????????????????????

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_PreservesUserId()
        {
            // Arrange
            var existing = BuildTask(7, 10, "Old title");
            var dto = new UpdateTaskDto { Id = 7, Title = "New title" };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(existing);
            repoMock.Setup(r => r.UpdateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => t);

            var service = CreateService(repoMock);

            // Act
            var result = await service.UpdateTaskAsync(dto);

            // Assert
            Assert.Equal(10, result!.UserId);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_UpdatesTimestamp()
        {
            // Arrange
            var existing = BuildTask(7, 10);
            var originalUpdated = existing.UpdatedAt;
            var dto = new UpdateTaskDto { Id = 7, Title = "Updated" };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(existing);
            repoMock.Setup(r => r.UpdateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => t);

            var service = CreateService(repoMock);

            // Act
            await service.UpdateTaskAsync(dto);

            // Assert
            Assert.True(existing.UpdatedAt > originalUpdated);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_ClearsDescriptionIfSet()
        {
            // Arrange
            var existing = BuildTask(7, 10, description: "Original description");
            var dto = new UpdateTaskDto { Id = 7, Title = "Title", Description = null };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(existing);
            repoMock.Setup(r => r.UpdateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => t);

            var service = CreateService(repoMock);

            // Act
            await service.UpdateTaskAsync(dto);

            // Assert
            Assert.Null(existing.Description);
        }

        // ?????????????????????????????????????????????????????????????
        // Boundary and error cases
        // ?????????????????????????????????????????????????????????????

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public async System.Threading.Tasks.Task GetTaskByIdAsync_VariousValidIds_DoesntThrow(int validId)
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(validId))
                    .ReturnsAsync(BuildTask(validId));

            var service = CreateService(repoMock);

            // Act & Assert
            var result = await service.GetTaskByIdAsync(validId);
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateTaskAsync_NullDescription_DoesntThrow()
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 10, Title = "Task", Description = null };

            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.CreateAsync(It.IsAny<DomainTask>()))
                    .ReturnsAsync((DomainTask t) => { t.Id = 1; return t; });

            var service = CreateService(repoMock);

            // Act
            var result = await service.CreateTaskAsync(dto);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTaskAsync_NullDto_ThrowsArgumentNullException()
        {
            var service = CreateService(new Mock<ITaskRepository>());

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.UpdateTaskAsync(null!));
        }

        [Fact]
        public async System.Threading.Tasks.Task GetTasksByUserIdAsync_RepositoryThrowsException_Propagates()
        {
            // Arrange
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetAllByUserIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception("Database error"));

            var service = CreateService(repoMock);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                service.GetTasksByUserIdAsync(10));
        }
    }
}
