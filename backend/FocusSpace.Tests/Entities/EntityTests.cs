using FocusSpace.Domain.Entities;
using FocusSpace.Domain.Enums;
using Xunit;
using DomainTask = FocusSpace.Domain.Entities.Task;
using TaskStatus = FocusSpace.Domain.Enums.TaskStatus;

namespace FocusSpace.Tests.Entities
{
    /// <summary>
    /// Unit tests for <see cref="User"/> entity.
    /// </summary>
    public class UserEntityTests
    {
        [Fact]
        public void User_DefaultValues_AreCorrect()
        {
            // Act
            var user = new User();

            // Assert
            Assert.Equal(UserRole.User, user.Role);
            Assert.False(user.IsBlocked);
            Assert.False(user.IsApproved);
            Assert.Equal(1, user.CurrentPlanetId);
            Assert.Equal(0, user.TotalFocusMinutes);
        }

        [Fact]
        public void User_CanSetProperties()
        {
            // Arrange
            var user = new User();

            // Act
            user.UserName = "testuser";
            user.Email = "test@example.com";
            user.Role = UserRole.Admin;
            user.IsBlocked = true;
            user.IsApproved = true;
            user.CurrentPlanetId = 2;
            user.TotalFocusMinutes = 120;

            // Assert
            Assert.Equal("testuser", user.UserName);
            Assert.Equal("test@example.com", user.Email);
            Assert.Equal(UserRole.Admin, user.Role);
            Assert.True(user.IsBlocked);
            Assert.True(user.IsApproved);
            Assert.Equal(2, user.CurrentPlanetId);
            Assert.Equal(120, user.TotalFocusMinutes);
        }

        [Fact]
        public void User_HasNavigationProperties()
        {
            // Arrange & Act
            var user = new User { Id = 1, UserName = "testuser" };

            // Assert
            Assert.NotNull(user.Sessions);
            Assert.NotNull(user.Tasks);
            Assert.Empty(user.Sessions);
            Assert.Empty(user.Tasks);
        }

        [Fact]
        public void User_CanAddSessions()
        {
            // Arrange
            var user = new User { Id = 1 };
            var session = new Session { Id = 1, UserId = 1, StartTime = DateTime.UtcNow };

            // Act
            user.Sessions.Add(session);

            // Assert
            Assert.Single(user.Sessions);
            Assert.Contains(session, user.Sessions);
        }

        [Fact]
        public void User_CanAddTasks()
        {
            // Arrange
            var user = new User { Id = 1 };
            var task = new DomainTask { Id = 1, UserId = 1, Title = "Test" };

            // Act
            user.Tasks.Add(task);

            // Assert
            Assert.Single(user.Tasks);
            Assert.Contains(task, user.Tasks);
        }
    }

    /// <summary>
    /// Unit tests for <see cref="Task"/> entity.
    /// </summary>
    public class TaskEntityTests
    {
        [Fact]
        public void Task_DefaultValues_AreCorrect()
        {
            // Act
            var task = new DomainTask();

            // Assert
            Assert.Empty(task.Title);
            Assert.Null(task.Description);
            Assert.NotNull(task.Sessions);
            Assert.Empty(task.Sessions);
        }

        [Fact]
        public void Task_CanSetProperties()
        {
            // Arrange
            var task = new DomainTask();

            // Act
            task.Id = 5;
            task.UserId = 10;
            task.Title = "My Task";
            task.Description = "Task description";

            // Assert
            Assert.Equal(5, task.Id);
            Assert.Equal(10, task.UserId);
            Assert.Equal("My Task", task.Title);
            Assert.Equal("Task description", task.Description);
        }

        [Fact]
        public void Task_TimestampsInitialized()
        {
            // Act
            var before = DateTime.UtcNow;
            var task = new DomainTask();
            var after = DateTime.UtcNow;

            // Assert
            Assert.True(task.CreatedAt >= before && task.CreatedAt <= after);
            Assert.True(task.UpdatedAt >= before && task.UpdatedAt <= after);
        }

