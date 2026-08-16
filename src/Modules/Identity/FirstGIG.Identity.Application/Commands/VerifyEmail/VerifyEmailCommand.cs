using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.BuildingBlocks.Domain.Primitives;

namespace FirstGIG.Identity.Application.Commands.VerifyEmail;

public record VerifyEmailCommand(string Token) : ICommand;

public sealed class VerifyEmailHandler : ICommandHandler<VerifyEmailCommand>
{
    private readonly Domain.Repositories.IUserRepository _userRepository;
    private readonly Interfaces.IUnitOfWork _unitOfWork;

    public VerifyEmailHandler(
        Domain.Repositories.IUserRepository userRepository,
        Interfaces.IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByVerificationTokenAsync(command.Token, cancellationToken);
        if (user is null)
            return Result.Failure(Domain.Errors.IdentityErrors.InvalidVerificationToken);

        var result = user.VerifyEmail(command.Token);
        if (result.IsFailure)
            return result;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
