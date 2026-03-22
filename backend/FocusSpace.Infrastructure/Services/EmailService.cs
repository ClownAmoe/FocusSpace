using FocusSpace.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace FocusSpace.Infrastructure.Services
{
    /// <summary>
    /// Sends emails via SMTP.
    /// Configure via appsettings.json under the "Email" section.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────
        // Core send
        // ──────────────────────────────────────────────────────────────

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var section = _config.GetSection("Email");

            var host = section["SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");
            var portStr = section["SmtpPort"] ?? "587";
            var user = section["Username"] ?? throw new InvalidOperationException("Email:Username is not configured.");
            var password = section["Password"] ?? throw new InvalidOperationException("Email:Password is not configured.");
            var from = section["From"] ?? user;
            var fromName = section["FromName"] ?? "FocusSpace";

            using var client = new SmtpClient(host, int.Parse(portStr))
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(from, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            _logger.LogInformation("Sending email to {To} — subject: {Subject}", to, subject);

            await client.SendMailAsync(message);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }

        // ──────────────────────────────────────────────────────────────
        // Named helpers
        // ──────────────────────────────────────────────────────────────

        public Task SendConfirmationEmailAsync(string to, string username, string confirmationLink) =>
            SendAsync(to, "Confirm your FocusSpace account", $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
                  <h2 style="color:#7c3aed">Welcome to FocusSpace, {username}!</h2>
                  <p>Thank you for registering. Please confirm your email address by clicking the button below.</p>
                  <p style="margin:30px 0">
                    <a href="{confirmationLink}"
                       style="background:#7c3aed;color:#fff;padding:14px 28px;border-radius:6px;text-decoration:none;font-weight:bold">
                      Confirm Email
                    </a>
                  </p>
                  <p style="color:#999;font-size:12px">
                    If you did not register on FocusSpace, you can safely ignore this email.
                  </p>
                </div>
                """);

        public Task SendPasswordResetEmailAsync(string to, string username, string resetLink) =>
            SendAsync(to, "Reset your FocusSpace password", $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
                  <h2 style="color:#7c3aed">Password Reset — FocusSpace</h2>
                  <p>Hi {username}, we received a request to reset your password.</p>
                  <p style="margin:30px 0">
                    <a href="{resetLink}"
                       style="background:#7c3aed;color:#fff;padding:14px 28px;border-radius:6px;text-decoration:none;font-weight:bold">
                      Reset Password
                    </a>
                  </p>
                  <p style="color:#999;font-size:12px">
                    This link expires in 2 hours. If you did not request a password reset, ignore this email.
                  </p>
                </div>
                """);
    }
}