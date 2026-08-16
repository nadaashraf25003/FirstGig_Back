using FirstGIG.BuildingBlocks.Domain.Primitives;

namespace FirstGIG.Identity.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>(Errors.IdentityErrors.EmailIsRequired);

        email = email.Trim().ToLowerInvariant();

        if (!IsValidEmail(email))
            return Result.Failure<Email>(Errors.IdentityErrors.EmailIsInvalid);

        return Result.Success(new Email(email));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
