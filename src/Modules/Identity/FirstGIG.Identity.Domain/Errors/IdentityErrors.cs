using FirstGIG.BuildingBlocks.Domain.Primitives;

namespace FirstGIG.Identity.Domain.Errors;

public static class IdentityErrors
{
    // Email errors
    public static readonly Error EmailIsRequired = new("Identity.Email.Required", "Email address is required.");
    public static readonly Error EmailIsInvalid = new("Identity.Email.Invalid", "Email address is not valid.");
    public static readonly Error EmailAlreadyInUse = new("Identity.Email.AlreadyInUse", "This email address is already registered.");
    public static readonly Error EmailNotVerified = new("Identity.Email.NotVerified", "Please verify your email address before logging in.");

    // Auth errors
    public static readonly Error InvalidCredentials = new("Identity.Auth.InvalidCredentials", "Email or password is incorrect.");
    public static readonly Error UserNotFound = new("Identity.User.NotFound", "User was not found.");
    public static readonly Error AccountSuspended = new("Identity.Account.Suspended", "Your account has been suspended.");
    public static readonly Error AccountDeactivated = new("Identity.Account.Deactivated", "Your account has been deactivated.");

    // Token errors
    public static readonly Error InvalidRefreshToken = new("Identity.Token.InvalidRefresh", "The refresh token is invalid or has expired.");
    public static readonly Error InvalidVerificationToken = new("Identity.Token.InvalidVerification", "The verification token is invalid or has expired.");
    public static readonly Error InvalidResetToken = new("Identity.Token.InvalidReset", "The password reset token is invalid or has expired.");

    // Password errors
    public static readonly Error PasswordTooWeak = new("Identity.Password.TooWeak", "Password must be at least 8 characters and include uppercase, lowercase, and a number.");
}
