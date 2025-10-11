using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class MultiListSource
{
    public List<int> Numbers { get; set; }
    public List<string> Tags { get; set; }
    public List<BasicSource> Items { get; set; }
}

