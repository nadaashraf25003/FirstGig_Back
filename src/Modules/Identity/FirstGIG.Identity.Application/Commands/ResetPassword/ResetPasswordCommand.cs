using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.BuildingBlocks.Domain.Primitives;
using FirstGIG.Identity.Application.Interfaces;
using FirstGIG.Identity.Domain.Errors;
using FirstGIG.Identity.Domain.Repositories;

namespace FirstGIG.Identity.Application.Commands.ResetPassword;

public record ResetPasswordCommand(string Token, string NewPassword) : ICommand;

public sealed class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByResetTokenAsync(command.Token, cancellationToken);
        if (user is null)
            return Result.Failure(IdentityErrors.InvalidResetToken);

        var newPasswordHash = _passwordService.HashPassword(command.NewPassword);
        var result = user.ResetPassword(command.Token, newPasswordHash);

        if (result.IsFailure) return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
