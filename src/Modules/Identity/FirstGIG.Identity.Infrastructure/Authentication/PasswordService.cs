using FirstGIG.Identity.Application.Interfaces;

namespace FirstGIG.Identity.Infrastructure.Authentication;

public sealed class PasswordService : IPasswordService
{
    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool VerifyPassword(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
