using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.Identity.Application.DTOs;

namespace FirstGIG.Identity.Application.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    string IpAddress) : ICommand<AuthResponse>;
