using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.BuildingBlocks.Domain.Primitives;
using FirstGIG.Identity.Application.Interfaces;
using FirstGIG.Identity.Domain.Errors;
using FirstGIG.Identity.Domain.Repositories;

namespace FirstGIG.Identity.Application.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand;

public sealed class ForgotPasswordHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email.ToLowerInvariant(), cancellationToken);

        // Always return success to prevent email enumeration attacks
        if (user is null) return Result.Success();

        user.GeneratePasswordResetToken();
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordResetAsync(
            user.Email.Value,
            user.FirstName,
            user.PasswordResetToken!,
            cancellationToken);

        return Result.Success();
    }
}
