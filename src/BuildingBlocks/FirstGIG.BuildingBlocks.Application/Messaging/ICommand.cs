using FirstGIG.BuildingBlocks.Domain.Primitives;
using MediatR;

namespace FirstGIG.BuildingBlocks.Application.Messaging;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }
