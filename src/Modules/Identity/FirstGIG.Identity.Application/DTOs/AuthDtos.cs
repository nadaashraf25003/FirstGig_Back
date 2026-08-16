using FirstGIG.Identity.Domain.Enums;

namespace FirstGIG.Identity.Application.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserRole Role,
    DateTime? EmailVerifiedAt,
    DateTime CreatedAt);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    UserDto User);
