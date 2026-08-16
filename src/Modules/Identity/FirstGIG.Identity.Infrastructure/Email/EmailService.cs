using FirstGIG.Identity.Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FirstGIG.Identity.Infrastructure.Email;

public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = "FirstGIG";
    public string FrontendBaseUrl { get; init; } = "http://localhost:3000";
}

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailVerificationAsync(
        string toEmail, string firstName, string verificationToken, CancellationToken ct = default)
    {
        var verificationUrl = $"{_settings.FrontendBaseUrl}/verify-email?token={verificationToken}";

        _logger.LogInformation(
            "\n═══════════════════════════════════════════════════════════════════════\n" +
            "📧 [DEV EMAIL - VERIFICATION]\n" +
            "To: {ToEmail} ({FirstName})\n" +
            "Verification Token: {Token}\n" +
            "Link: {VerificationUrl}\n" +
            "═══════════════════════════════════════════════════════════════════════",
            toEmail, firstName, verificationToken, verificationUrl);

        var body = $"""
            <h2>Welcome to FirstGIG, {firstName}!</h2>
            <p>Please verify your email address by clicking the link below:</p>
            <p><a href="{verificationUrl}" style="background:#4F46E5;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Verify Email</a></p>
            <p>This link expires in 24 hours.</p>
            <p>If you didn't create an account, you can safely ignore this email.</p>
            """;

        await SendEmailSafelyAsync(toEmail, "Verify your FirstGIG email", body, ct);
    }

    public async Task SendPasswordResetAsync(
        string toEmail, string firstName, string resetToken, CancellationToken ct = default)
    {
        var resetUrl = $"{_settings.FrontendBaseUrl}/reset-password?token={resetToken}";

        _logger.LogInformation(
            "\n═══════════════════════════════════════════════════════════════════════\n" +
            "📧 [DEV EMAIL - PASSWORD RESET]\n" +
            "To: {ToEmail} ({FirstName})\n" +
            "Reset Token: {Token}\n" +
            "Link: {ResetUrl}\n" +
            "═══════════════════════════════════════════════════════════════════════",
            toEmail, firstName, resetToken, resetUrl);

        var body = $"""
            <h2>Hi {firstName},</h2>
            <p>You requested to reset your password. Click the link below:</p>
            <p><a href="{resetUrl}" style="background:#4F46E5;color:white;padding:12px 24px;border-radius:6px;text-decoration:none;">Reset Password</a></p>
            <p>This link expires in 1 hour.</p>
            <p>If you didn't request a password reset, you can safely ignore this email.</p>
            """;

        await SendEmailSafelyAsync(toEmail, "Reset your FirstGIG password", body, ct);
    }

    private async Task SendEmailSafelyAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        // Skip actual SMTP dispatch if placeholder credentials are used
        if (string.IsNullOrWhiteSpace(_settings.Username) || 
            _settings.Username.Contains("your-email") || 
            string.IsNullOrWhiteSpace(_settings.Password))
        {
            _logger.LogInformation("SMTP credentials not configured — email logged to console above.");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(new MailboxAddress(string.Empty, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send email via SMTP, but token is logged above for dev testing.");
        }
    }
}
