using SECURA.Domain.Common;

namespace SECURA.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string State { get; }
    public string Zip { get; }
    public string Country { get; }

    private Address(string line1, string? line2, string city, string state, string zip, string country)
    {
        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        Zip = zip;
        Country = country;
    }

    public static Address Create(string line1, string? line2, string city, string state, string zip, string country = "US")
    {
        if (string.IsNullOrWhiteSpace(line1)) throw new DomainException("Address line 1 is required.");
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("City is required.");
        if (string.IsNullOrWhiteSpace(state)) throw new DomainException("State is required.");
        if (string.IsNullOrWhiteSpace(zip)) throw new DomainException("ZIP is required.");

        return new Address(line1.Trim(), line2?.Trim(), city.Trim(), state.Trim().ToUpperInvariant(), zip.Trim(), country.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return State;
        yield return Zip;
        yield return Country;
    }
}
