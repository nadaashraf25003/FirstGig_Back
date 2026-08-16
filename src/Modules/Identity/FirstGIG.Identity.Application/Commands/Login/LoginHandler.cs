using AutoMapper;
using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.BuildingBlocks.Domain.Primitives;
using FirstGIG.Identity.Application.DTOs;
using FirstGIG.Identity.Application.Interfaces;
using FirstGIG.Identity.Domain.Enums;
using FirstGIG.Identity.Domain.Errors;
using FirstGIG.Identity.Domain.Repositories;

namespace FirstGIG.Identity.Application.Commands.Login;

public sealed class LoginHandler : ICommandHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        // 1. Find user (use generic error to not reveal if email exists)
        var user = await _userRepository.GetByEmailAsync(command.Email.ToLowerInvariant(), cancellationToken);
        if (user is null)
            return Result.Failure<AuthResponse>(IdentityErrors.InvalidCredentials);

        // 2. Verify password
        if (!_passwordService.VerifyPassword(command.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>(IdentityErrors.InvalidCredentials);

        // 3. Check account status
        if (user.Status == AccountStatus.Suspended)
            return Result.Failure<AuthResponse>(IdentityErrors.AccountSuspended);

        if (user.Status == AccountStatus.Deactivated)
            return Result.Failure<AuthResponse>(IdentityErrors.AccountDeactivated);

        if (user.EmailVerifiedAt is null)
            return Result.Failure<AuthResponse>(IdentityErrors.EmailNotVerified);

        // 4. Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = user.AddRefreshToken(command.IpAddress);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);

        return Result.Success(new AuthResponse(
            accessToken,
            refreshToken.Token,
            _jwtService.ExpiresInSeconds,
            userDto));
    }
}
