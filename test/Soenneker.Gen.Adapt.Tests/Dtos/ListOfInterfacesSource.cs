using System.Collections.Generic;
using Soenneker.Gen.Adapt.Tests.Dtos.Abstract;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class ListOfInterfacesSource
{
    public List<IPersonInterface> People { get; set; }
    public List<IProductInterface> Products { get; set; }
}

