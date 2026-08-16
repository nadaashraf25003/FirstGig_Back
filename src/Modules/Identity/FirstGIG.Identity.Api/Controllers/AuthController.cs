using FirstGIG.BuildingBlocks.Domain.Primitives;
using FirstGIG.Identity.Application.Commands.ForgotPassword;
using FirstGIG.Identity.Application.Commands.Login;
using FirstGIG.Identity.Application.Commands.RefreshToken;
using FirstGIG.Identity.Application.Commands.Register;
using FirstGIG.Identity.Application.Commands.ResetPassword;
using FirstGIG.Identity.Application.Commands.VerifyEmail;
using FirstGIG.Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FirstGIG.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Register a new user (Freelancer or Client)</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Register), result.Value)
            : Problem(result.Error);
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new LoginCommand(request.Email, request.Password, ipAddress);
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    /// <summary>Refresh access token using a refresh token</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new RefreshTokenCommand(request.Token, ipAddress);
        var result = await _sender.Send(command, ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    /// <summary>Verify email address using the token from the verification email</summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new VerifyEmailCommand(request.Token), ct);
        return result.IsSuccess ? Ok(new { message = "Email verified successfully." }) : Problem(result.Error);
    }

    /// <summary>Request a password reset email</summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        await _sender.Send(new ForgotPasswordCommand(request.Email), ct);
        // Always 200 to prevent email enumeration
        return Ok(new { message = "If this email is registered, you will receive a password reset link." });
    }

    /// <summary>Reset password using the token from the reset email</summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new ResetPasswordCommand(request.Token, request.NewPassword), ct);
        return result.IsSuccess ? Ok(new { message = "Password reset successfully." }) : Problem(result.Error);
    }

    // Helper to convert domain Error to RFC 7807 ProblemDetails
    private IActionResult Problem(Error error) =>
        Problem(
            title: error.Code,
            detail: error.Description,
            statusCode: StatusCodes.Status400BadRequest);
}

// Request DTOs (separate from commands to keep IP injection in controller)
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string Token);
public record VerifyEmailRequest(string Token);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
