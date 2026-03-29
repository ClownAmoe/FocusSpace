using FocusSpace.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FocusSpace.Tests.DTOs
{
    /// <summary>
    /// Unit tests for <see cref="RegisterDto"/> and <see cref="LoginDto"/>.
    /// </summary>
    public class AuthDtosTests
    {
        // ═════════════════════════════════════════════════════════════
        // RegisterDto
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void RegisterDto_ValidData_PassesValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid, string.Join(", ", results.Select(r => r.ErrorMessage)));
        }

        [Fact]
        public void RegisterDto_UsernameTooShort_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "ab",
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void RegisterDto_InvalidEmail_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "not-an-email",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void RegisterDto_PasswordTooShort_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "short",
                ConfirmPassword = "short"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void RegisterDto_PasswordsDoNotMatch_FailsValidation()
        {
            // Arrange
            var dto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "DifferentPass123!"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void RegisterDto_MissingUsername_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new RegisterDto
            {
                Username = null,
                Email = "test@example.com",
                Password = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        // ═════════════════════════════════════════════════════════════
        // LoginDto
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void LoginDto_ValidData_PassesValidation()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = "SecurePass123!",
                RememberMe = true
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void LoginDto_MissingEmail_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new LoginDto
            {
                Email = null,
                Password = "SecurePass123!"
            };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void LoginDto_MissingPassword_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new LoginDto
            {
                Email = "test@example.com",
                Password = null
            };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void LoginDto_InvalidEmail_FailsValidation()
        {
            // Arrange
            var dto = new LoginDto
            {
                Email = "invalid-email",
                Password = "SecurePass123!"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void LoginDto_RememberMeDefault_IsFalse()
        {
            // Arrange & Act
            var dto = new LoginDto();

            // Assert
            Assert.False(dto.RememberMe);
        }

        [Fact]
        public void LoginDto_RememberMeCanBeSet_ToTrue()
        {
            // Arrange & Act
            var dto = new LoginDto { RememberMe = true };

            // Assert
            Assert.True(dto.RememberMe);
        }

        // ═════════════════════════════════════════════════════════════
        // ForgotPasswordDto
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void ForgotPasswordDto_ValidEmail_PassesValidation()
        {
            // Arrange
            var dto = new ForgotPasswordDto { Email = "test@example.com" };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid, string.Join(", ", results.Select(r => r.ErrorMessage)));
        }

        [Fact]
        public void ForgotPasswordDto_InvalidEmail_FailsValidation()
        {
            // Arrange
            var dto = new ForgotPasswordDto { Email = "not-an-email" };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ForgotPasswordDto_MissingEmail_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new ForgotPasswordDto { Email = null };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ForgotPasswordDto_EmptyEmail_FailsValidation()
        {
            // Arrange
            var dto = new ForgotPasswordDto { Email = string.Empty };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        // ═════════════════════════════════════════════════════════════
        // ResetPasswordDto
        // ═════════════════════════════════════════════════════════════

        [Fact]
        public void ResetPasswordDto_ValidData_PassesValidation()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                UserId = "123",
                Token = "reset-token",
                NewPassword = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid, string.Join(", ", results.Select(r => r.ErrorMessage)));
        }

        [Fact]
        public void ResetPasswordDto_MissingUserId_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new ResetPasswordDto
            {
                UserId = null,
                Token = "reset-token",
                NewPassword = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ResetPasswordDto_MissingToken_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new ResetPasswordDto
            {
                UserId = "123",
                Token = null,
                NewPassword = "SecurePass123!",
                ConfirmPassword = "SecurePass123!"
            };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ResetPasswordDto_PasswordTooShort_FailsValidation()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                UserId = "123",
                Token = "reset-token",
                NewPassword = "short",
                ConfirmPassword = "short"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ResetPasswordDto_PasswordsDoNotMatch_FailsValidation()
        {
            // Arrange
            var dto = new ResetPasswordDto
            {
                UserId = "123",
                Token = "reset-token",
                NewPassword = "SecurePass123!",
                ConfirmPassword = "DifferentPass123!"
            };

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ResetPasswordDto_MissingNewPassword_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new ResetPasswordDto
            {
                UserId = "123",
                Token = "reset-token",
                NewPassword = null,
                ConfirmPassword = "SecurePass123!"
            };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }

        [Fact]
        public void ResetPasswordDto_MissingConfirmPassword_FailsValidation()
        {
            // Arrange
#pragma warning disable CS8625
            var dto = new ResetPasswordDto
            {
                UserId = "123",
                Token = "reset-token",
                NewPassword = "SecurePass123!",
                ConfirmPassword = null
            };
#pragma warning restore CS8625

            // Act
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.NotEmpty(results);
        }
    }
}
