namespace FirstGIG.BuildingBlocks.Application.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<Behaviors.ValidationError> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors.ToList();
    }

    public List<Behaviors.ValidationError> Errors { get; }
}
