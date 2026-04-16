using SECURA.Domain.Common;

namespace SECURA.Domain.ValueObjects;

/// <summary>
/// National Producer Number (NPN) — unique NIPR-assigned identifier for insurance producers.
/// </summary>
public sealed class NationalProducerNumber : ValueObject
{
    public string Value { get; }

    private NationalProducerNumber(string value)
    {
        Value = value;
    }

    public static NationalProducerNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("NPN cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length is < 1 or > 20)
            throw new DomainException("NPN must be between 1 and 20 characters.");

        if (!trimmed.All(char.IsDigit))
            throw new DomainException("NPN must contain only numeric characters.");

        return new NationalProducerNumber(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
