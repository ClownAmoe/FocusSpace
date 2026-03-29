using FocusSpace.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FocusSpace.Tests.DTOs
{
    /// <summary>
    /// Unit tests for Task DTOs.
    /// </summary>
    public class TaskDtoTests
    {
        [Fact]
        public void TaskDto_CanBeCreated()
        {
            // Act
            var dto = new TaskDto
            {
                Id = 1,
                UserId = 5,
                Title = "Test Task",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal(5, dto.UserId);
            Assert.Equal("Test Task", dto.Title);
            Assert.Equal("Test Description", dto.Description);
        }

        [Fact]
        public void TaskDto_DescriptionCanBeNull()
        {
            // Act
            var dto = new TaskDto { Id = 1, Title = "Task", Description = null };

            // Assert
            Assert.Null(dto.Description);
        }

        [Fact]
        public void CreateTaskDto_ValidValues_PassesValidation()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                UserId = 5,
                Title = "New Task",
                Description = "New Description"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void CreateTaskDto_EmptyTitle_FailsValidation()
        {
            // Arrange
            var dto = new CreateTaskDto { UserId = 5, Title = "" };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        }

        [Fact]
        public void CreateTaskDto_TitleExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                UserId = 5,
                Title = new string('a', 301)
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void CreateTaskDto_DescriptionExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                UserId = 5,
                Title = "Valid",
                Description = new string('a', 2001)
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void CreateTaskDto_DescriptionCanBeNull()
        {
            // Arrange
            var dto = new CreateTaskDto
            {
                UserId = 5,
                Title = "Valid Task",
                Description = null
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void UpdateTaskDto_ValidValues_PassesValidation()
        {
            // Arrange
            var dto = new UpdateTaskDto
            {
                Id = 5,
                Title = "Updated Task",
                Description = "Updated Description"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void UpdateTaskDto_EmptyTitle_FailsValidation()
        {
            // Arrange
            var dto = new UpdateTaskDto { Id = 5, Title = "" };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void UpdateTaskDto_DefaultTitle_IsEmpty()
        {
            // Act
            var dto = new UpdateTaskDto();

            // Assert
            Assert.Empty(dto.Title);
        }
    }

    /// <summary>
    /// Unit tests for Session DTOs.
    /// </summary>
    public class SessionDtoTests
    {
        [Fact]
        public void SessionDto_CanBeCreated()
        {
            // Act
            var dto = new SessionDto
            {
                Id = 1,
                UserId = 5,
                TaskId = 10,
                TaskTitle = "Task Title",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                PlannedDuration = TimeSpan.FromMinutes(60),
                ActualDuration = TimeSpan.FromMinutes(55),
                Status = "Completed"
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal(5, dto.UserId);
            Assert.Equal(10, dto.TaskId);
            Assert.Equal("Task Title", dto.TaskTitle);
            Assert.Equal("Completed", dto.Status);
        }

        [Fact]
        public void SessionDto_TaskIdCanBeNull()
        {
            // Act
            var dto = new SessionDto { TaskId = null };

            // Assert
            Assert.Null(dto.TaskId);
        }

        [Fact]
        public void SessionDto_EndTimeCanBeNull()
        {
            // Act
            var dto = new SessionDto { EndTime = null };

            // Assert
            Assert.Null(dto.EndTime);
        }

        [Fact]
        public void CreateSessionDto_ValidValues_CanBeCreated()
        {
            // Act
            var dto = new CreateSessionDto
            {
                UserId = 5,
                TaskId = 10,
                PlannedDuration = TimeSpan.FromMinutes(60)
            };

            // Assert
            Assert.Equal(5, dto.UserId);
            Assert.Equal(10, dto.TaskId);
            Assert.Equal(TimeSpan.FromMinutes(60), dto.PlannedDuration);
        }

        [Fact]
        public void CreateSessionDto_TaskIdCanBeNull()
        {
            // Act
            var dto = new CreateSessionDto { TaskId = null };

            // Assert
            Assert.Null(dto.TaskId);
        }

        [Fact]
        public void UpdateSessionDto_DefaultValues()
        {
            // Act
            var dto = new UpdateSessionDto();

            // Assert
            Assert.Empty(dto.Status);
            Assert.Null(dto.EndTime);
            Assert.Null(dto.ActualDuration);
        }

        [Fact]
        public void UpdateSessionDto_CanSetStatus()
        {
            // Act
            var dto = new UpdateSessionDto { Status = "Completed" };

            // Assert
            Assert.Equal("Completed", dto.Status);
        }

        [Fact]
        public void UpdateSessionDto_CanSetDuration()
        {
            // Act
            var dto = new UpdateSessionDto { ActualDuration = "01:30:00" };

            // Assert
            Assert.Equal("01:30:00", dto.ActualDuration);
        }
    }

    /// <summary>
    /// Unit tests for User DTOs.
    /// </summary>
    public class UserDtoTests
    {
        [Fact]
        public void UserDto_CanBeCreated()
        {
            // Act
            var dto = new UserDto
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                Role = "User"
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("testuser", dto.Username);
            Assert.Equal("test@example.com", dto.Email);
            Assert.Equal("User", dto.Role);
        }

        [Fact]
        public void UserDto_AllPropertiesSettable()
        {
            // Arrange
            var dto = new UserDto();

            // Act
            dto.Id = 5;
            dto.Username = "newuser";
            dto.Email = "new@example.com";
            dto.Role = "Admin";
            dto.IsBlocked = true;
            dto.CurrentPlanetId = 2;
            dto.CurrentPlanetName = "Mars";
            dto.TotalFocusMinutes = 300;
            dto.CreatedAt = DateTime.UtcNow;

            // Assert
            Assert.Equal(5, dto.Id);
            Assert.Equal("newuser", dto.Username);
            Assert.Equal("new@example.com", dto.Email);
            Assert.Equal("Admin", dto.Role);
            Assert.True(dto.IsBlocked);
            Assert.Equal(2, dto.CurrentPlanetId);
            Assert.Equal("Mars", dto.CurrentPlanetName);
            Assert.Equal(300, dto.TotalFocusMinutes);
        }

        [Fact]
        public void RegisterUserDto_CanBeCreated()
        {
            // Act
            var dto = new RegisterUserDto
            {
                Username = "newuser",
                Email = "new@example.com",
                Password = "Password123!"
            };

            // Assert
            Assert.Equal("newuser", dto.Username);
            Assert.Equal("new@example.com", dto.Email);
            Assert.Equal("Password123!", dto.Password);
        }

        [Fact]
        public void LoginUserDto_CanBeCreated()
        {
            // Act
            var dto = new LoginUserDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Assert
            Assert.Equal("test@example.com", dto.Email);
            Assert.Equal("Password123!", dto.Password);
        }

        [Fact]
        public void UpdateUserDto_CanBeCreated()
        {
            // Act
            var dto = new UpdateUserDto
            {
                Id = 1,
                Username = "updateduser",
                Email = "updated@example.com"
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("updateduser", dto.Username);
            Assert.Equal("updated@example.com", dto.Email);
        }
    }

    /// <summary>
    /// Unit tests for Planet DTOs.
    /// </summary>
    public class PlanetDtoTests
    {
        [Fact]
        public void PlanetDto_CanBeCreated()
        {
            // Act
            var dto = new PlanetDto
            {
                Id = 1,
                Name = "Earth",
                OrderNumber = 3,
                Description = "Our home planet"
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("Earth", dto.Name);
            Assert.Equal(3, dto.OrderNumber);
            Assert.Equal("Our home planet", dto.Description);
        }

        [Fact]
        public void PlanetDto_AllPropertiesSettable()
        {
            // Arrange
            var dto = new PlanetDto();

            // Act
            dto.Id = 2;
            dto.Name = "Mars";
            dto.OrderNumber = 4;
            dto.Description = "The red planet";
            dto.DistanceFromPrevious = TimeSpan.FromHours(2);
            dto.ImageUrl = "https://example.com/mars.jpg";

            // Assert
            Assert.Equal(2, dto.Id);
            Assert.Equal("Mars", dto.Name);
            Assert.Equal(4, dto.OrderNumber);
            Assert.Equal("The red planet", dto.Description);
            Assert.Equal(TimeSpan.FromHours(2), dto.DistanceFromPrevious);
            Assert.Equal("https://example.com/mars.jpg", dto.ImageUrl);
        }

        [Fact]
        public void PlanetDto_OptionalPropertiesCanBeNull()
        {
            // Act
            var dto = new PlanetDto
            {
                Id = 1,
                Name = "Venus",
                OrderNumber = 2,
                Description = null,
                DistanceFromPrevious = null,
                ImageUrl = null
            };

            // Assert
            Assert.Null(dto.Description);
            Assert.Null(dto.DistanceFromPrevious);
            Assert.Null(dto.ImageUrl);
        }

        [Fact]
        public void UpdatePlanetDto_CanBeCreated()
        {
            // Act
            var dto = new UpdatePlanetDto
            {
                Id = 1,
                Name = "Updated Planet",
                OrderNumber = 5
            };

            // Assert
            Assert.Equal(1, dto.Id);
            Assert.Equal("Updated Planet", dto.Name);
            Assert.Equal(5, dto.OrderNumber);
        }
    }
}
