using Soenneker.Gen.Adapt.Tests.Dtos.Base;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ProductDocument : DocumentBase
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

