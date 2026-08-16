using AutoMapper;
using FirstGIG.BuildingBlocks.Application.Messaging;
using FirstGIG.BuildingBlocks.Domain.Primitives;
using FirstGIG.Identity.Application.DTOs;
using FirstGIG.Identity.Application.Interfaces;
using FirstGIG.Identity.Domain.Errors;
using FirstGIG.Identity.Domain.Repositories;

namespace FirstGIG.Identity.Application.Commands.RefreshToken;

public sealed class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RefreshTokenHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(command.Token, cancellationToken);
        if (user is null)
            return Result.Failure<AuthResponse>(IdentityErrors.InvalidRefreshToken);

        var rotateResult = user.RotateRefreshToken(command.Token, command.IpAddress);
        if (rotateResult.IsFailure)
            return Result.Failure<AuthResponse>(rotateResult.Error);

        var accessToken = _jwtService.GenerateAccessToken(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);

        return Result.Success(new AuthResponse(
            accessToken,
            rotateResult.Value.Token,
            _jwtService.ExpiresInSeconds,
            userDto));
    }
}
