namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ProductImplementation : IProductInterface
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

