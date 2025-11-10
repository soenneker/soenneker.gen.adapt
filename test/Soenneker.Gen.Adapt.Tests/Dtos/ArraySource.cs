using Soenneker.Gen.Adapt.Tests.Dtos.Abstract;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ArraySource
{
    public string[] Names { get; set; }
    public int[] Numbers { get; set; }
    public IPersonInterface[] People { get; set; }
}

