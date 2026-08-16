using FirstGIG.BuildingBlocks.Domain.Primitives;

namespace FirstGIG.Identity.Domain.Entities;

public sealed class RefreshToken : Entity
{
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }

    // EF Core constructor
    private RefreshToken() { }

    private RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt, string createdByIp)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
    }

    public static RefreshToken Create(Guid userId, string createdByIp)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            GenerateToken(),
            DateTime.UtcNow.AddDays(7),
            createdByIp);
    }

    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;

    public void Revoke() => RevokedAt = DateTime.UtcNow;

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
