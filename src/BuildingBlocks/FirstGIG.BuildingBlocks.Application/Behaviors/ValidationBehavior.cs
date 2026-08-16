using FluentValidation;
using MediatR;

namespace FirstGIG.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationErrors = _validators
            .Select(v => v.Validate(context))
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .Select(f => new ValidationError(f.PropertyName, f.ErrorMessage))
            .ToList();

        if (validationErrors.Count != 0)
            throw new Exceptions.ValidationException(validationErrors);

        return await next();
    }
}

public record ValidationError(string PropertyName, string ErrorMessage);
