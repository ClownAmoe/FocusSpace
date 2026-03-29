using FocusSpace.Domain.Entities;
using Xunit;

namespace FocusSpace.Tests.Entities
{
    /// <summary>
    /// Unit tests for <see cref="Planet"/>.
    /// </summary>
    public class PlanetTests
    {
        [Fact]
        public void Planet_DefaultConstructor_InitializesProperties()
        {
            // Act
            var planet = new Planet();

            // Assert
            Assert.NotNull(planet);
            Assert.Equal(string.Empty, planet.Name);
            Assert.Equal(0, planet.OrderNumber);
            Assert.Null(planet.DistanceFromPrevious);
            Assert.Null(planet.ImageUrl);
            Assert.NotNull(planet.Users);
            Assert.Empty(planet.Users);
        }

        [Fact]
        public void Planet_CanSetProperties_AllPropertiesAssignable()
        {
            // Arrange
            var planet = new Planet();

            // Act
            planet.Id = 1;
            planet.Name = "Earth";
            planet.OrderNumber = 3;
            planet.Description = "Our home planet";
            planet.ImageUrl = "https://example.com/earth.jpg";
            planet.DistanceFromPrevious = TimeSpan.FromDays(50);

            // Assert
            Assert.Equal(1, planet.Id);
            Assert.Equal("Earth", planet.Name);
            Assert.Equal(3, planet.OrderNumber);
            Assert.Equal("Our home planet", planet.Description);
            Assert.Equal("https://example.com/earth.jpg", planet.ImageUrl);
            Assert.NotNull(planet.DistanceFromPrevious);
            Assert.Equal(TimeSpan.FromDays(50), planet.DistanceFromPrevious);
        }

        [Fact]
        public void Planet_CanAddUsers_UsersCollectionUpdates()
        {
            // Arrange
            var planet = new Planet { Id = 1, Name = "Mars" };
            var user = new User
            {
                Id = 1,
                UserName = "explorer",
                Email = "explorer@example.com",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            // Act
            planet.Users.Add(user);

            // Assert
            Assert.Single(planet.Users);
            Assert.Contains(user, planet.Users);
        }

        [Fact]
        public void Planet_UsersCollectionIsNotNull_ByDefault()
        {
            // Act
            var planet = new Planet();

            // Assert
            Assert.NotNull(planet.Users);
            Assert.IsAssignableFrom<ICollection<User>>(planet.Users);
        }

        [Fact]
        public void Planet_WithAllProperties_CreatesCompleteEntity()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var planet = new Planet
            {
                Id = 4,
                Name = "Mars",
                OrderNumber = 4,
                Description = "The Red Planet",
                ImageUrl = "https://example.com/mars.jpg",
                DistanceFromPrevious = TimeSpan.FromDays(70)
            };

            // Assert
            Assert.Equal(4, planet.Id);
            Assert.Equal("Mars", planet.Name);
            Assert.Equal(4, planet.OrderNumber);
            Assert.Equal("The Red Planet", planet.Description);
            Assert.Equal("https://example.com/mars.jpg", planet.ImageUrl);
            Assert.Equal(TimeSpan.FromDays(70), planet.DistanceFromPrevious);
        }

        [Fact]
        public void Planet_NameCanBeEmpty_IsValid()
        {
            // Arrange & Act
            var planet = new Planet { Id = 1, Name = string.Empty, OrderNumber = 1 };

            // Assert
            Assert.Equal(string.Empty, planet.Name);
        }

        [Fact]
        public void Planet_DescriptionCanBeNull_IsValid()
        {
            // Arrange & Act
            var planet = new Planet
            {
                Id = 1,
                Name = "Unknown",
                OrderNumber = 1,
                Description = null
            };

            // Assert
            Assert.Null(planet.Description);
        }

        [Fact]
        public void Planet_MultipleUsers_CollectionHandlesThem()
        {
            // Arrange
            var planet = new Planet { Id = 1, Name = "Venus" };
            var users = new List<User>
            {
                new() { Id = 1, UserName = "user1", Email = "user1@example.com", SecurityStamp = Guid.NewGuid().ToString() },
                new() { Id = 2, UserName = "user2", Email = "user2@example.com", SecurityStamp = Guid.NewGuid().ToString() },
                new() { Id = 3, UserName = "user3", Email = "user3@example.com", SecurityStamp = Guid.NewGuid().ToString() }
            };

            // Act
            foreach (var user in users)
            {
                planet.Users.Add(user);
            }

            // Assert
            Assert.Equal(3, planet.Users.Count);
        }
    }
}
