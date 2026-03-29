using FocusSpace.Infrastructure.Data;
using FocusSpace.Infrastructure.Repositories;
using FocusSpace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DomainTask = FocusSpace.Domain.Entities.Task;
using User = FocusSpace.Domain.Entities.User;

namespace FocusSpace.Tests.Interfaces
{
    /// <summary>
    /// Unit tests for <see cref="TaskRepository"/>.
    /// </summary>
    public class TaskRepositoryTests
    {
        private DbContextOptions<AppDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private DomainTask BuildTask(int id = 1, int userId = 1, string title = "Test Task")
        {
            return new DomainTask
            {
                Id = id,
                UserId = userId,
                Title = title,
                Description = "Test description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private User BuildUser(int id = 1)
        {
            return new User
            {
                Id = id,
                UserName = $"user{id}",
                Email = $"user{id}@example.com",
                Role = UserRole.User,
                SecurityStamp = Guid.NewGuid().ToString()
            };
        }

        // ═════════════════════════════════════════════════════════════
        // GetAllByUserIdAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetAllByUserIdAsync_ValidUserId_ReturnsTasks()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser(1);

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Tasks.Add(BuildTask(1, 1, "Task A"));
                context.Tasks.Add(BuildTask(2, 1, "Task B"));
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var result = (await repository.GetAllByUserIdAsync(1)).ToList();

                // Assert
                Assert.NotEmpty(result);
                Assert.Equal(2, result.Count);
                Assert.True(result.All(t => t.UserId == 1));
            }
        }

        [Fact]
        public async Task GetAllByUserIdAsync_OrderedByCreatedAtDescending_ReturnsInCorrectOrder()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser(1);
            var task1 = BuildTask(1, 1, "Task A");
            task1.CreatedAt = DateTime.UtcNow.AddHours(-2);
            var task2 = BuildTask(2, 1, "Task B");
            task2.CreatedAt = DateTime.UtcNow;

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Tasks.Add(task1);
                context.Tasks.Add(task2);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var result = (await repository.GetAllByUserIdAsync(1)).ToList();

                // Assert
                Assert.Equal(2, result.Count);
                Assert.Equal(2, result[0].Id); // Most recent first
                Assert.Equal(1, result[1].Id);
            }
        }

        [Fact]
        public async Task GetAllByUserIdAsync_NoTasksForUser_ReturnsEmptyList()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser(1);

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var result = (await repository.GetAllByUserIdAsync(1)).ToList();

                // Assert
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetAllByUserIdAsync_FiltersByUserId_ExcludesOtherUserTasks()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user1 = BuildUser(1);
            var user2 = BuildUser(2);

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user1);
                context.Users.Add(user2);
                context.Tasks.Add(BuildTask(1, 1, "Task A"));
                context.Tasks.Add(BuildTask(2, 1, "Task B"));
                context.Tasks.Add(BuildTask(3, 2, "Task C"));
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var result = (await repository.GetAllByUserIdAsync(1)).ToList();

                // Assert
                Assert.Equal(2, result.Count);
                Assert.True(result.All(t => t.UserId == 1));
            }
        }

        // ═════════════════════════════════════════════════════════════
        // GetByIdAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetByIdAsync_ExistingTask_ReturnsTask()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Tasks.Add(BuildTask(1, 1));
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var result = await repository.GetByIdAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, result.Id);
            }
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentTask_ReturnsNull()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureCreated();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var result = await repository.GetByIdAsync(999);

                // Assert
                Assert.Null(result);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // CreateAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task CreateAsync_ValidTask_CreatesAndReturnsTask()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();
            var task = BuildTask();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            DomainTask createdTask;
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                createdTask = await repository.CreateAsync(task);
            }

            // Assert
            Assert.NotNull(createdTask);
            Assert.Equal(task.Title, createdTask.Title);

            using (var context = new AppDbContext(options))
            {
                var savedTask = await context.Tasks.FindAsync(task.Id);
                Assert.NotNull(savedTask);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // UpdateAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task UpdateAsync_ExistingTask_UpdatesAndReturnsTask()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();
            var originalTask = BuildTask();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Tasks.Add(originalTask);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var task = await repository.GetByIdAsync(1);
                task!.Title = "Updated Title";
                var result = await repository.UpdateAsync(task);

                // Assert
                Assert.Equal("Updated Title", result.Title);
            }

            // Verify persistence
            using (var context = new AppDbContext(options))
            {
                var updated = await context.Tasks.FindAsync(1);
                Assert.Equal("Updated Title", updated!.Title);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // DeleteAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task DeleteAsync_ExistingTask_DeletesTask()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();
            var task = BuildTask();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                await repository.DeleteAsync(1);
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var deleted = await context.Tasks.FindAsync(1);
                Assert.Null(deleted);
            }
        }

        [Fact]
        public async Task DeleteAsync_NonExistentTask_DoesNotThrow()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act & Assert - should not throw
            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureCreated();
                var repository = new TaskRepository(context);
                await repository.DeleteAsync(999);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // ExistsAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task ExistsAsync_ExistingTask_ReturnsTrue()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();
            var task = BuildTask();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Tasks.Add(task);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var exists = await repository.ExistsAsync(1);

                // Assert
                Assert.True(exists);
            }
        }

        [Fact]
        public async Task ExistsAsync_NonExistentTask_ReturnsFalse()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureCreated();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new TaskRepository(context);
                var exists = await repository.ExistsAsync(999);

                // Assert
                Assert.False(exists);
            }
        }
    }
}
