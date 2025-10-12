namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ProductEntity : EntityBase
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

