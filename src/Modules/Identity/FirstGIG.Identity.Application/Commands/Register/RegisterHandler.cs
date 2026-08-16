using AutoMapper;
using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.BuildingBlocks.Domain.Primitives;
using FirstGIG.Identity.Application.DTOs;
using FirstGIG.Identity.Application.Interfaces;
using FirstGIG.Identity.Domain.Entities;
using FirstGIG.Identity.Domain.Errors;
using FirstGIG.Identity.Domain.Repositories;
using FirstGIG.Identity.Domain.ValueObjects;

namespace FirstGIG.Identity.Application.Commands.Register;

public sealed class RegisterHandler : ICommandHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterHandler(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IJwtService jwtService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        // 1. Check if email already exists
        var emailExists = await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken);
        if (emailExists)
            return Result.Failure<AuthResponse>(IdentityErrors.EmailAlreadyInUse);

        // 2. Create Email value object
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
            return Result.Failure<AuthResponse>(emailResult.Error);

        // 3. Hash password
        var passwordHash = _passwordService.HashPassword(command.Password);

        // 4. Create user aggregate
        var user = User.Create(
            emailResult.Value,
            passwordHash,
            command.FirstName,
            command.LastName,
            command.Role);

        // 5. Add initial refresh token
        var refreshToken = user.AddRefreshToken("system");

        // 6. Persist user and token atomically
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Send verification email (logged to console in dev)
        await _emailService.SendEmailVerificationAsync(
            user.Email.Value,
            user.FirstName,
            user.EmailVerificationToken!,
            cancellationToken);

        // 8. Generate access token
        var accessToken = _jwtService.GenerateAccessToken(user);
        var userDto = _mapper.Map<UserDto>(user);

        return Result.Success(new AuthResponse(
            accessToken,
            refreshToken.Token,
            _jwtService.ExpiresInSeconds,
            userDto));
    }
}
