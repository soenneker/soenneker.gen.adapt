using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class MultiListDest
{
    public List<int> Numbers { get; set; }
    public List<string> Tags { get; set; }
    public List<BasicDest> Items { get; set; }
}

