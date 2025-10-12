namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class OrderWithInterfaceSource
{
    public string OrderId { get; set; }
    public IProductInterface Product { get; set; }
    public IPersonInterface Customer { get; set; }
}

