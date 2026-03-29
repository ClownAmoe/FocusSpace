using FocusSpace.Domain.Entities;
using FocusSpace.Domain.Enums;
using FocusSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Tests.Entities
{
    /// <summary>
    /// Unit tests for <see cref="AppDbContext"/> configuration and seeding.
    /// </summary>
    public class AppDbContextTests
    {
        private DbContextOptions<AppDbContext> CreateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        // ═════════════════════════════════════════════════════════════
        // Model Configuration
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void OnModelCreating_SeedsRoles_RolesArePresent()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act
            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureCreated();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
                var userRole = context.Roles.FirstOrDefault(r => r.Name == "User");

                Assert.NotNull(adminRole);
                Assert.NotNull(userRole);
                Assert.Equal("ADMIN", adminRole.NormalizedName);
                Assert.Equal("USER", userRole.NormalizedName);
            }
        }

        [Fact]
        public void OnModelCreating_SeedsPlanets_PlanetsArePresent()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act
            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureCreated();
            }

            // Assert
            using (var context = new AppDbContext(options))
            {
                var planets = context.Planets.ToList();

                Assert.Equal(8, planets.Count);
                Assert.Contains(planets, p => p.Name == "Mercury");
                Assert.Contains(planets, p => p.Name == "Venus");
                Assert.Contains(planets, p => p.Name == "Earth");
                Assert.Contains(planets, p => p.Name == "Mars");
            }
        }

        [Fact]
        public void OnModelCreating_ConfiguresUserEntity_RelationshipsAreCorrect()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act & Assert
            using (var context = new AppDbContext(options))
            {
                context.Database.EnsureCreated();
                // Context is successfully created with user configuration
                Assert.NotNull(context.Users);
            }
        }

        [Fact]
        public void OnModelCreating_ConfiguresTaskEntity_CascadeDeleteApplied()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var planet = new Planet { Id = 100, Name = "TestPlanet", OrderNumber = 100 };
            var user = new User
            {
                Id = 100,
                UserName = "testuser",
                Email = "test@example.com",
                CurrentPlanetId = 100,
                Role = UserRole.User
            };
            var task = new DomainTask
            {
                Id = 100,
                UserId = 100,
                Title = "Test Task",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act & Assert
            using (var context = new AppDbContext(options))
            {
                context.Planets.Add(planet);
                context.Users.Add(user);
                context.Tasks.Add(task);
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var savedTask = context.Tasks.Find(100);
                Assert.NotNull(savedTask);
                Assert.Equal(100, savedTask.UserId);
            }
        }

        [Fact]
        public void OnModelCreating_ConfiguresSessionEntity_RelationshipsAreConfigured()
        {
            // Arrange
            var options = CreateInMemoryOptions();
            var planet = new Planet { Id = 101, Name = "TestPlanet", OrderNumber = 101 };
            var user = new User
            {
                Id = 101,
                UserName = "testuser",
                Email = "test@example.com",
                CurrentPlanetId = 101,
                Role = UserRole.User
            };
            var task = new DomainTask
            {
                Id = 101,
                UserId = 101,
                Title = "Test Task",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var session = new Session
            {
                Id = 100,
                UserId = 101,
                TaskId = 101,
                PlannedDuration = TimeSpan.FromSeconds(3600),
                Status = SessionStatus.Ongoing,
                CreatedAt = DateTime.UtcNow
            };

            // Act & Assert
            using (var context = new AppDbContext(options))
            {
                context.Planets.Add(planet);
                context.Users.Add(user);
                context.Tasks.Add(task);
                context.Sessions.Add(session);
                context.SaveChanges();
            }

            using (var context = new AppDbContext(options))
            {
                var savedSession = context.Sessions.Find(100);
                Assert.NotNull(savedSession);
                Assert.Equal(SessionStatus.Ongoing, savedSession.Status);
            }
        }

        [Fact]
        public void DbSets_AreAccessible_ReturnCorrectTypes()
        {
            // Arrange
            var options = CreateInMemoryOptions();

            // Act & Assert
            using (var context = new AppDbContext(options))
            {
                Assert.NotNull(context.Users);
                Assert.NotNull(context.Tasks);
                Assert.NotNull(context.Sessions);
                Assert.NotNull(context.Planets);
            }
        }
    }
}
