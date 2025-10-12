using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ReadOnlyListSource
{
    public IReadOnlyList<IPersonInterface> People { get; set; }
    public IReadOnlyCollection<IProductInterface> Products { get; set; }
}

