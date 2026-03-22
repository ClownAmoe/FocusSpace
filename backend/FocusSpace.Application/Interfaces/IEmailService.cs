namespace FocusSpace.Application.Interfaces
{
    /// <summary>Contract for sending transactional emails.</summary>
    public interface IEmailService
    {
        /// <summary>Sends an email asynchronously.</summary>
        Task SendAsync(string to, string subject, string htmlBody);

        /// <summary>Helper — sends an account-confirmation link.</summary>
        Task SendConfirmationEmailAsync(string to, string username, string confirmationLink);

        /// <summary>Helper — sends a password-reset link.</summary>
        Task SendPasswordResetEmailAsync(string to, string username, string resetLink);
    }
}