using FirstGIG.BuildingBlocks.Domain.Primitives;
using FirstGIG.Identity.Domain.Enums;
using FirstGIG.Identity.Domain.ValueObjects;

namespace FirstGIG.Identity.Domain.Entities;

public sealed class User : AggregateRoot
{
    private readonly List<RefreshToken> _refreshTokens = [];

    // EF Core constructor
    private User() { }

    private User(
        Guid id,
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role) : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        Status = AccountStatus.Pending;
        EmailVerificationToken = GenerateSecureToken();
        EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
    }

    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public AccountStatus Status { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public string FullName => $"{FirstName} {LastName}";

    public static User Create(
        Email email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role)
    {
        var user = new User(Guid.NewGuid(), email, passwordHash, firstName, lastName, role);
        return user;
    }

    public Result VerifyEmail(string token)
    {
        if (EmailVerifiedAt is not null)
            return Result.Success();

        if (EmailVerificationToken != token || DateTime.UtcNow > EmailVerificationTokenExpiresAt)
            return Result.Failure(Errors.IdentityErrors.InvalidVerificationToken);

        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
        Status = AccountStatus.Active;

        return Result.Success();
    }

    public Result GeneratePasswordResetToken()
    {
        PasswordResetToken = GenerateSecureToken();
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        return Result.Success();
    }

    public Result ResetPassword(string token, string newPasswordHash)
    {
        if (PasswordResetToken != token || DateTime.UtcNow > PasswordResetTokenExpiresAt)
            return Result.Failure(Errors.IdentityErrors.InvalidResetToken);

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;

        // Revoke all existing refresh tokens on password change
        foreach (var rt in _refreshTokens.Where(r => r.IsActive))
            rt.Revoke();

        return Result.Success();
    }

    public RefreshToken AddRefreshToken(string ipAddress)
    {
        // Clean up old tokens — keep only last 5 active
        var oldTokens = _refreshTokens.Where(r => !r.IsActive).ToList();
        foreach (var old in oldTokens)
            _refreshTokens.Remove(old);

        var token = RefreshToken.Create(Id, ipAddress);
        _refreshTokens.Add(token);
        return token;
    }

    public Result<RefreshToken> RotateRefreshToken(string token, string ipAddress)
    {
        var existingToken = _refreshTokens.SingleOrDefault(t => t.Token == token);

        if (existingToken is null || !existingToken.IsActive)
            return Result.Failure<RefreshToken>(Errors.IdentityErrors.InvalidRefreshToken);

        existingToken.Revoke();
        var newToken = AddRefreshToken(ipAddress);
        return Result.Success(newToken);
    }

    public bool IsActive => Status == AccountStatus.Active;

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