        [Fact]
        public void Task_CanAddSessions()
        {
            // Arrange
            var task = new DomainTask { Id = 1, Title = "Test" };
            var session = new Session { Id = 1, TaskId = 1, StartTime = DateTime.UtcNow };

            // Act
            task.Sessions.Add(session);

            // Assert
            Assert.Single(task.Sessions);
            Assert.Contains(session, task.Sessions);
        }
    }

    /// <summary>
    /// Unit tests for <see cref="Session"/> entity.
    /// </summary>
    public class SessionEntityTests
    {
        [Fact]
        public void Session_DefaultStatus_IsOngoing()
        {
            // Act
            var session = new Session();

            // Assert
            Assert.Equal(SessionStatus.Ongoing, session.Status);
        }

        [Fact]
        public void Session_CanSetAllProperties()
        {
            // Arrange
            var session = new Session();
            var now = DateTime.UtcNow;

            // Act
            session.Id = 1;
            session.UserId = 5;
            session.TaskId = 10;
            session.StartTime = now;
            session.EndTime = now.AddHours(1);
            session.PlannedDuration = TimeSpan.FromMinutes(60);
            session.ActualDuration = TimeSpan.FromMinutes(55);
            session.Status = SessionStatus.Completed;

            // Assert
            Assert.Equal(1, session.Id);
            Assert.Equal(5, session.UserId);
            Assert.Equal(10, session.TaskId);
            Assert.Equal(now, session.StartTime);
            Assert.Equal(now.AddHours(1), session.EndTime);
            Assert.Equal(TimeSpan.FromMinutes(60), session.PlannedDuration);
            Assert.Equal(TimeSpan.FromMinutes(55), session.ActualDuration);
            Assert.Equal(SessionStatus.Completed, session.Status);
        }

        [Fact]
        public void Session_TaskIdCanBeNull()
        {
            // Arrange & Act
            var session = new Session { UserId = 1, TaskId = null };

            // Assert
            Assert.Null(session.TaskId);
        }

        [Fact]
        public void Session_EndTimeCanBeNull()
        {
            // Arrange & Act
            var session = new Session { UserId = 1, EndTime = null };

            // Assert
            Assert.Null(session.EndTime);
        }

        [Fact]
        public void Session_ActualDurationCanBeNull()
        {
            // Arrange & Act
            var session = new Session { UserId = 1, ActualDuration = null };

            // Assert
            Assert.Null(session.ActualDuration);
        }
    }

    /// <summary>
    /// Unit tests for <see cref="Planet"/> entity.
    /// </summary>
    public class PlanetEntityTests
    {
        [Fact]
        public void Planet_CanBeCreated()
        {
            // Act
            var planet = new Planet { Id = 1, Name = "Earth" };

            // Assert
            Assert.Equal(1, planet.Id);
            Assert.Equal("Earth", planet.Name);
        }

        [Fact]
        public void Planet_HasNavigationProperty()
        {
            // Arrange & Act
            var planet = new Planet { Id = 1 };

            // Assert
            Assert.NotNull(planet.Users);
            Assert.Empty(planet.Users);
        }

        [Fact]
        public void Planet_CanHaveMultipleUsers()
        {
            // Arrange
            var planet = new Planet { Id = 1, Name = "Mars" };
            var user1 = new User { Id = 1, UserName = "user1" };
            var user2 = new User { Id = 2, UserName = "user2" };

            // Act
            planet.Users.Add(user1);
            planet.Users.Add(user2);

            // Assert
            Assert.Equal(2, planet.Users.Count);
        }
    }

    /// <summary>
    /// Unit tests for enums.
    /// </summary>
    public class EnumTests
    {
        [Fact]
        public void UserRole_HasCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)UserRole.User);
            Assert.Equal(1, (int)UserRole.Admin);
        }

        [Fact]
        public void SessionStatus_HasCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)SessionStatus.Ongoing);
            Assert.Equal(1, (int)SessionStatus.Paused);
            Assert.Equal(2, (int)SessionStatus.Completed);
            Assert.Equal(3, (int)SessionStatus.Aborted);
        }

        [Fact]
        public void TaskStatus_HasCorrectValues()
        {
            // Assert
            Assert.Equal(0, (int)TaskStatus.Todo);
            Assert.Equal(1, (int)TaskStatus.Done);
        }
    }
}
