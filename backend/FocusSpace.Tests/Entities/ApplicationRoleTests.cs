using FocusSpace.Domain.Entities;
using Xunit;

namespace FocusSpace.Tests.Entities
{
    /// <summary>
    /// Unit tests for <see cref="ApplicationRole"/>.
    /// </summary>
    public class ApplicationRoleTests
    {
        [Fact]
        public void ApplicationRole_DefaultConstructor_CreatesEmptyRole()
        {
            // Act
            var role = new ApplicationRole();

            // Assert
            Assert.NotNull(role);
            Assert.Null(role.Name);
            Assert.Null(role.NormalizedName);
        }

        [Fact]
        public void ApplicationRole_ConstructorWithName_SetsRoleName()
        {
            // Arrange
            const string roleName = "Admin";

            // Act
            var role = new ApplicationRole(roleName);

            // Assert
            Assert.NotNull(role);
            Assert.Equal(roleName, role.Name);
        }

        [Fact]
        public void ApplicationRole_InheritsFromIdentityRole_HasKeyProperties()
        {
            // Arrange & Act
            var role = new ApplicationRole { Id = 1, Name = "User" };

            // Assert
            Assert.Equal(1, role.Id);
            Assert.Equal("User", role.Name);
        }

        [Fact]
        public void ApplicationRole_CanSetProperties_AllPropertiesAssignable()
        {
            // Arrange
            var role = new ApplicationRole();
            var stamp = Guid.NewGuid().ToString();

            // Act
            role.Id = 2;
            role.Name = "Editor";
            role.NormalizedName = "EDITOR";
            role.ConcurrencyStamp = stamp;

            // Assert
            Assert.Equal(2, role.Id);
            Assert.Equal("Editor", role.Name);
            Assert.Equal("EDITOR", role.NormalizedName);
            Assert.Equal(stamp, role.ConcurrencyStamp);
        }
    }
}
