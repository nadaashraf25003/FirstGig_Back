using FirstGIG.Identity.Domain.Entities;

namespace FirstGIG.Identity.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    int ExpiresInSeconds { get; }
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IEmailService
{
    Task SendEmailVerificationAsync(string toEmail, string firstName, string verificationToken, CancellationToken ct = default);
    Task SendPasswordResetAsync(string toEmail, string firstName, string resetToken, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
