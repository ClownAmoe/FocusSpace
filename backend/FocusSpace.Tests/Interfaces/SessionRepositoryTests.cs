using FocusSpace.Infrastructure.Data;
using FocusSpace.Infrastructure.Repositories;
using FocusSpace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DomainSession = FocusSpace.Domain.Entities.Session;
using User = FocusSpace.Domain.Entities.User;

namespace FocusSpace.Tests.Interfaces
{
    /// <summary>
    /// Unit tests for <see cref="SessionRepository"/>.
    /// </summary>
    public class SessionRepositoryTests
    {
        private DbContextOptions<AppDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private FocusSpace.Domain.Entities.Session BuildSession(int id = 1, int userId = 1, int? taskId = null)
        {
            return new FocusSpace.Domain.Entities.Session
            {
                Id = id,
                UserId = userId,
                TaskId = taskId,
                PlannedDuration = TimeSpan.FromSeconds(3600),
                Status = SessionStatus.Ongoing,
                CreatedAt = DateTime.UtcNow
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
        // AddAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task AddAsync_ValidSession_AddsSessionToDatabase()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();
            var session = BuildSession(1, 1);

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new SessionRepository(context);
                await repository.AddAsync(session);
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var savedSession = await context.Sessions.FirstOrDefaultAsync(s => s.Id == 1);
                Assert.NotNull(savedSession);
                Assert.Equal(1, savedSession.UserId);
            }
        }

        [Fact]
        public async Task AddAsync_MultipleSessions_AddsAllSessions()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new SessionRepository(context);
                await repository.AddAsync(BuildSession(1, 1));
                await repository.AddAsync(BuildSession(2, 1));
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var sessions = await context.Sessions.ToListAsync();
                Assert.Equal(2, sessions.Count);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // GetByIdAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetByIdAsync_ExistingSession_ReturnsSession()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();
            var session = BuildSession();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Sessions.Add(session);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new SessionRepository(context);
                var result = await repository.GetByIdAsync(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, result.Id);
                Assert.Equal(1, result.UserId);
            }
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentSession_ReturnsNull()
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
                var repository = new SessionRepository(context);
                var result = await repository.GetByIdAsync(999);

                // Assert
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ZeroId_ReturnsNull()
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
                var repository = new SessionRepository(context);
                var result = await repository.GetByIdAsync(0);

                // Assert
                Assert.Null(result);
            }
        }

        // ═════════════════════════════════════════════════════════════
        // SaveChangesAsync
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public async Task SaveChangesAsync_WithModifiedSession_PersistsChanges()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var user = BuildUser();
            var session = BuildSession();

            using (var context = new AppDbContext(options))
            {
                context.Users.Add(user);
                context.Sessions.Add(session);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var repository = new SessionRepository(context);
                var existingSession = await repository.GetByIdAsync(1);
                existingSession!.Status = SessionStatus.Completed;
                await repository.SaveChangesAsync();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var updatedSession = await context.Sessions.FindAsync(1);
                Assert.Equal(SessionStatus.Completed, updatedSession!.Status);
            }
        }

        [Fact]
        public async Task SaveChangesAsync_NoChanges_Succeeds()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act & Assert - should not throw
            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureCreated();
                var repository = new SessionRepository(context);
                await repository.SaveChangesAsync();
            }
        }
    }
}
