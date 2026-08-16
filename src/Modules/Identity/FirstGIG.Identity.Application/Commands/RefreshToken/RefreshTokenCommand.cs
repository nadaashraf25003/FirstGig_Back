using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.Identity.Application.DTOs;

namespace FirstGIG.Identity.Application.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string IpAddress) : ICommand<AuthResponse>;
