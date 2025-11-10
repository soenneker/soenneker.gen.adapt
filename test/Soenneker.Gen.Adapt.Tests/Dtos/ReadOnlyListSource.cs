using System.Collections.Generic;
using Soenneker.Gen.Adapt.Tests.Dtos.Abstract;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ReadOnlyListSource
{
    public IReadOnlyList<IPersonInterface> People { get; set; }
    public IReadOnlyCollection<IProductInterface> Products { get; set; }
}

