namespace Soenneker.Gen.Adapt.Tests.Dtos;

public record PersonRecord
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public int Age { get; init; }
}



