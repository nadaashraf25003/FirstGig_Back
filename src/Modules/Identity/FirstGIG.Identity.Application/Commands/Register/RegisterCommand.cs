using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.Identity.Application.DTOs;
using FirstGIG.Identity.Domain.Enums;

namespace FirstGIG.Identity.Application.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    UserRole Role) : ICommand<AuthResponse>;
