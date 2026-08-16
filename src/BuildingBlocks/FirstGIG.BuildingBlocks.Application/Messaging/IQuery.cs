using FirstGIG.BuildingBlocks.Domain.Primitives;
using MediatR;

namespace FirstGIG.BuildingBlocks.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
