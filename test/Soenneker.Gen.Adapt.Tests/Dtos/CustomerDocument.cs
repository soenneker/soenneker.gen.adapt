using Soenneker.Gen.Adapt.Tests.Dtos.Base;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class CustomerDocument : DocumentBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

